using SkylinesPythonShared;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API
{
    [Doc("Network object (road, pipe, power line etc.)")]
    public class Segment : CitiesObject<NetSegmentData, Segment>
    {
        public override string type => "segment";

        private protected override CitiesObjectStorage<NetSegmentData, Segment, uint> GetStorage()
        {
            return ObjectStorage.Instance.Segments;
        }

        [Doc("Network asset")]
        public NetPrefab prefab => ObjectStorage.Instance.NetPrefabs.GetById(prefab_name);

        [ToStringIgnore]
        [Doc("ID of start node (junction)")]
        public int start_node_id => _.start_node_id;

        [ToStringIgnore]
        [Doc("ID of end node (junction)")]
        public int end_node_id => _.end_node_id;

        [Doc("Road direction at start node")]
        public Vector start_dir => _.start_dir;

        [Doc("Road direction at end node")]
        public Vector end_dir => _.end_dir;

        [Doc("Underlying bezier shape")]
        public Bezier bezier => _.bezier;

        [Doc("Road middle position")]
        public Vector middle_pos {
            get => position;
            set => position = value;
        }

        [Doc("Moves bezier control point to the new position")]
        public void move(IPositionable pos) => MoveImpl(pos.position, null);

        [Doc("Road length")]
        public float length => _.length;

        [Doc("Is segment straight")]
        public bool is_straight => _.is_straight;

        [Doc("Road start node (junction)")]
        public Node start_node {
            get => ObjectStorage.Instance.Nodes.GetById((uint)start_node_id);
        }

        [Doc("Road end node (junction)")]
        public Node end_node {
            get => ObjectStorage.Instance.Nodes.GetById((uint)end_node_id);
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
        public void delete(bool keep_nodes = false)
        {
            if (!exists) {
                return;
            }
            GetStorage().Delete(id, keep_nodes);
        }

        public override void refresh()
        {
            ObjectStorage.Instance.Segments.RefreshInstance(id);
        }

        public Segment()
        {
            if (!CitiesObjectController.AllowInstantiation) {
                throw new Exception("Instantiation is not allowed!");
            }
        }
    }
}
