import io
import logging
import sys
from typing import Any, Iterator, List, Optional

import ipykernel.comm

from .. import meta, model, xml_


class RuntimeEnviron(metaclass=meta.Singleton):

    _content: List[Any]

    def __init__(self, *content: Any):
        self._content = list(content)

    def __getitem__(self, index: int) -> Any:
        return self._content[index]

    def __iter__(self) -> Iterator[Any]:
        return iter(self._content)

    def update(self, *content: Any) -> None:
        self._content = list(content)

    def __len__(self) -> int:
        return len(self._content)


class Clipboard(RuntimeEnviron):
    pass


class RuntimePath(RuntimeEnviron):

    _original_path: List[str] = sys.path

    def apply(self):
        self._original_path = sys.path
        sys.path.extend(self)

    def close(self):
        sys.path = self._original_path


class RuntimeLogger(metaclass=meta.Singleton):

    _channel: ipykernel.comm.Comm
    _stream: io.StringIO

    def __init__(self, channel: Optional[ipykernel.comm.Comm] = None):
        if channel is not None:
            self._channel = channel

    def log(self, level: int, msg: str):
        self._channel.send({'level': level, 'msg': msg})

    def debug(self, msg: str):
        self.log(logging.DEBUG, msg)

    def info(self, msg: str):
        self.log(logging.INFO, msg)

    def warning(self, msg: str):
        self.log(logging.WARNING, msg)

    def error(self, msg: str):
        self.log(logging.ERROR, msg)

    def critical(self, msg: str):
        self.log(logging.CRITICAL, msg)



class RuntimeEnvironHandler(metaclass=meta.Singleton):

    def handle(self, msg: str):
        decoder = xml_.XMLDeserializer()
        msg_content = decoder.deserialize(xml_.parse(msg))
        assert isinstance(msg_content, model.RunScriptMessage)
        Clipboard().update(*msg_content['clipboard'])
        RuntimePath().update(*msg_content['searchPaths'])


def log(level: int, msg: str):
    RuntimeLogger().log(level, msg)


def debug(msg: str):
    RuntimeLogger().debug(msg)


def info(msg: str):
    RuntimeLogger().info(msg)


def warning(msg: str):
    RuntimeLogger().warning(msg)


def error(msg: str):
    RuntimeLogger().error(msg)


def critical(msg: str):
    RuntimeLogger().critical(msg)

