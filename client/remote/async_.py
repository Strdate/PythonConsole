import asyncio
import itertools
import json
import logging
import re
from typing import Any, Callable, Dict, Optional

from .. import model, network, protocol, utils, xml_
from . import comm, kernel

_CALLBACK = Callable[['asyncio.Future[Any]'], None]

class AsyncMessageHandler(network.AsyncSocketHandler):

    def __init__(self, host: str, port: int):
        super().__init__(host=host, port=port)

    async def send_msg(self, msg: model.MessageHeader):
        encoder = xml_.XMLSerializer()
        await self.send(str(encoder.export(msg)))

    async def recv_msg(self) -> model.MessageHeader:
        decoder = xml_.XMLDeserializer()
        content = await self.recv()
        msg = decoder.deserialize(xml_.parse(content))
        assert isinstance(msg, model.MessageHeader)
        return msg

    async def compose_msg(self, name: str, payload: Any = None, request_id: int = 0):
        msg = model.MessageHeader(
            messageType=name,
            payload=payload,
            requestId=request_id
        )
        await self.send_msg(msg)


class AsyncServerHandler(AsyncMessageHandler, kernel.AsyncKernelHandler):

    _callback: Dict[int, 'asyncio.Future[Any]']

    def __init__(self, host: str, port: int):
        super().__init__(host=host, port=port)
        self._callback = {}
        self._counter = itertools.count(1, 1)
        self._encoder = xml_.XMLSerializer()
        self._decoder = xml_.XMLDeserializer()
        self._runtime_comm = ''

        self._log_comm = comm.AsyncCommManager('log')
        @self._log_comm.on_msg
        async def log_msg(comm_id: str, msg: Dict[str, Any]):
            logging.log(**msg['data'])

    async def start(self):
        await asyncio.gather(
            self.handle_network(),
            self.handle_kernel(),
            self.handle_input()
        )

    async def handle_network(self):
        await self.accept()
        while True:
            message = await self.recv_msg()
            await self.dispatch_network(message)

    async def handle_kernel(self):
        await self.start_kernel()
        await asyncio.sleep(1)
        await self.execute_script('from client.kernel_api import *', True)
        self._runtime_comm = await self.comm_open('runtime', {})
        while True:
            message = await self.recv_kernel_msg()
            await self.dispatch_kernel(message)

    async def handle_input(self):
        while True:
            message = await self.recv_stdin_msg()
            await self.dispatch_input(message)

    async def dispatch_network(self, msg: model.MessageHeader):
        return await self._dispatch_network(**msg.content)

    @utils.dispatch
    async def _dispatch_network(self, messageType: str, requestId: int = 0, payload: Any = None):
        if requestId:
            payload = payload
            if messageType == "s_exception":
                self._callback[requestId].set_exception(
                    model.RemoteException(str(payload)))
            else:
                self._callback[requestId].set_result(payload)
        else:
            logging.warn(f"Received unknown message: '{messageType}'")

    @_dispatch_network.register(messageType='s_script_run')
    async def _run_script(self, messageType: str, requestId: int = 0, payload: Any = None):
        assert isinstance(payload, model.RunScriptMessage)
        encoder = xml_.XMLSerializer()
        await self.comm_msg(self._runtime_comm, {'data': str(encoder.export(payload))})
        await self.execute_script(payload['script'])

    @_dispatch_network.register(messageType='s_script_abort')
    async def _abort(self, messageType: str, requestId: int = 0, payload: Any = None):
        await self.interrupt_kernel()
        ret_msg = model.MessageHeader(
            messageType='c_ready', requestId=0, payload=None
        )
        await self.send_msg(ret_msg)

    @_dispatch_network.register(messageType='s_exception', requestId=0)
    async def _exception(self, messageType: str, requestId: int = 0, payload: Any = None):
        pass

    async def dispatch_kernel(self, msg: Dict[str, Any]):
        return await self._dispatch_kernel(msg['header']['msg_type'], msg)

    @utils.dispatch
    async def _dispatch_kernel(self, messageType: str, msg: Dict[str, Any]):
        pass

    @_dispatch_kernel.register(messageType='stream')
    async def _stream_result(self, messageType: str, msg: Dict[str, Any]):
        ret_msg = model.MessageHeader(
            payload=msg['content']['text'],
            requestId=0, messageType='c_output_message'
        )
        await self.send_msg(ret_msg)

    @_dispatch_kernel.register(messageType='execute_result')
    async def _execute_result(self, messageType: str, msg: Dict[str, Any]):
        ret_msg = model.MessageHeader(
            payload=msg['content']['data']['text/plain'] + '\n',
            requestId=0, messageType='c_output_message'
        )
        await self.send_msg(ret_msg)
        
    @_dispatch_kernel.register(messageType='error')
    async def _error_result(self, messageType: str, msg: Dict[str, Any]):
        traceback = '\n'.join(
            re.sub(r'\x1b\[(?:.*?)m', '', _)
            for _ in msg['content']['traceback']
        )
        ret_msg = model.MessageHeader(
            payload=traceback,
            requestId=0, messageType='c_output_message'
        )
        await self.send_msg(ret_msg)

    @_dispatch_kernel.register(messageType='status')
    async def _kerenl_status(self, messageType: str, msg: Dict[str, Any]):
        if msg['content']['execution_state'] == 'idle':
            if msg['parent_header']['msg_type'] == 'execute_request':
                ret_msg = model.MessageHeader(
                    payload=None,
                    requestId=0, messageType='c_script_end'
                )
                await self.send_msg(ret_msg)

    @_dispatch_kernel.register(messageType='comm_open')
    async def _comm_open(self, messageType: str, msg: Dict[str, Any]):
        try:
            await comm.AsyncCommManager().handle_message('open', msg['content'])
        except RuntimeError:
            await self.comm_close(msg['comm_id'])

    @_dispatch_kernel.register(messageType='comm_msg')
    async def _comm_msg(self, messageType: str, msg: Dict[str, Any]):
        try:
            await comm.AsyncCommManager().handle_message('msg', msg['content'])
        except RuntimeError:
            await self.comm_close(msg['comm_id'])

    @_dispatch_kernel.register(messageType='comm_close')
    async def _comm_close(self, messageType: str, msg: Dict[str, Any]):
        await comm.AsyncCommManager().handle_message('close', msg['content'])

    async def dispatch_input(self, msg: Dict[str, Any]):
        return await self._dispatch_input(msg['header']['msg_type'], msg)

    @utils.dispatch
    async def _dispatch_input(self, messageType: str, msg: Dict[str, Any]):
        pass

    @_dispatch_input.register(messageType='input_request')
    async def _provide_input(self, messageType: str, msg: Dict[str, Any]):
        try:
            if not msg['content']['prompt']:
                raise Exception()
            data = json.loads(msg['content']['prompt'])
            contract = protocol.Contract(**data['contract'])
            if data['builtin']:
                message = data['message']
            else:
                message = self._decoder.deserialize(
                    xml_.parse(data['message'])
                    )
            if data['async']:
                await self.input('')

            def remote_callback(future: 'asyncio.Future[Any]'):
                loop = asyncio.get_event_loop()
                try:
                    loop.create_task(self.input(
                        str(self._encoder.export(future.result()))
                    ))
                except model.RemoteException as e:
                    loop.create_task(self.input(
                        str(self._encoder.export(e))
                    ))

            await self.remote_call(contract, message, remote_callback)
        except Exception:
            await self.input('')

    async def wait_for(self, request_id: int):
        if request_id not in self._callback:
            raise KeyError("Corresponding execution not found")
        ret = await self._callback[request_id]
        self._callback.pop(request_id)
        return ret

    async def remote_call(self,
        contract: protocol.Contract, param: Any,
        callback: Optional[_CALLBACK] = None
    ):
        request_id = next(self._counter)
        loop = asyncio.get_event_loop()
        self._callback[request_id] = loop.create_future()
        if callback is not None:
            self._callback[request_id].add_done_callback(callback)
        await self.compose_msg(
            f"c_callfunc_{contract.name}",
            param, request_id
        )
        return request_id

    async def remote_call_blocking(self,
        contract: protocol.Contract, param: Any,
        callback: Optional[_CALLBACK] = None
    ):
        request_id = await self.remote_call(contract, param, callback)
        return await self.wait_for(request_id)
