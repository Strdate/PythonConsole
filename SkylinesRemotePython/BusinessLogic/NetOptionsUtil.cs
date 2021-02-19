using SkylinesPythonShared.API;
using SkylinesRemotePython.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython
{
    public static class NetOptionsUtil
    {
        internal static NetOptions Ensure(object obj)
        {
            if (!(obj is string) && !(obj is NetOptions) && !(obj is NetPrefab)) {
                throw new Exception("Segment type must be prefab name, NetPrefab or NetOptions object - not " + obj.GetType().Name);
            }
            NetOptions options = obj as NetOptions ?? new NetOptions(obj is NetPrefab ? ((NetPrefab)obj).name : (string)obj);
            return options;
        }
    }
}
