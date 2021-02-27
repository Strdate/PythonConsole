using SkylinesPythonShared;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API
{
    [Doc("Structure holding type of road/network (eg. 'Basic Road' or 'Power Line')")]
    public class NetPrefab : ApiRefObject, ISimpleToString
    {
        [Doc("Network name")]
        public string name { get; private set; }

        [Doc("Network width")]
        public float width { get; private set; }

        [Doc("Is elevated/bridge")]
        public bool is_overground { get; private set; }

        [Doc("Is tunnel")]
        public bool is_underground { get; private set; }

        [Doc("Count of forward vehicle lanes")]
        public int fw_vehicle_lane_count { get; private set; }

        [Doc("Count of backward vehicle lanes")]
        public int bw_vehicle_lane_count { get; private set; }

        protected void AssignData(NetPrefabMessage msg)
        {
            name = msg.name;
            width = msg.width;
            is_overground = msg.is_overground;
            is_underground = msg.is_underground;
            fw_vehicle_lane_count = msg.fw_vehicle_lane_count;
            bw_vehicle_lane_count = msg.bw_vehicle_lane_count;
        }

        protected NetPrefab(NetPrefabMessage obj, GameAPI api) : base(api)
        {
            AssignData(obj);
        }

        internal static NetPrefab GetNetPrefab(string name, GameAPI api)
        {
            return new NetPrefab(api.client.RemoteCall<NetPrefabMessage>(Contracts.GetNetPrefabFromName, name), api);
        }

        public override string ToString()
        {
            return PythonHelp.RuntimeToString(this);
        }

        public string SimpleToString()
        {
            return name;
        }
    }
}
