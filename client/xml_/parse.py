from typing import Optional
from xml import sax
from xml.sax.handler import ContentHandler
from xml.sax.xmlreader import AttributesImpl

from .base import XMLNode


class XMLParser(ContentHandler):
    """ Custom XML Parser """

    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
        self._result = None
        self._parent: Optional[XMLNode] = None
        self._current: Optional[XMLNode] = None

    @property
    def result(self) -> XMLNode:
        """ Get the parse result """
        if self._result is None:
            raise ValueError("Unknown content")
        return self._result

    def startElement(self, name, attrs: AttributesImpl):
        """ Create an XML node """
        self._parent = self._current
        self._current = XMLNode(
            name, parent=self._parent, **dict(attrs.items()))
        if self._parent is not None:
            self._parent.add_child(self._current)

    def endElement(self, name: str):
        """ Finish an XML node and return to its parent """
        if self._parent is not None:
            self._current = self._parent
            self._parent = self._current.parent

    def characters(self, content: str):
        """ Process the content of an XML node """
        if self._current is None:
            raise ValueError("More data detected")
        self._current.add_content(content)

    def endDocument(self):
        """ Set the parse result """
        self._result = self._current

def parse(src: str) -> XMLNode:
    handler = XMLParser()
    sax.parseString(src, handler=handler)
    return handler.result