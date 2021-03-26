using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared.API
{
    [Serializable]
    [Doc("Network options object")]
    public class NetOptions : ISimpleToString
    {
        [Doc("Network type (eg. 'Basic Road')")]
        public string prefab_name { get; private set; }

        [Doc("Supported values: True (follow), False (do not follow - default), \"auto_offset\" - Keeps start and end node at given elevation and interpolates elevation of intermediate nodes")]
        public string follow_terrain { get; private set; }

        [Doc("Elevation mode: default, ground, elevated, bridge, tunnel, slope")]
        public string elevation_mode { get; private set; }

        [Doc("Invert road (eg. flip direction of one-way streets)")]
        public bool invert { get; private set; }

        [Doc("Spacing betwwen two nodes (pillars). Default 100")]
        public int node_spacing { get; private set; }

        [Doc("Example call: NetOptions(\"Basic Road\", false, \"elevated\", true)")]
        public NetOptions(string prefab_name, object follow_terrain = null, string elevation_mode = "default", bool invert = false, int node_spacing = 100)
        {
            follow_terrain = follow_terrain ?? "false";
            string follow_terrain_string = follow_terrain as string;
            if(follow_terrain is bool bl) {
                follow_terrain_string = bl ? "true" : "false";
            }
            this.prefab_name = prefab_name;
            this.follow_terrain = follow_terrain_string;
            this.elevation_mode = elevation_mode.ToLower();
            this.invert = invert;
            this.node_spacing = node_spacing;
        }

        public override string ToString()
        {
            return Util.RuntimeToString(this);
        }

        public string SimpleToString()
        {
            return ToString();
        }
    }
}
