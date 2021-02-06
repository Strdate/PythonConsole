using SkylinesPythonShared;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython
{
    public class GameAPI
    {
        private ClientHandler client;
        public GameAPI(ClientHandler client)
        {
            this.client = client;
        }
        public int CreateNode(Vector3 position)
        {
            CreateNodeMessage msg = new CreateNodeMessage()
            {
                Position = position
            };
            client.SendMessage(msg, "c_callfunc_CreateNode");

            MessageHeader retMsg = client.AwaitMessage();

            if(retMsg.messageType != "s_ret_integer")
            {
                throw new Exception("Invalid return message");
            }

            return (int)retMsg.payload;
        }
    }
}
