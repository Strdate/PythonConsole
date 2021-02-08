using SkylinesPythonShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace SkylinesRemotePython
{
    public class ClientHandler : TcpConversation
    {
        private PythonEngine engine;

        public static void Accept(Socket listener, Socket handler)
        {
            ClientHandler client = new ClientHandler(handler);
            client.HandleClient();
        }

        protected ClientHandler(Socket client) : base(client)
        {
            
        }

        private void HandleClient()
        {
            engine = new PythonEngine(this);
            while (true)
            {
                HandleGeneralMessage( GetMessage() );
            }
        }

        public MessageHeader GetMessage(string assertedType = null)
        {
            MessageHeader msg = AwaitMessage();
            Console.WriteLine("In: " + msg.messageType);

            if (msg.messageType == "s_exception")
            {
                string text = (string)msg.payload;
                Console.WriteLine("Exception: " + text);
                throw new Exception(text);
            }

            if (assertedType != null && assertedType != msg.messageType) {
                throw new Exception("Invalid return message: expected '" + assertedType + "' but received '" + msg.messageType + "'");
            }

            return msg;
        }

        public T RemoteCall<T>(Contract contract, object parameters)
        {
            SendMessage(parameters, "c_callfunc_" + contract.FuncName);
            MessageHeader retMsg = GetMessage("s_ret_" + contract.RetType);
            return (T)retMsg.payload;
        }

        public override void SendMessage(object obj, string type)
        {
            base.SendMessage(obj, type);
            Console.WriteLine("Out: " + type);
        }

        private void HandleGeneralMessage(MessageHeader msg)
        {
            switch(msg.messageType)
            {
                case "s_script_run": engine.RunScript(msg.payload); break;
            }
        }
    }
}
