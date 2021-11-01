from typing import Optional
from .. import xml_

@xml_.XMLInclude
class RemoteException(xml_.SupportsXML, Exception):
    """
    Exception that is thrown when a remote method fails.
    """
    def __init__(self, message):
        self.msg = message

    def __str__(self):
        return self.msg

    def default(self, parent: Optional[xml_.XMLNode]) -> xml_.XMLNode:
        encoder = xml_.XMLSerializer()
        ret_node = xml_.XMLNode(
            xml_.XMLInclude.get_name(type(self)),
            parent=parent, **{'xsi:type': xml_.XMLInclude.get_name(type(self))}
        )
        encoder.serialize(
            self.msg, name_override='msg', parent=ret_node, force_xsi_name=True
        )
        return ret_node

    @classmethod
    def from_xml(cls, root_node: xml_.XMLNode) -> 'RemoteException':
        decoder = xml_.XMLDeserializer()
        child = {_.name: _ for _ in root_node.child}
        ret = super().__new__(cls)
        if 'msg' in child:
            ret.msg = decoder.deserialize(child['msg'], type_override=str)
        else:
            raise xml_.IncompatibleError
        return ret
