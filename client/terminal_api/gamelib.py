from __future__ import annotations

import itertools
from typing import Any

from .. import meta, model, protocol, xml_, utils, network


class MessageHandler(network.SocketHandler):

    def __init__(self, host: str, port: int):
        super().__init__(host=host, port=port)

    def send_msg(self, msg: model.MessageHeader):
        encoder = xml_.XMLSerializer()
        self.send(str(encoder.export(msg)))

    def recv_msg(self) -> model.MessageHeader:
        decoder = xml_.XMLDeserializer()
        content = self.recv()
        msg = decoder.deserialize(xml_.parse(content))
        assert isinstance(msg, model.MessageHeader)
        return msg

    def compose_msg(self, name: str, payload: Any = None, request_id: int = 0):
        msg = model.MessageHeader(
            messageType=name,
            payload=payload,
            requestId=request_id
        )
        self.send_msg(msg)

class Game(model.GameABC, metaclass=meta.Singleton):

    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
        self._counter = itertools.count(1, 1)

    @property
    def async_mode(self) -> bool:
        return True
    
    def configure(self, host: str, port: int):
        self._handler = MessageHandler(host, port)
        self._handler.accept()

    def _remote_call(self,
        contract: protocol.Contract, message: Any
    ) -> Any:
        self._handler.compose_msg(
            name=f"c_callfunc_{contract.name}",
            payload=message,
            request_id=next(self._counter)
        )
        if self.async_mode and contract.can_run_async:
            return
        
        ret_msg = self._handler.recv_msg()
        if ret_msg['messageType'] == "s_exception":
            raise model.RemoteException(str(ret_msg['payload']))
        return ret_msg['payload']
