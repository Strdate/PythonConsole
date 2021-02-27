using SkylinesPythonShared;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesRemotePython.API
{
    [Doc("Node structure - junction of Segments (roads/power lines etc.)")]
    public class Node : CitiesObject
    {
        public override string type => "node";

        [Doc("Node asset name (eg. 'Basic Road')")]
        public string prefab_name { get; private set; }

        [Doc("Node asset object (eg. 'Basic Road')")]
        public NetPrefab prefab => NetPrefab.GetNetPrefab(prefab_name, api);

        [Doc("Elevation over terrain or water level")]
        public double terrain_offset { get; private set; }

        [Doc("ID of building (usually pillar)")]
        public int building_id { get; private set; }

        [Doc("Node building (usually pillar)")]
        public Building building => building_id == 0 ? null : new Building(api.client.RemoteCall<BuildingMessage>(Contracts.GetBuildingFromId, (uint)building_id), api);

        [Doc("Count of adjacent segments")]
        public int seg_count { get; private set; }

        [Doc("Move to new position")]
        public void move(IPositionable pos) => MoveImpl(pos.position, null);

        private CachedObj<List<Segment>> _cachedSegments;

        [Doc("List of adjacent segments")]
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
            terrain_offset = msg.terrain_offset;
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
    }
}
