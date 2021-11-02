from __future__ import annotations

import json
from typing import Any

from .. import meta, model, protocol, xml_

class Game(model.GameABC, metaclass=meta.Singleton):

    def _remote_call(self,
        contract: protocol.Contract, message: Any
    ) -> Any:
        encoder = xml_.XMLSerializer()
        decoder = xml_.XMLDeserializer()
        async_mode = self._async_mode and contract.can_run_async
        builtin = isinstance(message, (bool, str, int, float))
        response = input(json.dumps({
            'contract': vars(contract),
            'message': str(encoder.export(message)) if not builtin else message,
            'async': async_mode,
            'builtin': builtin
        }))
        if response is not None:
            ret = decoder.deserialize(xml_.parse(response))
            if isinstance(ret, Exception):
                raise ret
            return ret
        else:
            return None
