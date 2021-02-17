using SkylinesPythonShared;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesRemotePython.API
{
    public class NetNode : ObjectAPI
    {
        public override string type => "node";

        public string prefab_name { get; private set; }

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
                is_deleted = true;
                return;
            }
            id = msg.id;
            prefab_name = msg.prefab_name;
            position = msg.position;
            elevation = msg.elevation;
            _cachedSegments = new CachedObj<List<Segment>>(() => api.client.RemoteCall<List<NetSegmentMessage>>(Contracts.GetSegmentsForNodeId, id).Select((obj) => new Segment(obj, api)).ToList()); ;
        }

        internal NetNode(NetNodeMessage obj, GameAPI api) : base(api)
        {
            AssignData(obj);
        }

        internal static NetNode GetNetNode(int id, GameAPI api)
        {
            return new NetNode(api.client.RemoteCall<NetNodeMessage>(Contracts.GetNodeFromId, id), api);
        }
    }
}
