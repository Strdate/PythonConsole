using SkylinesPythonShared;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API
{
    public class GameAPI
    {
        internal ClientHandler client;
        public GameAPI(ClientHandler client)
        {
            this.client = client;
        }

        public Prop get_prop(int id)
        {
            return new Prop(client.RemoteCall<PropMessage>(Contracts.GetPropFromId, id), this);
        }

        public Tree get_tree(long id)
        {
            return new Tree(client.RemoteCall<TreeMessage>(Contracts.GetTreeFromId, id), this);
        }

        public Building get_building(int id)
        {
            return new Building(client.RemoteCall<BuildingMessage>(Contracts.GetBuildingFromId, id), this);
        }

        public NetNode get_node(int id)
        {
            return new NetNode(client.RemoteCall<NetNodeMessage>(Contracts.GetNodeFromId, id), this);
        }

        public Segment get_segment(int id)
        {
            return new Segment(client.RemoteCall<NetSegmentMessage>(Contracts.GetSegmentFromId, id), this);
        }

        public Prop create_prop(Vector position, string type, double angle = 0)
        {
            var msg = new CreatePropMessage()
            {
                Position = position,
                Type = type,
                Angle = angle
            };

            return new Prop(client.RemoteCall<PropMessage>(Contracts.CreateProp, msg), this);
        }

        public Tree create_tree(Vector position, string prefab_name)
        {
            var msg = new CreateTreeMessage() {
                Position = position,
                prefab_name = prefab_name
            };

            return new Tree(client.RemoteCall<TreeMessage>(Contracts.CreateTree, msg), this);
        }

        public Building create_building(Vector position, string type, double angle = 0)
        {
            var msg = new CreateBuildingMessage() {
                Position = position,
                Type = type,
                Angle = angle
            };

            return new Building(client.RemoteCall<BuildingMessage>(Contracts.CreateBuilding, msg), this);
        }

        public NetNode create_node(Vector position, string prefab)
        {
            CreateNodeMessage msg = new CreateNodeMessage()
            {
                Position = position,
                Type = prefab
            };

            return new NetNode(client.RemoteCall<NetNodeMessage>(Contracts.CreateNode, msg), this);
        }

        public Segment create_segment(object startNode, object endNode, object type)
        {
            return CreateSegmentImpl(startNode, endNode, type, null, null, null);
        }

        public Segment create_segment(object startNode, object endNode, object type, Vector middle_pos)
        {
            return CreateSegmentImpl(startNode, endNode, type, null, null, middle_pos);
        }

        public Segment create_segment(object startNode, object endNode, object type, Vector start_dir, Vector end_dir)
        {
            return CreateSegmentImpl(startNode, endNode, type, start_dir, end_dir, null);
        }

        private Segment CreateSegmentImpl(object startNode, object endNode, object type, Vector start_dir, Vector end_dir, Vector middle_pos)
        {
            if(!(startNode is NetNode) && !(startNode is Vector)) {
                throw new Exception("Segment startNode must be NetNode or Vector, not " + startNode.GetType().Name);
            }
            if (!(endNode is NetNode) && !(endNode is Vector)) {
                throw new Exception("Segment endNode must be NetNode or Vector, not " + endNode.GetType().Name);
            }
            if (!(type is string) && !(type is NetOptions)) {
                throw new Exception("Segment type must be prefab name or NetOptions object, not " + type.GetType().Name);
            }
            NetOptions options = type as NetOptions ?? new NetOptions((string) type);
            CreateSegmentMessage msg = new CreateSegmentMessage() {
                start_node_id = startNode is NetNode ? ((NetNode)startNode).id : (ushort)0,
                end_node_id = endNode is NetNode ? ((NetNode)endNode).id : (ushort)0,
                start_postition = startNode is Vector ? (Vector)startNode : null,
                end_postition = endNode is Vector ? (Vector)endNode : null,
                net_options = options,
                start_dir = start_dir,
                end_dir = end_dir,
                middle_pos = middle_pos
            };
            return new Segment(client.RemoteCall<NetSegmentMessage>(Contracts.CreateSegment, msg), this);
        }

        public IList<Segment> create_segments(object startNode, object endNode, object type)
        {
            return CreateSegmentsImpl(startNode, endNode, type, null, null, null);
        }

        public IList<Segment> create_segments(object startNode, object endNode, object type, Vector middle_pos)
        {
            return CreateSegmentsImpl(startNode, endNode, type, null, null, middle_pos);
        }

        public IList<Segment> create_segments(object startNode, object endNode, object type, Vector start_dir, Vector end_dir)
        {
            return CreateSegmentsImpl(startNode, endNode, type, start_dir, end_dir, null);
        }

        private IList<Segment> CreateSegmentsImpl(object startNode, object endNode, object type, Vector start_dir, Vector end_dir, Vector middle_pos, bool autoSplit = false)
        {
            if (!(startNode is NetNode) && !(startNode is Vector)) {
                throw new Exception("Segment startNode must be NetNode or Vector, not " + startNode.GetType().Name);
            }
            if (!(endNode is NetNode) && !(endNode is Vector)) {
                throw new Exception("Segment endNode must be NetNode or Vector, not " + endNode.GetType().Name);
            }
            if (!(type is string) && !(type is NetOptions)) {
                throw new Exception("Segment type must be prefab name or NetOptions object, not " + type.GetType().Name);
            }
            NetOptions options = type as NetOptions ?? new NetOptions((string)type);
            CreateSegmentMessage msg = new CreateSegmentMessage() {
                start_node_id = startNode is NetNode ? ((NetNode)startNode).id : (ushort)0,
                end_node_id = endNode is NetNode ? ((NetNode)endNode).id : (ushort)0,
                start_postition = startNode is Vector ? (Vector)startNode : null,
                end_postition = endNode is Vector ? (Vector)endNode : null,
                net_options = options,
                start_dir = start_dir,
                end_dir = end_dir,
                middle_pos = middle_pos,
                auto_split = true
            };
            return NetLogic.PrepareSegmentList(client.RemoteCall<NetSegmentListMessage>(Contracts.CreateSegments, msg).list, this);
        }

        public bool is_prefab(string name)
        {
            return client.RemoteCall<bool>(Contracts.ExistsPrefab, name);
        }

        public float terrain_height(Vector pos)
        {
            return client.RemoteCall<float>(Contracts.GetTerrainHeight, pos);
        }

        public override string ToString()
        {
            return "Provides API to manipulate in-game objects, such as buildings or roads";
        }
    }
}
