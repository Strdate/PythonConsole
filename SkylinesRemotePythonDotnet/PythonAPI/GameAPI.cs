using Microsoft.Scripting.Hosting;
using SkylinesPythonShared;
using SkylinesPythonShared.API;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API
{
    [Doc("Contains basic set of functions for interaction with Cities:Skylines")]
    public class GameAPI
    {
        internal ClientHandler client;
        internal NetLogic _netLogic;
        internal ScriptScope _scope;

        internal GameAPI(ClientHandler client, ScriptScope scope)
        {
            this.client = client;
            _netLogic = new NetLogic(this);
            _scope = scope;
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
        public Prop create_prop(IPositionable position, string prefab_name, double angle = 0)
        {
            var msg = new CreatePropMessage()
            {
                Position = position.position,
                Type = prefab_name,
                Angle = angle
            };

            return new Prop(client.RemoteCall<PropMessage>(Contracts.CreateProp, msg), this);
        }

        [Doc("Creates tree")]
        public Tree create_tree(IPositionable position, string prefab_name)
        {
            var msg = new CreateTreeMessage() {
                Position = position.position,
                prefab_name = prefab_name
            };

            return new Tree(client.RemoteCall<TreeMessage>(Contracts.CreateTree, msg), this);
        }

        [Doc("Creates building")]
        public Building create_building(IPositionable position, string type, double angle = 0)
        {
            var msg = new CreateBuildingMessage() {
                Position = position.position,
                Type = type,
                Angle = angle
            };

            return new Building(client.RemoteCall<BuildingMessage>(Contracts.CreateBuilding, msg), this);
        }

        [Doc("Creates node (eg. segment junction)")]
        public Node create_node(IPositionable position, object prefab)
        {
            if(!(prefab is string) && !(prefab is NetPrefab)) {
                throw new Exception("Prefab must be string or NetPrefab");
            }
            CreateNodeMessage msg = new CreateNodeMessage() {
                Position = position.position,
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
        public Segment create_segment(IPositionable startNode, IPositionable endNode, object type, IPositionable control_point)
        {
            return _netLogic.CreateSegmentImpl(startNode, endNode, type, null, null, control_point);
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

        [Doc("Creates set of straight segments (road). Returns array of created segments")]
        public IList<Segment> create_segments(IPositionable startNode, IPositionable endNode, object type)
        {
            return _netLogic.CreateSegmentsImpl(startNode, endNode, type, null, null, null);
        }

        [Doc("Creates set of straight segments (road) with specified control point of the underlying bezier curve")]
        public IList<Segment> create_segments(IPositionable startNode, IPositionable endNode, object type, IPositionable control_point)
        {
            return _netLogic.CreateSegmentsImpl(startNode, endNode, type, null, null, control_point);
        }

        [Doc("Creates set of straight segments (road) with specified direction vectors at the start and end position")]
        public IList<Segment> create_segments(IPositionable startNode, IPositionable endNode, object type, Vector start_dir, Vector end_dir)
        {
            return _netLogic.CreateSegmentsImpl(startNode, endNode, type, start_dir, end_dir, null);
        }

        [Doc("Returns network prefab (used to build nodes and segments). Eg. 'Basic road'")]
        public NetPrefab get_net_prefab(string name)
        {
            return NetPrefab.GetNetPrefab(name, this);
        }

        [Doc("Returns if name is a valid prefab (network, building, tree etc.)")]
        public bool is_prefab(string name)
        {
            return client.RemoteCall<bool>(Contracts.ExistsPrefab, name);
        }

        [Doc("Returns terrain height at a given point (height is Y coord)")]
        public float terrain_height(IPositionable pos)
        {
            return client.RemoteCall<float>(Contracts.GetTerrainHeight, pos.position);
        }

        [Doc("Returns terrain height inlucing water level at a given point")]
        public float surface_level(IPositionable pos)
        {
            return client.RemoteCall<float>(Contracts.GetWaterLevel, pos.position);
        }

        [Doc("Draws line on map. Returns handle which can be used to delete the line. Use clear() to delete all lines")]
        public RenderableObjectHandle draw_vector(IPositionable vector, IPositionable origin, string color = "red", double length = 20, double size = 0.1)
        {
            return new RenderableObjectHandle(client.RemoteCall<int>(Contracts.RenderVector, new RenderVectorMessage() {
                vector = vector.position,
                origin = origin.position,
                color = color,
                length = (float)length,
                size = (float)size
            }), this);
        }

        [Doc("Draws circle on map. Returns handle which can be used to delete the circle")]
        public RenderableObjectHandle draw_circle(IPositionable position, double radius = 5, string color = "red")
        {
            return new RenderableObjectHandle(client.RemoteCall<int>(Contracts.RenderCircle, new RenderCircleMessage() {
                position = position.position,
                radius = (float)radius,
                color = color
            }), this);
        }

        [Doc("Clears all lines drawn on map")]
        public void clear()
        {
            client.RemoteVoidCall(Contracts.RemoveRenderedObject, 0);
        }

        [Doc("Prints collection content")]
        public void print_list(IEnumerable collection)
        {
            client.SendMessage(PythonHelp.PrintList(collection), "c_output_message");
        }

        public delegate void __HelpDeleg(object obj = null);

        [Doc("Prints help for given object")]
        public void help(object obj = null)
        {
            obj = obj ?? this;
            string text = PythonHelp.GetHelp(obj);
            client.SendMessage(text, "c_output_message");
        }

        [Doc("Dumps all available documentation in the output")]
        public void help_all()
        {
            string text = PythonHelp.DumpDoc();
            client.SendMessage(text, "c_output_message");
        }

        [Doc("Prints all variables available in the global scope")]
        public void list_globals()
        {
            string vars = string.Join(", ",_scope.GetVariableNames());
            client.SendMessage("Variables in global scope: " + vars + "\n", "c_output_message");
        }

        public override string ToString()
        {
            return PythonHelp.GetHelp(this);
        }
    }
}
