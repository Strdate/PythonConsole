using SkylinesPythonShared;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API
{
    public class GameAPI
    {
        private ClientHandler client;
        public GameAPI(ClientHandler client)
        {
            this.client = client;
        }

        public Prop get_prop_from_id(int id)
        {
            client.SendMessage(id, "c_callfunc_GetPropFromId");
            MessageHeader retMsg = client.GetMessage();
            AssertMessage(retMsg, "s_ret_Prop");

            return new Prop(retMsg.payload);
        }

        public NetNode get_node_from_id(int id)
        {
            client.SendMessage(id, "c_callfunc_GetNodeFromId");

            MessageHeader retMsg = client.GetMessage();
            AssertMessage(retMsg, "s_ret_NetNode");

            return new NetNode(retMsg.payload);
        }

        public Prop create_prop(Vector position, string type, double angle = 0)
        {
            var msg = new CreatePropMessage()
            {
                Position = position,
                Type = type,
                Angle = angle
            };
            client.SendMessage(msg, "c_callfunc_CreateProp");

            MessageHeader retMsg = client.GetMessage();
            AssertMessage(retMsg, "s_ret_Prop");

            return new Prop(retMsg.payload);
        }

        public NetNode create_node(Vector position, string type)
        {
            CreateNodeMessage msg = new CreateNodeMessage()
            {
                Position = position,
                Type = type
            };
            client.SendMessage(msg, "c_callfunc_CreateNode");

            MessageHeader retMsg = client.GetMessage();
            AssertMessage(retMsg, "s_ret_NetNode");

            return new NetNode(retMsg.payload);
        }

        public bool exists_prefab(string name)
        {
            client.SendMessage(name, "c_callfunc_ExistsPrefab");
            MessageHeader retMsg = client.GetMessage();
            AssertMessage(retMsg, "s_ret_bool");

            return (bool)retMsg.payload;
        }

        public override string ToString()
        {
            return "Provides API to manipulate in-game objects, such as buildings or roads";
        }

        public static void AssertMessage(MessageHeader msg, string expected)
        {
            if(msg.messageType != expected)
            {
                throw new Exception("Invalid return message: expected '" + expected + "' but received '" + msg.messageType + "'");
            }
        }
    }
}
