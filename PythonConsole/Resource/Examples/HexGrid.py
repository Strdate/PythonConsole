# This script generates hexagon road grid
# Select a point with clipboard tool as a starting point

import math

# Params

leng = 80              # Segment length
sizeX = 10             # Number of columns
sizeY = 10             # Number of rows
prefab = "Basic Road"  # Road type

# ---

diagX = 0.86602540 * leng
diagY = 0.5 * leng
buffer = []
carryNode = None

for y in range(0,sizeY):
  anchor = None
  rowY = math.ceil(float(y)/2) * leng + math.floor(float(y)/2) * leng * 2
  startVector = vector_xz( cb.pos.x , cb.pos.z + rowY )
  rowBuffer = []
  for x in range(0,sizeX):
    if anchor is None:
      anchor = game.create_node(startVector, prefab)
    downY = (1, -1)[y % 2 == 0] * diagY
    midNode = game.create_node(vector_xz(anchor.position.x + diagX, anchor.position.z + downY), prefab)
    rightNode = game.create_node(vector_xz(anchor.position.x + 2 * diagX, anchor.position.z), prefab)
    game.create_segments(anchor, midNode, prefab)
    game.create_segments(midNode, rightNode, prefab)
    if y % 2 == 0:
      rowBuffer.append(anchor)
      if len(buffer) > x:
        cnode = buffer[x]
        game.create_segments(midNode, cnode, prefab)
    else:
      rowBuffer.append(midNode)
      if len(buffer) > x:
        cnode = buffer[x]
        game.create_segments(anchor, cnode, prefab)
    anchor = rightNode
    if x == sizeX - 1:
      if y % 2 == 0:
        carryNode = rightNode
      else:
       game.create_segments(rightNode, carryNode, prefab)
  buffer = rowBuffer
