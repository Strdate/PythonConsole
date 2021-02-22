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

        [Doc("Elevation mode: default, ground, elevated, bridge, tunnel, slope")]
        public string elevation_mode { get; private set; }

        [Doc("Invert road (eg. flip direction of one-way streets)")]
        public bool invert { get; private set; }

        [Doc("Spacing betwwen two nodes (pillars). Default 100")]
        public int node_spacing { get; private set; }

        [Doc("Example call: NetOptions(\"Basic Road\", \"elevated\", true)")]
        public NetOptions(string prefab_name, string elevation_mode = "default", bool invert = false, int node_spacing = 100)
        {
            this.prefab_name = prefab_name;
            this.elevation_mode = elevation_mode;
            this.invert = invert;
            this.node_spacing = node_spacing;
        }

        public override string ToString()
        {
            return Util.RuntimeToString(this);
            /*return "{" + "\n" +
                "type: " + nameof(NetOptions) + "\n" +
                "prefab_name: " + prefab_name + "\n" +
                "elevation_mode: " + elevation_mode + "\n" +
                "invert: " + invert + "\n" +
                "node_spacing: " + node_spacing + "\n" +
                "}";*/
        }

        public string SimpleToString()
        {
            return ToString().Replace("\n","\n\n");
        }
    }
}
