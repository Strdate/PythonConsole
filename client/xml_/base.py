from __future__ import annotations

from typing import Callable, Dict, List, Optional


class XMLNode():
    """ Python representation of an XML Node """

    def __init__(self,
        name: str,
        *child: 'XMLNode',
        parent: Optional['XMLNode'] = None,
        **attrs: str
    ):
        self._name = name
        self._content: List[str] = [] # Text node between child nodes
        self._parent = parent # None if the node is root node
        self._child = list(child)
        self._attrs = attrs
        self._seq = [] # len(_seq) == len(_child) + len(_content)
                       # 1 if corresponds to a child node
                       # 0 uf corresponds to a text node

    @property
    def name(self) -> str:
        """ Name of the XML node """
        return self._name

    @property
    def content(self) -> List[str]:
        """ Raw text of the XML node"""
        return self._content

    @property
    def parent(self) -> Optional['XMLNode']:
        """ Parent node (if exists) of the XML node """
        return self._parent

    @property
    def child(self) -> List['XMLNode']:
        """ Child node of the XML node"""
        return self._child

    @property
    def attrs(self) -> Dict[str, str]:
        """ Attributes of the XML node """
        return self._attrs

    def add_child(self, child: 'XMLNode'):
        """ Add a child node to the current node """
        self._seq.append(1)
        self._child.append(child)

    def add_content(self, content: str):
        """ Add a text node to the current node """
        self._seq.append(0)
        self._content.append(content)

    def __str__(self) -> str:
        """ Returns the raw XML string """
        attrs = " ".join(f'{k}="{v}"' for k, v in self._attrs.items())
        content = iter(self._content)
        child = iter(self._child)

        return '''<{header}>{content}</{ending}>'''.format(
            header=" ".join([self._name, attrs]).strip(),
            content="".join(
                str(next(child)) if _ else next(content) for _ in self._seq
            ),
            ending=self._name
        )

    def __repr__(self) -> str:
        """ Returns the minified XML string """
        attrs = " ".join(f'{k}="{v}"' for k, v in self._attrs.items())
        content = iter(self._content)
        child = iter(self._child)

        return '''<{header}>{content}</{ending}>'''.format(
            header=" ".join([self._name, attrs]).strip(),
            content="".join(
                repr(next(child)) if _ else next(content).strip() for _ in self._seq
            ),
            ending=self._name
        )

    def __bytes__(self) -> bytes:
        """ Returns the byte sequence """
        return repr(self).encode('utf-8')
