using SkylinesPythonShared;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API
{
    [Doc("Contains basic set of functions for interaction with Cities:Skylines")]
    public class GameAPI
    {
        internal ClientHandler client;
        internal NetLogic _netLogic;
        internal GameAPI(ClientHandler client)
        {
            this.client = client;
            _netLogic = new NetLogic(this);
        }

        [Doc("Returns prop object from its id")]
        public Prop get_prop(int id)
        {
            return new Prop(client.RemoteCall<PropMessage>(Contracts.GetPropFromId, (uint)id), this);
        }

        [Doc("Returns tree object from its id")]
        public Tree get_tree(long id)
        {
            return new Tree(client.RemoteCall<TreeMessage>(Contracts.GetTreeFromId, (uint)id), this);
        }

        [Doc("Returns building object from its id")]
        public Building get_building(int id)
        {
            return new Building(client.RemoteCall<BuildingMessage>(Contracts.GetBuildingFromId, (uint)id), this);
        }

        [Doc("Returns node object from its id")]
        public Node get_node(int id) => Node.GetNetNode((uint)id, this);

        [Doc("Returns segment object from its id")]
        public Segment get_segment(int id)
        {
            return new Segment(client.RemoteCall<NetSegmentMessage>(Contracts.GetSegmentFromId, (uint)id), this);
        }

        [Doc("Creates prop")]
        public Prop create_prop(Vector position, string prefab_name, double angle = 0)
        {
            var msg = new CreatePropMessage()
            {
                Position = position,
                Type = prefab_name,
                Angle = angle
            };

            return new Prop(client.RemoteCall<PropMessage>(Contracts.CreateProp, msg), this);
        }

        [Doc("Creates tree")]
        public Tree create_tree(Vector position, string prefab_name)
        {
            var msg = new CreateTreeMessage() {
                Position = position,
                prefab_name = prefab_name
            };

            return new Tree(client.RemoteCall<TreeMessage>(Contracts.CreateTree, msg), this);
        }

        [Doc("Creates building")]
        public Building create_building(Vector position, string type, double angle = 0)
        {
            var msg = new CreateBuildingMessage() {
                Position = position,
                Type = type,
                Angle = angle
            };

            return new Building(client.RemoteCall<BuildingMessage>(Contracts.CreateBuilding, msg), this);
        }

        [Doc("Creates node (eg. segment junction)")]
        public Node create_node(Vector position, object prefab)
        {
            if(!(prefab is string) && !(prefab is NetPrefab)) {
                throw new Exception("Prefab must be string or NetPrefab");
            }
            CreateNodeMessage msg = new CreateNodeMessage() {
                Position = position,
                Type = prefab is NetPrefab ? ((NetPrefab)prefab).name : (string)prefab
            };

            return new Node(client.RemoteCall<NetNodeMessage>(Contracts.CreateNode, msg), this);
        }

        [Doc("Creates straight segment (road). Don't use this method, but CreateSegments(..)")]
        public Segment create_segment(IPositionable startNode, IPositionable endNode, object type)
        {
            return _netLogic.CreateSegmentImpl(startNode, endNode, type, null, null, null);
        }

        [Doc("Creates segment (road). Don't use this method, but CreateSegments(..)")]
        public Segment create_segment(IPositionable startNode, IPositionable endNode, object type, Vector middle_pos)
        {
            return _netLogic.CreateSegmentImpl(startNode, endNode, type, null, null, middle_pos);
        }

        [Doc("Creates segment (road). Don't use this method, but CreateSegments(..)")]
        public Segment create_segment(IPositionable startNode, IPositionable endNode, object type, Vector start_dir, Vector end_dir)
        {
            return _netLogic.CreateSegmentImpl(startNode, endNode, type, start_dir, end_dir, null);
        }

        [Doc("Starts a road on a given point. Call path_to(..) on the returned object to build a road")]
        public PathBuilder begin_path(IPositionable startNode, object options = null)
        {
            return PathBuilder.BeginPath(this, startNode, options);
        }

        [Doc("Creates straight segment (road) and automatically splits it in smaller pieces")]
        public IList<Segment> create_segments(IPositionable startNode, IPositionable endNode, object type)
        {
            return _netLogic.CreateSegmentsImpl(startNode, endNode, type, null, null, null);
        }

        [Doc("Creates segment (road) and automatically splits it in smaller pieces")]
        public IList<Segment> create_segments(IPositionable startNode, IPositionable endNode, object type, Vector middle_pos)
        {
            return _netLogic.CreateSegmentsImpl(startNode, endNode, type, null, null, middle_pos);
        }

        [Doc("Creates segment (road) and automatically splits it in smaller pieces")]
        public IList<Segment> create_segments(IPositionable startNode, IPositionable endNode, object type, Vector start_dir, Vector end_dir)
        {
            return _netLogic.CreateSegmentsImpl(startNode, endNode, type, start_dir, end_dir, null);
        }

        [Doc("Returns network prefab (used to build nodes and segments). Eg. 'Basic road'")]
        public NetPrefab get_net_prefab(string name)
        {
            return NetPrefab.GetNetPrefab(name, this);
        }

        [Doc("Is name valid prefab (network, building, tree etc.)")]
        public bool is_prefab(string name)
        {
            return client.RemoteCall<bool>(Contracts.ExistsPrefab, name);
        }

        [Doc("Returns terrain height at a given point (height is Y coord)")]
        public float terrain_height(Vector pos)
        {
            return client.RemoteCall<float>(Contracts.GetTerrainHeight, pos);
        }

        [Doc("Draws line on map. Returns handle which can be used to delete the line. Use clear() to delete all lines")]
        public RenderableObjectHandle draw_line(IPositionable vector, IPositionable origin, string color = "red", double length = 20, double size = 0.1)
        {
            return new RenderableObjectHandle(client.RemoteCall<int>(Contracts.RenderVector, new RenderVectorMessage() {
                vector = vector.position,
                origin = origin.position,
                color = color,
                length = (float)length,
                size = (float)size
            }), this);
        }

        public delegate void __HelpDeleg(object obj = null);

        [Doc("Prints help for given object")]
        public void help(object obj = null)
        {
            obj = obj ?? this;
            string text = PythonHelp.GetHelp(obj);
            client.SendMessage(text, "c_output_message");
        }

        [Doc("Clears all lines drawn on map")]
        public void clear()
        {
            client.RemoteVoidCall(Contracts.RemoveRenderedObject, 0);
        }

        public override string ToString()
        {
            return PythonHelp.GetHelp(this);
        }
    }
}
