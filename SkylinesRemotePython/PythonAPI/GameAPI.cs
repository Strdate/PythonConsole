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

        public NetNode create_node(Vector position, string prefab)
        {
            CreateNodeMessage msg = new CreateNodeMessage()
            {
                Position = position,
                Type = prefab
            };

            return new NetNode(client.RemoteCall<NetNodeMessage>(Contracts.CreateNode, msg), this);
        }

        public Segment create_segment(NetNode startNode, NetNode endNode, string prefab)
        {
            return create_segment(startNode, endNode, prefab, null);
        }

        public Segment create_segment(NetNode startNode, NetNode endNode, string prefab, Vector middle_pos)
        {
            CreateSegmentMessage msg = new CreateSegmentMessage() {
                start_node_id = startNode.id,
                end_node_id = endNode.id,
                prefab_name = prefab,
                middle_pos = middle_pos
            };
            return new Segment(client.RemoteCall<NetSegmentMessage>(Contracts.CreateSegment, msg), this);
        }

        public Segment create_segment(NetNode startNode, NetNode endNode, string prefab, Vector start_dir, Vector end_dir)
        {
            CreateSegmentMessage msg = new CreateSegmentMessage() {
                start_node_id = startNode.id,
                end_node_id = endNode.id,
                prefab_name = prefab,
                start_dir = start_dir,
                end_dir = end_dir
            };
            return new Segment(client.RemoteCall<NetSegmentMessage>(Contracts.CreateSegment, msg), this);
        }

        public bool is_prefab(string name)
        {
            return client.RemoteCall<bool>(Contracts.ExistsPrefab, name);
        }

        public override string ToString()
        {
            return "Provides API to manipulate in-game objects, such as buildings or roads";
        }
    }
}
