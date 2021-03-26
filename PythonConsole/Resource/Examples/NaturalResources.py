
# Select two point on the map
# The rectangular area between them will be converted to fertile land
# It takes a few seconds before the changes take place (game must be unpaused)

from_x = cba[0].resources.natural_resources_row_id
to_x = cba[1].resources.natural_resources_row_id
from_z = cba[0].resources.natural_resources_column_id
to_z = cba[1].resources.natural_resources_column_id

if to_x < from_x:
    from_x, to_x = to_x, from_x

if to_z < from_z:
    from_z, to_z = to_z, from_z

print(from_x, to_x, from_z, to_z)
for x in range(from_x, to_x):
    for z in range(from_z, to_z):
        print(x, z)
        NaturalResourceCell(x, z).fertility = 255