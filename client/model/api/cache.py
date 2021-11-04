import abc
import weakref
from typing import NamedTuple, Optional

from ... import meta

CACHE_CONTAINER = weakref.WeakValueDictionary

class CacheKey(NamedTuple):
    type_: str
    id_: int = 0
    idString: Optional[str] = None


class CachedObject():

    _out_dated: bool = False

    @property
    def out_dated(self) -> bool:
        return self._out_dated

    @out_dated.setter
    def out_dated(self, value: bool):
        self._out_dated = value

    @property
    @abc.abstractmethod
    def _cache_key(self) -> NamedTuple:
        ...

class CacheManager(metaclass=meta.Singleton):
    
    def __init__(self):
        self._cache: CACHE_CONTAINER[NamedTuple, CachedObject] \
            = weakref.WeakValueDictionary()

    def __len__(self):
        return len(self._cache)

    def __getitem__(self, key: NamedTuple) -> Optional[CachedObject]:
        ret = self._cache.get(key, None)
        if ret is not None and ret.out_dated:
            self._cache.pop(key)
            ret = None
        return ret

    def __setitem__(self, key: NamedTuple, obj: CachedObject):
        self._cache[key] = obj
