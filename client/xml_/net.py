from __future__ import annotations
from abc import ABC, abstractmethod
from typing import Callable, Dict, List, NoReturn, Optional, Type, TypeVar, Union, overload
from typing_extensions import Literal

from .base import XMLNode

XML_NAMESPACE = {
    "xmlns:xsi": "http://www.w3.org/2001/XMLSchema-instance",
    "xmlns:xsd": "http://www.w3.org/2001/XMLSchema"
}


class IncompatibleError(Exception):

    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
    
    def __str__(self):
        return super().__str__()


class SupportsXML(ABC):

    @classmethod
    def __subclasshook__(cls, C):
        if cls is SupportsXML:
            if 'default' in C.__dict__ and 'from_xml_node' in C.__dict__:
                return True
        return NotImplemented

    @abstractmethod
    def default(self, parent: Optional[XMLNode]) -> XMLNode:
        ...

    @classmethod
    @abstractmethod
    def from_xml_node(cls, root: XMLNode) -> SupportsXML:
        ...

T = TypeVar('T')
BUILT_IN = Union[int, float, bool, str]
CONTAINER = List[Union[SupportsXML, BUILT_IN]]
TRUE = Literal[True]
FALSE = Literal[False]

class XMLInclude():
    """ Custom class name registration """

    _registered_name: Dict[str, type] = {}
    _registered_class: Dict[type, str] = {}
    BUILTIN_CLASS: Dict[type, str] = {
        int: 'int', str: 'string', float: 'float', bool: 'boolean'}
    BUILTIN_NAMES: Dict[str, type] = {
        'int': int,
        'uint': int,
        'float': float,
        'double': float,
        'string': str,
        'boolean': bool,
    }

    @overload
    def __new__(cls, xsi_name: Type[T]) -> Type[T]:
        ...

    @overload
    def __new__(cls, xsi_name: Optional[str]) -> Callable[[Type[T]], Type[T]]:
        ...

    def __new__(cls,
        xsi_name: Optional[str] | Type[T] = None
    ) -> Type[T] | Callable[[Type[T]], Type[T]]:

        if callable(xsi_name):
            result = xsi_name
            xsi_name = None
        else:
            result = None

        def wrapper(class_: Type[T]) -> Type[T]:
            assert isinstance(xsi_name, str) or xsi_name is None
            if not issubclass(class_, SupportsXML):
                raise TypeError("Registered classes should be subclass of `SupportsXML`")

            name = class_.__name__ if xsi_name is None else xsi_name
            cls._registered_name.update({name: class_})
            cls._registered_class.update({class_: name})
            return class_
        if result is None:
            return wrapper

        return wrapper(result)

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

    def export(self, obj: SupportsXML | BUILT_IN | CONTAINER, *,
        force_xsi_name: bool = False, type_override: Optional[str] = None,
        name_override: Optional[str] = None, parent: Optional[XMLNode] = None
    ) -> XMLNode:
        ret = self.serialize(
            obj, force_xsi_name=force_xsi_name, type_override=type_override,
            name_override=name_override, parent=parent
        )
        ret.attrs.update(XML_NAMESPACE)
        return ret

    def serialize(self, obj: SupportsXML | BUILT_IN | CONTAINER, *,
        force_xsi_name: bool = False, type_override: Optional[str] = None,
        name_override: Optional[str] = None, parent: Optional[XMLNode] = None
    ) -> XMLNode:
        """ Serialize a Python object into XMLNode tree """
        if type(obj) in XMLInclude.BUILTIN_CLASS:
            # Processing built-in types
            if name_override is None:
                name_override = XMLInclude.get_name(type(obj))
            if force_xsi_name:
                attrs = {'xsi:type': 'xsd:' + XMLInclude.get_name(type(obj))}
            else:
                attrs = {}
            ret = XMLNode(name_override, parent=parent, **attrs)

            if isinstance(obj, bool):
                ret.add_content(str(obj).lower())
            else:
                ret.add_content(str(obj))

        elif isinstance(obj, list):
            # Processing container types
            if name_override is None:
                name_override = 'list'

            ret = XMLNode(name_override, parent=parent)
            if len(set(type(_) for _ in obj)) == 1 \
                and type(obj[0]) not in XMLInclude.BUILTIN_CLASS:

                ret.attrs.update({'xsi:type': f'ArrayOf{type(obj[0]).__name__}'})

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
            assert isinstance(obj, SupportsXML)
            try:
                ret: XMLNode = obj.default(parent)
            except AttributeError as exception:
                raise ValueError("Registered class {0} have no `default()` method") \
                    from exception
            if force_xsi_name:
                ret.attrs.update({'xsi:type': XMLInclude.get_name(type(obj))})
            if name_override is not None:
                ret._name = name_override


        if parent is not None:
            parent.add_child(ret)
        return ret


class XMLDeserializer():
    """ Deserialize XML node to Python object """

    @overload
    def deserialize(self, root: XMLNode, *,
        is_container: TRUE = True, in_container: TRUE = True,
        type_override: None = None
    ) -> NoReturn:
        ...

    @overload
    def deserialize(self, root: XMLNode, *,
        is_container: TRUE = True, in_container: FALSE = False,
        type_override: None = None
    ) -> CONTAINER:
        ...

    @overload
    def deserialize(self, root: XMLNode, *,
        is_container: FALSE = False, in_container: bool = False,
        type_override: None = None
    ) -> SupportsXML | BUILT_IN:
        ...
    
    @overload
    def deserialize(self, root: XMLNode, *,
        is_container: TRUE = True, in_container: FALSE = False,
        type_override: Type[T]
    ) -> List[T]:
        ...

    @overload
    def deserialize(self, root: XMLNode, *,
        is_container: FALSE = False, in_container: bool = False,
        type_override: Type[T]
    ) -> T:
        ...

    def deserialize(self, root: XMLNode, *,
        is_container: bool = False,
        in_container: bool = False,
        type_override: Optional[Type[T]] = None
    ) -> SupportsXML | BUILT_IN | CONTAINER | NoReturn | T | List[T]:
        if is_container:
            if type_override is None:
                return [
                    self.deserialize(_, in_container=True, is_container=False)
                    for _ in root.child
                ]
            else:
                return [
                    self.deserialize(_,
                        in_container=True, is_container=False,
                        type_override=type_override
                    )
                    for _ in root.child
                ]
        if root.attrs.get('xsi:type', '').startswith("ArrayOf"):
            del root.attrs['xsi:type']
            return self.deserialize(root, is_container=True)
            
        if in_container or 'xsi:type' in root.attrs:
            name = root.attrs['xsi:type'] if 'xsi:type' in root.attrs else root.name
            type_ = XMLInclude.get_class(name)
            if type_ in XMLInclude.BUILTIN_CLASS:
                if root.child:
                    raise ValueError("Child node in built-in types")
                if 'xsi:type' in root.attrs:
                    del root.attrs['xsi:type']
                return type_(self.deserialize(root))

            assert issubclass(type_, SupportsXML)
            try:
                return type_.from_xml_node(root)
            except AttributeError:
                raise ValueError("Registered class {0} have no `from_xml_node()` method")

        if not root.child: # Built-in types
            ret = ''.join(root.content).strip()
            if type_override is not None:
                return type_override(ret)
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
        if type_override is None:
            for _ in XMLInclude.registered_class():
                assert issubclass(_, SupportsXML)
                try:
                    return _.from_xml_node(root)
                except IncompatibleError:
                    continue
        else:
            assert issubclass(type_override, SupportsXML)
            return type_override.from_xml_node(root)

        raise ValueError(f"No compatible registered class found for {root.name}")
