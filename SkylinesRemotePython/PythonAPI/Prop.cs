using SkylinesPythonShared;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API
{
    public class Prop
    {
        public int id { get; private set; }

        public string prefab_name { get; private set; }

        public Vector position { get; private set; }

        public double angle { get; private set; }
        internal Prop(object obj)
        {
            PropMessage msg = (PropMessage)obj;
            id = msg.id;
            prefab_name = msg.prefab_name;
            position = msg.position;
            angle = msg.angle;
        }
    }
}
