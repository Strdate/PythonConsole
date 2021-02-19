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
            get => NetNode.GetNetNode((uint)start_node_id, api);
        }

        public NetNode end_node {
            get => NetNode.GetNetNode((uint)end_node_id, api);
        }

        public NetNode get_other_node(object node)
        {
            uint? nodeId = node as uint?;
            if(nodeId == null) {
                NetNode netNode = (NetNode)node;
                nodeId = netNode.id;
            }
            if(start_node_id == nodeId) {
                return end_node;
            }
            if (end_node_id == nodeId) {
                return start_node;
            }
            return null;
        }

        public bool delete(bool keep_nodes)
        {
            if (deleted) {
                return true;
            }
            api.client.RemoteCall<bool>(Contracts.DeleteObject, new DeleteObjectMessage() {
                id = id,
                type = type,
                keep_nodes = keep_nodes
            });
            refresh();
            return deleted;
        }

        public override void refresh()
        {
            AssignData(api.client.RemoteCall<NetSegmentMessage>(Contracts.GetSegmentFromId, id));
        }

        internal void AssignData(NetSegmentMessage msg)
        {
            if(msg == null) {
                deleted = true;
                return;
            }
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
