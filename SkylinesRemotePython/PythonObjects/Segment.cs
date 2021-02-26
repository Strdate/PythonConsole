using SkylinesPythonShared;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API
{
    [Doc("Network object (road, pipe, power line etc.)")]
    public class Segment : CitiesObject
    {
        public override string type => "segment";

        [ToStringIgnore]
        [Doc("Network asset name (eg. 'Highway')")]
        public string prefab_name { get; private set; }

        [Doc("Network asset")]
        public NetPrefab prefab => NetPrefab.GetNetPrefab(prefab_name, api);

        [ToStringIgnore]
        [Doc("ID of start node (junction)")]
        public int start_node_id { get; private set; }

        [ToStringIgnore]
        [Doc("ID of end node (junction)")]
        public int end_node_id { get; private set; }

        [Doc("Road direction at start node")]
        public Vector start_dir { get; private set; }

        [Doc("Road direction at end node")]
        public Vector end_dir { get; private set; }

        [Doc("Underlying bezier shape")]
        public Bezier bezier { get; private set; }

        [Doc("Road middle position")]
        public Vector middle_pos {
            get => position;
            set => position = value;
        }

        [Doc("Moves bezier control point to the new position")]
        public void move(IPositionable pos) => MoveImpl(pos.position, null);

        [Doc("Road length")]
        public float length { get; private set; }

        [Doc("Is segment straight")]
        public bool is_straight { get; private set; }

        [Doc("Road start node (junction)")]
        public Node start_node {
            get => Node.GetNetNode((uint)start_node_id, api);
        }

        [Doc("Road end node (junction)")]
        public Node end_node {
            get => Node.GetNetNode((uint)end_node_id, api);
        }

        [Doc("Returns the other junction given the first one")]
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

        [Doc("Deletes the road. keep_nodes param specifies if the nodes should be deleted too if there are no roads left that connect to them")]
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
            start_dir = msg.start_dir;
            end_dir = msg.end_dir;
            length = msg.length;
            _position = msg.middle_pos;
            bezier = msg.bezier;
            is_straight = msg.is_straight;
        }

        internal Segment(NetSegmentMessage obj, GameAPI api) : base(api)
        {
            AssignData(obj);
        }
    }
}
