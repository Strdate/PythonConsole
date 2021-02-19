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

        public Vector pos => position;

        public Vector position { get; private set; }

        public int elevation { get; private set; }

        private CachedObj<List<Segment>> _cachedSegments;
        public List<Segment> segments => _cachedSegments.Get;

        public override void refresh()
        {
            AssignData(api.client.RemoteCall<NetNodeMessage>(Contracts.GetNodeFromId, id));
        }

        internal void AssignData(NetNodeMessage msg)
        {
            if (msg == null) {
                deleted = true;
                return;
            }
            id = msg.id;
            prefab_name = msg.prefab_name;
            position = msg.position;
            elevation = msg.elevation;
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
