""" network.py
Socket-based networking for communicating with Cities: Skylines
"""

from __future__ import annotations

import asyncio
import logging
import socket
import struct


class AsyncSocketHandler():
    """ Asynchronous socket """

    def __init__(self, host: str, port: int):
        self._sock = socket.socket()
        self._sock.bind((host, port))
        logging.info(f"Listening on {host} port {port}")
        self._sock.listen()
        self._conn = None

    async def accept(self):
        """ Accept an incoming connection """
        loop = asyncio.get_running_loop()
        self._conn, _ = await loop.sock_accept(self._sock)
        logging.info(f"Connection accepted")

    async def recv(self) -> str:
        """ Receive data from an existing connection """
        if self._conn is None:
            logging.error("Socket unconnected")
            raise RuntimeError("Socket unconnected")

        loop = asyncio.get_running_loop()
        length, *_ = struct.unpack('I', await loop.sock_recv(self._conn, 4))
        ret = (await loop.sock_recv(self._conn, length)).decode('utf-8')
        logging.info(f"Received {len(ret)}-byte message")
        logging.debug(ret)
        return ret

    async def send(self, content: str | bytes):
        """ Send data through an existing connection """
        if self._conn is None:
            logging.error("Socket unconnected")
            raise RuntimeError("Socket unconnected")

        loop = asyncio.get_running_loop()
        if isinstance(content, str):
            content = content.encode('utf-8')

        length = len(content)
        data = struct.pack('I', length) + content
        await loop.sock_sendall(self._conn, data)
        logging.debug(f"Sent {len(data)}-byte message")
        logging.debug(content.decode('utf-8'))

class SocketHandler():
    """ Synchronous socket """

    def __init__(self, host: str, port: int):
        self._sock = socket.socket()
        self._sock.bind((host, port))
        self._sock.listen()
        self._conn = None

    def accept(self):
        """ Accept an incoming connection """
        self._conn, _ = self._sock.accept()

    def recv(self) -> str:
        """ Receive data from an existing connection """
        if self._conn is None:
            raise RuntimeError("Socket unconnected")

        length, *_ = struct.unpack('I', self._conn.recv(4))
        return self._conn.recv(length).decode('utf-8')

    def send(self, content: str | bytes):
        """ Send data through an existing connection """
        if self._conn is None:
            raise RuntimeError("Socket unconnected")

        if isinstance(content, str):
            content = content.encode('utf-8')

        length = len(content)
        data = struct.pack('I', length) + content
        self._conn.sendall(data)
