from __future__ import annotations

from typing import TYPE_CHECKING, Any, Dict

import ipykernel.comm

from . import runtime, gamelib
from .runtime import debug, info, warning, error, critical
from ..model import Vector, Bezier

if TYPE_CHECKING:
    import ipykernel.zmqshell

    def get_ipython() -> ipykernel.zmqshell.ZMQInteractiveShell:
        ...

def runtime_wrapper(comm: ipykernel.comm.Comm, open_msg: Dict[str, Any]):
    
    @comm.on_msg
    def update_runtime(msg: Dict[str, Any]):
        runtime.RuntimeEnvironHandler().handle(msg['content']['data']['data'])

def pre_execute() -> None:
    runtime.RuntimePath().apply()


def post_execute() -> None:
    runtime.RuntimePath().close()


def _initialize():

    try:
        shell = get_ipython()
    except NameError:
        return

    log = ipykernel.comm.Comm(target_name='log', data={})
    runtime.RuntimeLogger(log)

    shell.kernel.comm_manager.register_target('runtime', runtime_wrapper)

    shell.events.register('pre_execute', pre_execute)
    shell.events.register('post_execute', post_execute)

_initialize()

cb = runtime.Clipboard()
game = gamelib.Game()

__all__ = [
    'cb', 'game',
    'debug', 'info', 'warning', 'error', 'critical',
    'Vector', 'Bezier'
]
