#  When you are building road segments, you can specify start and end nodes using many
#  different objects:
#
#  - Points that you selected with Clipboard Tool
#  - Existing road nodes. The new segments will be connected at the intersection
#  - Your own vectors:
#  
#  my_vector = Vector(500, -10, 50)
#  
#  Alternatively if you don't want to define the height (height is the Y coord):
#  
#  my_vector = vector_xz(800, -300)
#  
#  Node height will then be automatically set to terrain height.
#  You can always find out the terrain height yourself using these functions:
#
#  my_height = game.terrain_height(position)
#  my_height_2 = game.surface_level(position)
#
#  Surface level function includes water level.
#
#  Elevated roads / tunnels / other
#  --------------------------------
#
#  You can use NetOptions object to specify many things regarding terrain.
#
#  my_segments = game.create_segments(cba[0], cba[1],
#    NetOptions("Highway", "auto_offset", "elevated", True) )
#
#  The second argument specifies terrain conformity. Possible values are:
#
#  - False (default): the start and end nodes are at specified height or at terrain height
#      if the height is not specified. The slope is constant.
#  - True: all nodes are at terrain height
#  - "auto_offset": all nodes are at the specified offset from the terrain. The offset is
#      calculated from the offset of the start and end node. Example: first node is 10 units
#      above ground, last node is 8 units above ground. All middlwe nodes will be then
#      10 to 8 units above ground.
#
#  The 3rd argument specifies road elevation type. Basic set of values is:
#
#  - "default" ( = ground )
#  - "ground"
#  - "elevated"
#  - "bridge"
#  - "tunnel"
#  - "slope": Tunnel entarance/exit
#
#  The 4th argument specifies, if the road should be inverted. For example, it changes
#  the direction of one-way streets of flips asymmetric roads.
#
#  The 5th argumnent specifies node (pillar) spacing. Default is 100 units. (The actual
#  spacing will always vary slightly so the nodes are equally spaced)
#
#  You don't always need to specify all arguments of the NetOptions object, but only the
#  first few that you need.
#
#  Vector / point highlighting
#  ---------------------------
#
#  You can temporarily highlight points or vectors on the map to help you with debugging.
#
#  The following script build a new road and highlights the middle of all new segments:
#
#
#  my_segments = game.create_segments(cba[0], cba[1],
#    NetOptions("Highway", "auto_offset", "elevated", True) )
#
#  for segment in my_segments:
#      game.draw_circle(segment)
#
#
#  You can call 'game.clear()' to remove the highlighting.
#  To draw vectors, use the game.draw_vector(...) function. Call 'help()' to find what the
#  parameters are as well as to information about many other functions.
#