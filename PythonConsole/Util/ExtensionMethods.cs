using ColossalFramework.Math;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PythonConsole
{
    public static class ExtensionMethods
    {
        public static UnityEngine.Vector3 ToUnity(this SkylinesPythonShared.API.Vector vect)
        {
            return new UnityEngine.Vector3((float)vect.x, (float)vect.y, (float)vect.z);
        }

        public static SkylinesPythonShared.API.Vector FromUnity(this UnityEngine.Vector3 vect)
        {
            return new SkylinesPythonShared.API.Vector(vect.x, vect.y, vect.z);
        }

        public static Vector3 FlatNormalized(this Vector3 vect)
        {
            Vector3 newVect = vect;
            newVect.y = 0;
            newVect.Normalize();
            return newVect;
        }

        public static Bezier FromUnity(this Bezier3 x)
        {
            return new Bezier(x.a.FromUnity(), x.b.FromUnity(), x.c.FromUnity(), x.d.FromUnity());
        }

        public static Bezier3 ShiftTo3D(this Bezier2 bezier)
        {
            Vector3 v1 = new Vector3(bezier.a.x, 0, bezier.a.y);
            Vector3 v2 = new Vector3(bezier.b.x, 0, bezier.b.y);
            Vector3 v3 = new Vector3(bezier.c.x, 0, bezier.c.y);
            Vector3 v4 = new Vector3(bezier.d.x, 0, bezier.d.y);
            return new Bezier3(v1, v2, v3, v4);
        }
    }
}
