
# This code helps you build smooth road and track curves as seen on the Workshop page.
# For usage, go to the bottom of the script.

# Warning: do not edit and then save code of the example scripts, as it will be overridden
# on the next mod load. Copy this file to the root directory instead.

import curvetools as ct
from math import radians, cos, sin

def buildCorner(coords, opt = None):
    '''This function takes the calculated coordinates (x, z) of the curve and builds it in the game.'''
    if opt == None:
        opt = NetOptions("Gravel Road", True, node_spacing = 96)
    vectors = [vector_xz(c[0], c[1]) for c in coords]
    my_path = game.begin_path(vectors[0], opt)

    for i in range((len(vectors)-1)//2):
        cp_index = 2*i+1 #control point index
        ep_index = cp_index+1 #endpoint index
        cp = vectors[cp_index]
        ep = vectors[ep_index]
        my_path.path_to(ep, cp)
        #print("%.3f, %.3f" %(cp.x, cp.y))

    return None #my_path

def curve(cba, left=True, radius=100, vlength=100, n=0, ramp=False):
    '''This function collects the user input, passes them to the curve calculator
    and then passes the calculated coordinates on to the curve builder.'''
    points = [(cb.pos.x, cb.pos.z) for cb in cba]
    try:
        opt = NetOptions(cba[0].prefab_name, True, node_spacing = 96)
    except:
        opt = None

    #If vlength in range 0 to 1: use it as a percentage of the shortest vector
    if type(vlength) in (int, float) and 0 < vlength <= 1:
        distance = min(ct.getDistance(points[0], points[1]), ct.getDistance(points[-1], points[-2]))
        vlength *= distance
        
    coords = ct.exe(points, left, radius, vlength, n, ramp)
    my_path = buildCorner(coords, opt)

def test(angle = 90, left = True, radius=100, vlength=100, n=0, ramp=False):
    a = radians(angle)
    points = [(-100, 0), (0, 0), (100*cos(a), 100*sin(a))]
    if type(vlength) in (int, float) and 0 < vlength <= 1:
        distance = min(ct.getDistance(points[0], points[1]), ct.getDistance(points[-1], points[-2]))
        vlength *= distance
        
    coords = ct.exe(points, left, radius, vlength, n, ramp)
    

### 1. Build two straight and intersecting road segments at an angle
### (e.g. V-shape), representing the  start vector and end vector of
### the curve.
###
### 2. Using the clipboard tool, select a node of the start vector,
### then the node at the intersection and finally a node of the end
### vector so that cba[0] = the start node, cba[1] = the intersection
### node and cba[2] = the end node.
###
### 3. Uncomment the function at the very bottom of this page.
###   
### 4. Set left = True (or 1) for the curve to turn left;
### Set left = False (or 0) to turn right.
###
### 5. Set your desired curve radius (or set radius = None if you would
### rather define your curve by the vector length, see below).
###
### 6. If you want the curve to fit within a specific vector length, set
### that length as vlength. Alternatively: Set vlength = 1 to fit the
### curve to the shortest of the distances cb[0] to cb[1] or cb[1] to cb[2].
### You can also set vlength to a number between 0 and 1  to use a
### fraction of that distance. Set vlength = None to for an unlimited
### vector length.
###
### 7. To build a spiral (a curve of more than 360 degrees), set n to
### an integer greater than 0. This adds n number of full circles to
### the curve.
###
### 8. Highway ramps often use longer transition curves. This can be
### achieved by setting ramp = True.
###
### 9. When you have entered your settings, execute the script to build
### the curve.
###
### Note: Instead of step 1 and 2, you can select three points on the
### map with the clipboard
###
### If you have questions about this script, send me an email at
### eran0004@gmail.com

#curve(cba, left=True, radius=100, vlength=None, n=0, ramp=False)

