import logging
from typing import Any, Dict, Optional
import jupyter_client
import uuid

class AsyncKernelHandler():

    _instance: Optional['AsyncKernelHandler'] = None
    _client: Optional[jupyter_client.AsyncKernelClient] = None

    def __new__(cls, *args, **kwargs):
        if cls._instance is None:
            cls._instance = super().__new__(cls)
            cls._kernel = jupyter_client.AsyncKernelManager()
        return cls._instance

    async def start_kernel(self):
        logging.info("Starting the kernel")
        await self._kernel.start_kernel()
        logging.info("Kernel started")
        client = self._kernel.client()
        assert isinstance(client, jupyter_client.AsyncKernelClient)
        self._client = client

    async def execute_script(self, script: str, silent: bool = False):
        if self._client is None:
            logging.error("Kernel not started")
            raise RuntimeError("Kernel not started")
        logging.info("Executing script")
        return self._client.execute(script, silent, allow_stdin=True)

    async def comm_open(self, target_name: str, data: Dict[str, Any]):
        if self._client is None:
            logging.error("Kernel not started")
            raise RuntimeError("Kernel not started")
        ret = str(uuid.uuid1())
        msg = self._client.session.msg(
            'comm_open', {
                'comm_id': ret,
                'target_name': target_name,
                'data': data
            }
        )
        logging.debug(f"Opening {ret} comm")
        self._client.shell_channel.send(msg)
        return ret

    async def comm_msg(self, uuid: str, data: Dict[str, Any]):
        if self._client is None:
            logging.error("Kernel not started")
            raise RuntimeError("Kernel not started")
        msg = self._client.session.msg(
            'comm_msg', {
                'comm_id': uuid,
                'data': data
            }
        )
        logging.debug(f"Sending {data} to {uuid}")
        self._client.shell_channel.send(msg)

    async def comm_close(self, uuid: str, data: Dict[str, Any] = {}):
        if self._client is None:
            logging.error("Kernel not started")
            raise RuntimeError("Kernel not started")
        msg = self._client.session.msg(
            'comm_close', {
                'comm_id': uuid,
                'data': {}
            }
        )
        logging.debug(f"Sending comm_close message to {uuid}")
        self._client.shell_channel.send(msg)

    async def interrupt_kernel(self):
        logging.info("Attempting to interrupt the kernel")
        await self._kernel.interrupt_kernel()
        logging.info("Kernel interrupted")

    async def restart_kernel(self):
        logging.info("Shutting down kernel")
        self._kernel.request_shutdown(True)

    async def recv_kernel_msg(self):
        if self._client is None:
            logging.error("Kernel not started")
            raise RuntimeError("Kernel not started")
        logging.debug("Receiving kernel message")
        ret = await self._client.get_iopub_msg()
        logging.debug(ret)
        return ret

    async def recv_stdin_msg(self):
        if self._client is None:
            logging.error("Kernel not started")
            raise RuntimeError("Kernel not started")
        logging.debug("Receiving kernel message")
        ret = await self._client.get_stdin_msg()
        logging.debug(ret)
        return ret

    async def clear_kernel_msg(self):
        if self._client is None:
            logging.error("Kernel not started")
            raise RuntimeError("Kernel not started")
        ret = await self._client.iopub_channel.get_msgs()
        logging.debug(ret)
        return ret

    async def input(self, content: str):
        if self._client is None:
            logging.error("Kernel not started")
            raise RuntimeError("Kernel not started")
        logging.debug(content)
        self._client.input(content)
