from . import gamelib

def initialize(host: str = 'localhost', port = 6672):
    game = gamelib.Game()
    game.configure(host, port)

game = gamelib.Game()

__all__ = ['initialize', 'game']
