
# ADVANCED

# This is a library needed for building smooth transition curves
# The code is never executed directly but used as an import

# To use this code, go to 'curveapp.py' script

from math import tan, sin, cos, pi, radians, sqrt, atan2


###     --------- CLASSES --------- CLASSES --------- CLASSES ---------

class Configuration():
    def __init__(self):
        self.reset()

    def reset(self):
        self.Ltk            = 24            #transition length factor: Lt = sqrt(Ltk*R)
        self.min_v          = radians(5)    #no transition curves for turn angles < 5 degrees
        self.min_v1         = radians(2)    #transition angle must be >= 2 degrees
        self.max_v1         = radians(60)   #transition angle must be <= 45 degrees
        self.minlen         = 24            #minimum segment length
        self.maxLt          = 200           #maximum transition length   
        
class Coord():
    def __init__(self, x, y):
        self.x = x
        self.y = y
        self.xy = (x, y)

    def s(self, offset = (0, 0)):
        dx, dy = offset
        return '(%.3f, %.3f)' % (self.x+dx, self.y+dy)
    
###     --------- MISC FUNCTIONS --------- MISC FUNCTIONS --------- MISC FUNCTIONS ---------  

def extractAngles(points, left, n):
    p = points
    startAngle = getAngle(p[0], p[1])
    endAngle = getAngle(p[-2], p[-1]) 
    turnAngle = abs(getTurnAngle(startAngle, endAngle, left, n))
    startpoint = p[1]
    endpoint = p[-2]
##    intersectpoint = intersect(p[1], startAngle, p[-2], endAngle, False)
    angles = (startAngle, endAngle, turnAngle)
    coords = (startpoint, endpoint)
    return angles, coords
    

def degrees(x):
    '''Takes an angle in radians and returns it in degrees'''
    return 180*x/pi

def getDistance(p0, p1):
    '''Returns the distance between two points'''
    x0, y0 = p0
    x1, y1 = p1

    dx = x1 - x0
    dy = y1 - y0

    d = sqrt(dx**2 + dy**2)

    return d

def getAngle(p0, p1):
    '''Returns the vector between two points'''
    x0, y0 = p0
    x1, y1 = p1
    a = atan2(y1-y0, x1-x0)
    a = normalizeAngle(a)

    return a   

def getTurnAngle(v0, v1, ccw=True, n=0):
    '''Calculates turn angle from start vector and end vector. Specify direction and n number of circles.'''
    v0 = normalizeAngle(v0)
    v1 = normalizeAngle(v1)

    dv1 = normalizeAngle(v1-v0)
    dv2 = 2*pi-dv1
    if ccw:
        turnangle = dv1+n*2*pi
    else:
        turnangle = -dv2 -n*2*pi
    return turnangle

def intersect(p0, v0, p1, v1, inverted=True):
    x0, y0 = p0
    x1, y1 = p1
    if inverted: #cities skylines coords
        a = y1*sin(v1) - x1*cos(v1)
        b = y0*sin(v0) - x0*cos(v0)
        c = sin(v0)*cos(v1) - cos(v0)*sin(v1)
        x = (sin(v1)*b - sin(v0)*a)/c
        y = (cos(v1)*b - cos(v0)*a)/c
    else:
        a = x1*sin(v1) - y1*cos(v1)
        b = x0*sin(v0) - y0*cos(v0)
        c = cos(v0)*sin(v1) - sin(v0)*cos(v1)
        x = (cos(v0)*a - cos(v1)*b)/c
        y = (sin(v0)*a - sin(v1)*b)/c
    return (x, y)

def normalizeAngle(v):
    '''Takes an angle v and returns it as a normalized value between 0 and 2*pi'''
    fullcircle = 2*pi
    return v%fullcircle

def psum(n):
    '''Pyramid sum of n'''
    return n*(n+1)/2

def vectorToCoord(distance, angle, origo=None, inverted=True):
    if origo == None:
        x = 0
        y = 0
    else:
        x = origo.x
        y = origo.y
        
    if inverted: #to CS coords
        x = x-sin(angle)*distance
        y = y-cos(angle)*distance
    else:     #correct coords...
        x = x+cos(angle)*distance
        y = y+sin(angle)*distance

    return Coord(x, y)


###     --------- CLOTHOID FUNCTIONS --------- CLOTHOID FUNCTIONS --------- CLOTHOID FUNCTIONS ---------

def approxAlfaBetaGamma(v1, v2):
    '''Fast, but losing accuracy at small and large angles.
Should only be used for angles between 2 and 90 degrees'''
    abRatio = 0.0012*v1**4-0.0108*v1**3-0.0704*v1**2-0.0087*v1+2.0002
    alfa = 0.0495*v1**4-0.018*v1**3+0.0583*v1**2+1.318*v1+0.0006
    beta = alfa/abRatio
    gamma = tan(v2/2)

    return alfa, beta, gamma

def altAlfaBetaGamma(v1, v2, n = 1000):
    '''Fast for small angles, takes more time for greater angles. Very accurate.'''
    T = L_normalized = sqrt(abs(v1))
    n = max([int(T*5000), 1000])
    R_n = 1/(2*T)
    dt = T/n
    t = 0
    x = 0
    y = 0
    for i in range(1, n+1):
        dx = cos(t**2) * dt
        dy = sin(t**2) * dt
        t += dt
        x += dx
        y += dy
    #print(t, T)
    alfa = (x-y/tan(v1))/R_n
    beta = (y/sin(v1))/R_n
    gamma = tan(v2/2)
    
    return alfa, beta, gamma

def getDesignRatio(v1, v3, alfa, beta, gamma):
    '''This function returns a designRatio that is length/radius'''
    try:
        designRatio = alfa + (beta+gamma)*(cos(v1)+sin(v1)/tan(v3))
    except ZeroDivisionError:
        designRatio = alfa + (beta+gamma)*cos(v1)
    return designRatio

def constructCorner(v1, v2, v3, R = None, l = 100):
    if radians(2) < v1: 
        alfa, beta, gamma = approxAlfaBetaGamma(v1, v2) #the fast algorithm
    else:
        alfa, beta, gamma = altAlfaBetaGamma(v1, v2) #the accurate algorithm

    designRatio = getDesignRatio(v1, v3, alfa, beta, gamma)       
    if R == None:
        R = l/designRatio
    else:
        l = R*designRatio
            
    a = alfa*R
    b = beta*R
    c = gamma*R

    return a, b, c, v1, v2, R, l


###     --------- EXE --------- EXE --------- EXE ---------

def exe(points, left, radius=None, vlength=None, n=0, ramp=0):
    config = Configuration()
    
    angles, coords = extractAngles(points, left, n)
    startAngle, endAngle, turnAngle = angles
    startpoint, endpoint = coords

    v = turnAngle
    v3 = (pi-v)/2
    if ramp: #ramp is a corner with exceptionally long transition angle, common in highway interchange design
        v1 = min(config.max_v1, v/3)
        v2 = v-2*v1
    elif radius != None:
        v1 = radiusMode(radius, config.Ltk)
        v1, v2 = checkAngles(v, v1, radius, config.minlen)
    else:
        Rref, vlength, v1 = vectorLengthMode(v, v3, vlength, config.Ltk)
        v1, v2 = checkAngles(v, v1, Rref, config.minlen)
        
    a, b, c, v1, v2, R, l = constructCorner(v1, v2, v3, radius, vlength)
    if radius != None and vlength != None and vlength-l < config.minlen:
        a, b, c, v1, v2, R, l = constructCorner(v1, v2, v3, None, vlength)

    if min(turnAngle, turnAngle%pi) < radians(1):
        print("turnAngle close to n*180")
        placeMethod = "SP"
        ref = startpoint
    else:
        try:
            ref = intersectpoint = intersect(startpoint, startAngle, endpoint, endAngle, False)
            placeMethod = "IP"
        except ZeroDivisionError:
            placeMethod = "SP"
            ref = startpoint
                                                
    coords = printCorner(a, b, v1, v2, R, l, startAngle, left, placeMethod, ref, printing=True)
    return coords

def convertAngle(v):
    '''Converting between normal angles and CS angles (radians only)'''
    v = 3*pi/2 - v
    v = normalizeAngle(v)
    return v

def radiusMode(R, Ltk = 24):
    Lt = sqrt(Ltk*R)
    v1 = Lt/(2*R)
    return v1
    
def vectorLengthMode(v, v3, l, Ltk = 24):
    R1 = l*tan(v3)
    R2 = R1*sin(v/2)
    Rref = (R1+R2)/2
    if Rref < 0 and l > 0:
        print("Rref < 0; l becomes negative")
        l = -l
    Lt = sqrt(Ltk*Rref)
    v1 = Lt/(2*l*tan(v3)) #transition angle
    return Rref, l, v1

def checkAngles(v, v1, Rref, minlen = 24):
    if v1 > v/2:            # transition angle can't be greater than turn angle/2
        v1 = v/2
    if v1 > pi/2:
        v1 = pi/2           # accuracy breaks down if v1 > pi/2
    v2 = v-2*v1             # angle for the circular arc
    if v2*Rref < minlen:        # circular segment length should not be smaller than 24 meters
        v2 = minlen/Rref
        v1 = (v-v2)/2
        if v2/v1 < 0.5:     # if v2 is small compared to v1, then skip the circular arc segment
            v2 = 0
            v1 = v/2
    return v1, v2 

def printCorner(a, b, v1, v2, R, l, startangle, ccw = True, placeMethod = 'IP', ref= None, printing = True):
    '''Generates and prints coordinates useful for building the corner in Cities Skylines'''

    ### --- Is the turn counterclockwise or not? --- ###
    if ccw:
        k = 1
    else:
        k = -1

    ### --- Describe the corner as a list of vectors --- ###  
    #transition intro
    va = startangle
    vb = startangle+v1*k
    vectors = [[a, va, 'S1'], [b, vb, 'S2']]

    #subdivide circular arc
    arclength = abs(v2*R)
    divisor = int(1 + max(v2 // radians(90), arclength // 96)) #segments can't cover more than 90 degree and can't be longer than 96 meters
    dv = v2/divisor
    c = d = R*tan(dv/2) 
    for i in range(divisor):
        vc = startangle+(v1+i*dv)*k
        vd = startangle+(v1+(i+1)*dv)*k
        vectors.extend([[c, vc, 'C'], [d, vd, 'C']])

    #transition outro
    e = b
    f = a
    ve = startangle+(v1+v2)*k
    vf = startangle+(2*v1+v2)*k
    vectors.extend([[e, ve, 'S2'], [f, vf, 'S1']])

    ### --- Generate coordinates from the vectors --- ###
    p0 = Coord(0, 0)
    for vector in vectors:
        d, v, litt = vector
        p1 = vectorToCoord(d, v, p0, inverted = False)
        vector.extend([p0, p1])
        p0 = p1


    ### --- Place the corner by calculating an offset for x and y --- ###
    if placeMethod == 'IP':   #place the corner so that it aligns with two intersecting vectors      
        alfa = va
        beta = vf
        pa = (0, 0)
        pb = vectors[-1][-1].xy
        x0, y0 = intersect(pa, alfa, pb, beta, inverted = False)

    elif placeMethod == 'SP':   #place the corner so that it starts at a given point
        x0, y0 = (0, 0)

    elif placeMethod == 'EP':   #place the corner so that it ends at a given point
        x0, y0 = vectors[-1][-1].xy

    x1, y1 = ref  
    dx = x1 - x0
    dy = y1 - y0
    offset = (dx, dy)


    ### --- Time to print the data --- ###
    index = 0
    coords = []
    print('i, d[i], v[i],(x[i], y[i]), descr')
    for vector in vectors:
        d, v, litt, p0, p1 = vector
        if d != 0:
            v = convertAngle(v)
            print("[%d], %.2fm, %.2f, " % (index, d, degrees(v)) + p0.s(offset)+ ' ' + litt)
            coords.append((p0.x+dx, p0.y+dy))
            index += 1
            p0 = p1
    print("[%d], ---, ---, " % index + p0.s(offset))
    coords.append((p0.x+dx, p0.y+dy))
    
    v1 = degrees(v1)
    v2 = degrees(v2)
    print('R: %.1fm, l: %.2fm, v1: %.2f, v2: %.2f' %(R, l, v1, v2))

    return coords
