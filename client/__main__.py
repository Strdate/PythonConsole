""" __main__.py -- demo version
Launchend by external Python 3.5+ engine
"""
import asyncio
import logging
from . import config
from . import remote

if __name__ == "__main__":
    logging.basicConfig(
        filename=config.LOG_PATH, filemode='a', level=config.LOG_LEVEL
    )
    server = remote.AsyncServerHandler(config.HOST_ADDR, config.HOST_PORT)
    asyncio.run(server.start())
