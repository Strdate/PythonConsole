using SkylinesPythonShared;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API
{
    public class Segment : CitiesObject
    {
        public override string type => "segment";

        public string prefab_name { get; private set; }

        public NetPrefab prefab => NetPrefab.GetNetPrefab(prefab_name, api);

        public int start_node_id { get; private set; }

        public int end_node_id { get; private set; }

        public Vector middle_pos {
            get => position;
            set => position = value;
        }

        public override Vector position {
            get => _position;
            set => MoveImpl(value, null);
        }
        public float length { get; private set; }

        public Node start_node {
            get => Node.GetNetNode((uint)start_node_id, api);
        }

        public Node end_node {
            get => Node.GetNetNode((uint)end_node_id, api);
        }

        public Node get_other_node(object node)
        {
            uint? nodeId = node as uint?;
            if(nodeId == null) {
                Node netNode = (Node)node;
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

        internal override void AssignData(InstanceMessage data)
        {
            NetSegmentMessage msg = data as NetSegmentMessage;
            if(msg == null) {
                deleted = true;
                return;
            }
            id = msg.id;
            prefab_name = msg.prefab_name;
            start_node_id = msg.start_node_id;
            end_node_id = msg.end_node_id;
            length = msg.length;
            _position = msg.middle_pos;
        }

        internal Segment(NetSegmentMessage obj, GameAPI api) : base(api)
        {
            AssignData(obj);
        }
    }
}
