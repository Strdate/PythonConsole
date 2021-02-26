# Press ALT+A to activate the Clipboard tool. With this tool you can inspect properties of
# Warning! Do not save any script in this subfolder as they will be overwritten.
#
# Building roads
# --------------
#
# While holding CTRL and SHIFT select two point on the map
# Execute this command to build a straight road:

# game.create_segments(cba[0], cba[1], "Basic Road")

# Instead of a point on a map you can select an existing node (intersection) so the roads
# are connected.
#
# To build curved road, select three points with the middle point being little bit
# off axis. Then execute the following command:

# game.create_segments(cba[0], cba[1], "Highway", cba[2])