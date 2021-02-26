# This example demonstrates building a row of trees in the shape of the sinus function
# While hlding SHIFT select any point on the map with the Clipboard Tool and execute the script

import math

for i in range(0,100):
    game.create_tree( vector_xz(cb.pos.x + i*10, cb.pos.z + math.sin(i / 5.0) * 70), "Tree3variant")