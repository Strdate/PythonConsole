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
        internal NetLogic _netLogic;
        public GameAPI(ClientHandler client)
        {
            this.client = client;
            _netLogic = new NetLogic(this);
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

        public NetNode get_node(int id) => NetNode.GetNetNode((uint)id, this);

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
            return _netLogic.CreateSegmentImpl(startNode, endNode, type, null, null, null);
        }

        public Segment create_segment(object startNode, object endNode, object type, Vector middle_pos)
        {
            return _netLogic.CreateSegmentImpl(startNode, endNode, type, null, null, middle_pos);
        }

        public Segment create_segment(object startNode, object endNode, object type, Vector start_dir, Vector end_dir)
        {
            return _netLogic.CreateSegmentImpl(startNode, endNode, type, start_dir, end_dir, null);
        }

        public PathBuilder begin_path(object startNode, object options = null)
        {
            return PathBuilder.BeginPath(this, startNode, options);
        }

        public IList<Segment> create_segments(object startNode, object endNode, object type)
        {
            return _netLogic.CreateSegmentsImpl(startNode, endNode, type, null, null, null);
        }

        public IList<Segment> create_segments(object startNode, object endNode, object type, Vector middle_pos)
        {
            return _netLogic.CreateSegmentsImpl(startNode, endNode, type, null, null, middle_pos);
        }

        public IList<Segment> create_segments(object startNode, object endNode, object type, Vector start_dir, Vector end_dir)
        {
            return _netLogic.CreateSegmentsImpl(startNode, endNode, type, start_dir, end_dir, null);
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
