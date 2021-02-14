using SkylinesPythonShared;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API
{
    public class Segment : ObjectAPI
    {
        public override string type => "segment";

        public string prefab_name { get; private set; }

        public int start_node_id { get; private set; }

        public int end_node_id { get; private set; }

        public Vector middle_pos { get; private set; }

        public float length { get; private set; }

        public NetNode start_node {
            get {
                return new NetNode(api.client.RemoteCall<NetNodeMessage>(Contracts.GetNodeFromId, start_node_id), api);
            }
        }

        public NetNode end_node {
            get {
                return new NetNode(api.client.RemoteCall<NetNodeMessage>(Contracts.GetNodeFromId, end_node_id), api);
            }
        }

        public void refresh()
        {
            AssignData(api.client.RemoteCall<NetSegmentMessage>(Contracts.GetSegmentFromId, id));
        }

        internal void AssignData(NetSegmentMessage msg)
        {
            id = msg.id;
            prefab_name = msg.prefab_name;
            start_node_id = msg.start_node_id;
            end_node_id = msg.end_node_id;
            length = msg.length;
            middle_pos = msg.middle_pos;
        }

        internal Segment(NetSegmentMessage obj, GameAPI api) : base(api)
        {
            AssignData(obj);
        }
    }
}
