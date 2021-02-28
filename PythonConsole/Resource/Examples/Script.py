# Press ALT+A to activate the Clipboard tool. With this tool you can inspect properties of
# Warning! Do not save any script in this subfolder as it will be overwritten.
#
# Building roads
# --------------
#
# While holding CTRL and SHIFT select two point on the map
# Execute this command to build a straight road between these points:

# game.create_segments(cba[0], cba[1], "Basic Road")

# Instead of a point on a map you can select an existing node (intersection) so the roads
# are connected.
#
# To build curved road, select three points different points. The third point is a middle
# control point of the underlying Bézier curve (it is usually somewhere around the center
# of the segment, but might not directly lie on the segment). More around Bézier curves:
# https://en.wikipedia.org/wiki/B%C3%A9zier_curve#Quadratic_curves
# The argument order is as following - start, end, net options, control point

# my_segments = game.create_segments(cba[0], cba[1], "Highway", cba[2])

# You can also use this overload of the method, which instead of a control point takes
# direction vectors at the start and end point:
# game.create_segment(start, end, net_options, start_direction, end_direction)
# If you want to specify more information about created segments, use NetOptions object
# instead of prefab name. It takes these arguments:
#
# NetOptions(string prefab_name, object follow_terrain = null, string elevation_mode =
#   "default", bool invert = false, int node_spacing = 100)
#
# Example call:

# game.create_segments(cba[0], cba[1], NetOptions("Highway", false, "elevated", true) )

# Learn more about NetOptions by calling 'help(NetOptions)'
#
# Last option how to build segments is using the PathBuilder object. While holding SHIFT and
# CTRL at the same time, select any number of points which will be then connected:

# my_path = game.begin_path(cba[0], "Basic Road")
#
# for point in cba[1:]:
#     my_path.path_to(point)

# The path_to method might take control point or direction vectors as the second
# and third argument.
#
# See more examples by opening another script file at the top of this window.
#