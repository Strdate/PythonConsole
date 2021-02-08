using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
