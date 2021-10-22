from abc import abstractmethod
from .. import xml_
from typing import Any, Dict, Optional


class BaseMessage(xml_.SupportsXML):
    def __init__(self, **kwargs: Any):
        absent = list(set(self.attributes) - set(kwargs))
        if absent:
            absent.sort()
            raise ValueError(f"Field {0} not found in kwargs".format(absent))
        self._attrs = {_: kwargs[_] for _ in self.attributes}

    @property
    def content(self) -> Dict[str, Any]:
        return self._attrs

    @property
    @abstractmethod
    def attributes(self) -> Dict[str, bool]:
        return {}

    @classmethod
    def from_xml_node(cls, root_node: xml_.XMLNode):
        decoder = xml_.XMLDeserializer()
        child = {_.name: _ for _ in root_node.child}
        ret = super().__new__(cls)

        if set(child) != set(ret.attributes):
            raise xml_.IncompatibleError
        kwargs = {}
        for _ in ret.attributes:
            if ret.attributes[_]:
                kwargs.update({_: decoder.deserialize(child[_], is_container=True)})
            else:
                kwargs.update({_: decoder.deserialize(child[_], is_container=False)})
        ret.__init__(**kwargs)
        return ret

    def default(self, parent: Optional[xml_.XMLNode]) -> xml_.XMLNode:
        encoder = xml_.XMLSerializer()
        ret_node = xml_.XMLNode(
            xml_.XMLInclude.get_name(type(self)),
            parent=parent, **{'xsi:type': xml_.XMLInclude.get_name(type(self))}
        )
        for _ in self.attributes:
            if self.content[_] is None:
                continue
            try:
                encoder.serialize(
                    self.content[_],
                    name_override=_, parent=ret_node, force_xsi_name=True
                )
            except KeyError as e:
                raise KeyError("Attribute {0} not found".format(_)) from e
        return ret_node

    def __contains__(self, o: str):
        return o in self.content

    def __getitem__(self, name: str) -> Any:
        return self.content[name]

    def __iter__(self):
        return iter(self.content)


class InstanceDataBase(BaseMessage):
    """ Base class for instance data """

    def __init__(self, **kwargs):
        super().__init__(**kwargs)

    @property
    def attributes(self):
        return {'id': False}


class InstanceData(BaseMessage):
    """ Building information """

    def __init__(self, **kwargs):
        super().__init__(**kwargs)

    @property
    def attributes(self):
        ret = super().attributes.copy()
        ret.update({
            'position': False, 'prefab_name': False, 'exists': False
        })
        return ret


@xml_.XMLInclude
class MessageHeader(BaseMessage):
    """ Wraps message payload """

    def __init__(self, **kwargs):
        super().__init__(**kwargs)

    @property
    def attributes(self):
        return {
            'messageType': False,
            'requestId': False, 'payload': False
        }
