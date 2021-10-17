from __future__ import annotations
from abc import ABC, abstractmethod
from typing import Any, Callable, Dict, Optional

from .base import XMLNode


class IncompatibleError(Exception):

    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
    
    def __str__(self):
        return super().__str__()


class SupportsXML(ABC):

    @classmethod
    def __subclasshook__(cls, C):
        if cls is SupportsXML:
            if 'default' in C.__dict__ and 'from_xml_node' in C.__dict:
                return True
        return NotImplemented

    @abstractmethod
    def default(self, parent: Optional[XMLNode]) -> XMLNode:
        ...

    @classmethod
    @abstractmethod
    def from_xml_node(cls, root: XMLNode):
        ...

class XMLInclude():
    """ Custom class name registration """

    _registered_name: Dict[str, type] = {}
    _registered_class: Dict[type, str] = {}
    BUILTIN_CLASS: Dict[type, str] = {
        int: 'int', str: 'string', float: 'float', bool: 'bool'}
    BUILTIN_NAMES: Dict[str, type] = {
        'int': int,
        'uint': int,
        'float': float,
        'double': float,
        'string': str,
        'bool': bool,
    }

    def __new__(cls, xsi_name: Optional[str | Callable] = None):

        if callable(xsi_name):
            xsi_name = None

        def wrapper(class_: type):
            assert isinstance(xsi_name, str) or xsi_name is None
            if not issubclass(class_, SupportsXML):
                raise TypeError("Registered classes should be subclass of `SupportsXML`")

            name = class_.__name__ if xsi_name is None else xsi_name
            cls._registered_name.update({name: class_})
            cls._registered_class.update({class_: name})
            return class_

        return wrapper

    @classmethod
    def registered_class(cls):
        return list(cls._registered_class)

    @classmethod
    def get_class(cls, name: str) -> type:
        name = name.replace('xsd:', '')
        if name in cls.BUILTIN_NAMES:
            return cls.BUILTIN_NAMES[name]
        try:
            return cls._registered_name[name]
        except KeyError as exception:
            raise ValueError(
                "Unregistered type name: {0}".format(name)) from exception

    @classmethod
    def get_name(cls, type_: type) -> str:
        if type_ in cls.BUILTIN_CLASS:
            return cls.BUILTIN_CLASS[type_]
        try:
            return cls._registered_class[type_]
        except KeyError as exception:
            raise ValueError(
                "Unregistered type: {0}".format(type_)) from exception


class XMLSerializer():
    """ Serialize a Python object to XML node """

    def serialize(self, obj: SupportsXML, *,
        force_xsi_name: bool = False, type_override: Optional[str] = None,
        name_override: Optional[str] = None, parent: Optional[XMLNode] = None
    ) -> XMLNode:
        """ Serialize a Python object into XMLNode tree """
        if type(obj) in XMLInclude.BUILTIN_CLASS:
            # Processing built-in types 
            if name_override is None:
                raise ValueError("Unknown tag name for value {0}".format(obj))
            if force_xsi_name:
                attrs = {'xsi:type': 'xsd:' + XMLInclude.get_name(type(obj))}
            else:
                attrs = {}
            ret = XMLNode(name_override, parent=parent, **attrs)

            if isinstance(obj, bool):
                ret.add_content(str(obj).lower())
            ret.add_content(str(obj))

        elif isinstance(obj, list):
            # Processing container types
            if name_override is None:
                raise ValueError("Unknown tag name for value {0}".format(obj))

            ret = XMLNode(name_override, parent=parent)
            child_name = "anyType" if type_override is None else type_override
            force_xsi_name = type_override is None
            for _ in obj:
                if type_override is None or isinstance(_, XMLInclude.get_class(type_override)):
                    self.serialize(_, 
                        name_override=child_name,
                        force_xsi_name=force_xsi_name, 
                        parent=ret
                    )
                else:
                    raise ValueError("Class type conflict")

        else:
            # Processing custom types
            try:
                ret: XMLNode = obj.default(parent)
            except AttributeError as exception:
                raise ValueError("Registered class {0} have no `default()` method") \
                    from exception
            if force_xsi_name:
                ret.attrs.update({'xsi:type': XMLInclude.get_name(type(obj))})


        if parent is not None:
            parent.add_child(ret)
        return ret


class XMLDeserializer():
    """ Deserialize XML node to Python object """

    def deserialize(self, root: XMLNode, *,
        is_container: bool = False,
        in_container: bool = False,
    ) -> Any:
        if is_container:
            return [self.deserialize(_, in_container=True) for _ in root.child]
        if in_container or 'xsi:type' in root.attrs:
            name = root.attrs['xsi:type'] if root.name is None else root.name
            type_ = XMLInclude.get_class(name)
            if type_ in XMLInclude.BUILTIN_CLASS:
                if root.child:
                    raise ValueError("Child node in built-in types")
                return type_(self.deserialize(root))

            try:
                return type_.from_xml_node(root)
            except AttributeError:
                raise ValueError("Registered class {0} have no `from_xml_node()` method")

        if not root.child: # Built-in types
            ret = ''.join(root.content).strip()
            if ret == 'true':
                return True
            if ret == 'false':
                return False

            for _ in [int, float, str]:
                try:
                    return _(ret)
                except ValueError:
                    continue
            
            return ret

        # Direct custom attribute 
        for _ in XMLInclude.registered_class():
            try:
                return _.from_xml_node(root)
            except IncompatibleError:
                continue

        raise ValueError("No compatible registered class found")
