from typing import Callable, Generic, List, Type, TypeVar

from .. import header, objects

ST = TypeVar('ST', bound=objects.EntityObject)
DT = TypeVar('DT', bound=header.BaseMessage)

class GameObjectIterator(Generic[ST, DT]):

    def __init__(self, shell_type: Type[ST], data_type: Type[DT], fetch: Callable[[int], List[DT]]):
        self._shell_type = shell_type
        self._data_type = data_type
        self._fetch = fetch
        self._idx = 0
        self._cache: List[ST] = []

    def _feed(self) -> None:
        for _ in self._fetch(self._idx + 1):
            self._cache.append(self._shell_type.from_message(_))

    def __iter__(self):
        return self

    def __next__(self) -> ST:
        if not self._cache:
            self._feed()
        if not self._cache:
            raise StopIteration
        ret = self._cache.pop(0)
        self._idx = ret.id_
        return ret

    def reset(self) -> None:
        self._idx = 0
