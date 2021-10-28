import functools
import inspect
import types
from typing import TYPE_CHECKING, Any, Callable, Dict, List, Tuple

_REGISTERED = Callable[..., Any]
_DISPATCH = Callable[..., bool]
_DECORATOR = Callable[[_REGISTERED], _REGISTERED]

class _EqualsAll():

    def __eq__(self, o: Any) -> bool:
        return True


class _NotEqualsAll():

    def __eq__(self, o: Any) -> bool:
        return False


SKIP = _EqualsAll()
NOT_EQUALS_ALL = _NotEqualsAll()

class Dispatcher():

    def __init__(self, func: _REGISTERED):
        functools.wraps(func)(self)
        self._func = func
        self._dispatched: List[Tuple[_DISPATCH, _REGISTERED]] = []

    def __call__(self, *args: Any, **kwargs: Any):
        for dispatch, target in self._dispatched:
            if dispatch(*args, **kwargs):
                return target(*args, **kwargs)
        return self._func(*args, **kwargs)

    def __get__(self, instance, owner):
        if instance is None:
            return self
        return types.MethodType(self, instance)

    def register(self, *args: Any, **kwargs: Any) -> _DECORATOR:

        def wrapper(func: _REGISTERED) -> _REGISTERED:
            sig = inspect.signature(func)
            bound_args = sig.bind_partial(*args, **kwargs)

            def _dispatch(*args, **kwargs):
                call_args = sig.bind(*args, **kwargs)
                for k, v in bound_args.arguments.items():
                    if v != call_args.arguments.get(k, NOT_EQUALS_ALL):
                        return False
                
                return True

            self._dispatched.append(
                (_dispatch, func)
            )

            return func

        return wrapper

dispatch = Dispatcher

__all__ = ['dispatch', 'SKIP']
