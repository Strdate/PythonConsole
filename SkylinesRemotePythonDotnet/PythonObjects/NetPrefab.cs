using SkylinesPythonShared;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API
{
    // todo - rewrite to shell object
    [Doc("Structure holding type of road/network (eg. 'Basic Road' or 'Power Line')")]
    public class NetPrefab : CitiesObjectBase<NetPrefabData, NetPrefab, string>, ISimpleToString
    {
        public override string type => "net prefab";

        private protected override CitiesObjectStorage<NetPrefabData, NetPrefab, string> GetStorage()
        {
            return ObjectStorage.Instance.NetPrefabs;
        }

        [Doc("Network name")]
        public string name => _.id;

        [Doc("Network width")]
        public float width => _.width;

        [Doc("Is elevated/bridge")]
        public bool is_overground => _.is_overground;

        [Doc("Is tunnel")]
        public bool is_underground => _.is_underground;

        [Doc("Count of forward vehicle lanes")]
        public int fw_vehicle_lane_count => _.fw_vehicle_lane_count;

        [Doc("Count of backward vehicle lanes")]
        public int bw_vehicle_lane_count => _.bw_vehicle_lane_count;

        public string SimpleToString()
        {
            return name;
        }

        public NetPrefab()
        {
            if (!CitiesObjectController.AllowInstantiation) {
                throw new Exception("Instantiation is not allowed!");
            }
        }
    }
}
