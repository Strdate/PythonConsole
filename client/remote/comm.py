import inspect
from typing import Any, Callable, Dict, Optional, Union, overload

from typing_extensions import Literal

_CALLBACK = Callable[[str, Dict[str, Any]], Any]
_MSG_TYPE = Union[Literal['open'], Literal['close'], Literal['msg'], str]

class AsyncCommTarget():

    def __init__(self,
        target_name: str, on_open: Optional[_CALLBACK] = None,
        on_msg: Optional[_CALLBACK] = None, on_close: Optional[_CALLBACK] = None
    ):
        self.target_name = target_name

        def default_handler(comm_id: str, msg: Dict[str, Any]) -> Any:
            return msg

        self._on_open = on_open if on_open is not None else default_handler
        self._on_msg = on_msg if on_msg is not None else default_handler
        self._on_close = on_close if on_close is not None else default_handler

        self._comm_id = None

    @property
    def comm_id(self) -> str:
        if self._comm_id is None:
            raise ValueError("Comm id not set")
        return self._comm_id

    def on_msg(self, func: _CALLBACK) -> _CALLBACK:
        self._on_msg = func
        return func

    def on_close(self, func: _CALLBACK) -> _CALLBACK:
        self._on_close = func
        return func

    def on_open(self, func: _CALLBACK) -> _CALLBACK:
        self._on_open = func
        return func

    async def __call__(self,
        comm_id: str, msg_type: _MSG_TYPE,
        message: Dict[str, Any]
    ) -> Any:
        if msg_type == 'open':
            ret = self._on_open(comm_id, message)
            self._comm_id = message['comm_id']
        elif msg_type == 'close':
            ret = self._on_close(comm_id, message)
        elif msg_type == 'msg':
            ret = self._on_msg(comm_id, message)
            self._comm_id = None
        else:
            raise ValueError("Unknown message type")
        if inspect.isawaitable(ret):
            return await ret
        return ret


class AsyncCommManager():

    _instance: Optional['AsyncCommManager'] = None
    _targets: Dict[str, AsyncCommTarget] = {}
    _comms: Dict[str, str] = {}

    @overload
    def __new__(cls, target_name: str) -> AsyncCommTarget:
        ...

    @overload
    def __new__(cls, target_name: None = None) -> 'AsyncCommManager':
        ...

    def __new__(cls, target_name: Optional[str] = None):
        if cls._instance is None:
            cls._instance = super().__new__(cls)
        if target_name is None:
            return cls._instance
        if target_name not in cls._targets:
            cls._targets[target_name] = AsyncCommTarget(target_name)

        return cls._targets[target_name]

    async def handle_message(self, msg_type: _MSG_TYPE, message: Dict[str, Any]) -> Any:
        if msg_type == 'open':
            target_name = message['target_name']
            comm_id = message['comm_id']
            try:
                comm_target = self._targets[target_name]
            except KeyError as e:
                raise RuntimeError(f"Unregistered target {target_name}") from e

            self._comms[comm_id] = target_name
        else:
            comm_id = message['comm_id']
            try:
                target_name = self._comms[comm_id]
            except KeyError as e:
                raise RuntimeError(f"Unknown comm_id {comm_id}") from e
            
            comm_target = self._targets[target_name]
        
        return await comm_target(comm_id, msg_type, message)
