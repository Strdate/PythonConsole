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

        public Prop get_prop_from_id(int id)
        {
            return new Prop(client.RemoteCall<PropMessage>(Contracts.GetPropFromId, id), this);
        }

        public NetNode get_node_from_id(int id)
        {
            return new NetNode(client.RemoteCall<NetNodeMessage>(Contracts.GetNodeFromId, id), this);
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

        public NetNode create_node(Vector position, string type)
        {
            CreateNodeMessage msg = new CreateNodeMessage()
            {
                Position = position,
                Type = type
            };

            return new NetNode(client.RemoteCall<NetNodeMessage>(Contracts.CreateNode, msg), this);
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
