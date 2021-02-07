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

        public int CreateProp(Vector3 position, string type, double angle = 0)
        {
            var msg = new CreatePropMessage()
            {
                Position = position,
                Type = type,
                Angle = angle
            };
            client.SendMessage(msg, "c_callfunc_CreateProp");

            MessageHeader retMsg = client.GetMessage();

            if (retMsg.messageType != "s_ret_integer")
            {
                throw new Exception("Invalid return message");
            }

            return (int)retMsg.payload;
        }

        public int CreateNode(Vector3 position)
        {
            CreateNodeMessage msg = new CreateNodeMessage()
            {
                Position = position
            };
            client.SendMessage(msg, "c_callfunc_CreateNode");

            MessageHeader retMsg = client.GetMessage();

            if(retMsg.messageType != "s_ret_integer")
            {
                throw new Exception("Invalid return message");
            }

            return (int)retMsg.payload;
        }
    }
}
