using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PythonConsole
{
    public static class VectUtil
    {
        public static void DirectionVectorsFromMiddlePos(Vector3 startPos, Vector3 endPos, Vector3 middlePos, out Vector3 startDir, out Vector3 endDir)
        {
            startDir = (middlePos - startPos).FlatNormalized();
            endDir = (middlePos - endPos).FlatNormalized();
        }
    }
}
