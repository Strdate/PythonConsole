#  Save your game before executing this script!
#
# This script replaces all the trees on the map
#

engine.async_mode = True;

for tree in game.trees:
  if tree.prefab_name != "treeForest":
    pos = tree.pos
    tree.delete()
    game.create_tree(pos, "treeForest")

engine.async_mode = False;