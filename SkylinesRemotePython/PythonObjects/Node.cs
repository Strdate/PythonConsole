using SkylinesPythonShared;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesRemotePython.API
{
    public class Node : CitiesObject
    {
        public override string type => "node";

        public string prefab_name { get; private set; }

        public NetPrefab prefab => NetPrefab.GetNetPrefab(prefab_name, api);

        public int elevation { get; private set; }

        public int building_id { get; private set; }

        public Building building => building_id == 0 ? null : new Building(api.client.RemoteCall<BuildingMessage>(Contracts.GetBuildingFromId, (uint)building_id), api);

        public int seg_count { get; private set; }

        public void move(IPositionable pos) => MoveImpl(pos.position, null);

        private CachedObj<List<Segment>> _cachedSegments;
        public List<Segment> segments => _cachedSegments.Get;

        public override void refresh()
        {
            AssignData(api.client.RemoteCall<NetNodeMessage>(Contracts.GetNodeFromId, id));
        }

        internal override void AssignData(InstanceMessage data)
        {
            NetNodeMessage msg = data as NetNodeMessage;
            if (msg == null) {
                deleted = true;
                return;
            }
            id = msg.id;
            prefab_name = msg.prefab_name;
            _position = msg.position;
            elevation = msg.elevation;
            building_id = msg.building_id;
            seg_count = msg.seg_count;
            _cachedSegments = new CachedObj<List<Segment>>(() => api.client.RemoteCall<List<NetSegmentMessage>>(Contracts.GetSegmentsForNodeId, id).Select((obj) => new Segment(obj, api)).ToList()); ;
        }

        internal Node(NetNodeMessage obj, GameAPI api) : base(api)
        {
            AssignData(obj);
        }

        internal static Node GetNetNode(uint id, GameAPI api)
        {
            return new Node(api.client.RemoteCall<NetNodeMessage>(Contracts.GetNodeFromId, id), api);
        }

        public override string ToString()
        {
            return "{" + "\n" +
                "type: " + type + "\n" +
                "id: " + id + "\n" +
                "position: " + pos + "\n" +
                "prefab_name: " + prefab_name + "\n" +
                "seg_count: " + seg_count + "\n" +
                "}";
        }
    }
}
