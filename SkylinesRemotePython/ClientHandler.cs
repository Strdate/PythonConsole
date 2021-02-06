using SkylinesPythonShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace SkylinesRemotePython
{
    public class ClientHandler
    {
        private Socket listener;
        private Socket handler;

        private PythonEngine engine;

        public static void Accept(Socket listener, Socket handler)
        {
            ClientHandler client = new ClientHandler(listener, handler);
            client.HandleClient();
        }

        private ClientHandler(Socket listener, Socket handler)
        {
            this.listener = listener;
            this.handler = handler;
        }

        private void HandleClient()
        {
            engine = new PythonEngine(this);
            while (true)
            {
                HandleGeneralMessage( AwaitMessage() );
            }
        }

        public MessageHeader AwaitMessage()
        {
            byte[] data = new byte[handler.SendBufferSize];
            int j = handler.Receive(data);

            byte[] adata = new byte[j];
            for (int i = 0; i < j; i++)
                adata[i] = data[i];

            MessageHeader msg = (MessageHeader)Deserialize(adata);

            Console.WriteLine("In: " + msg.messageType);

            if (msg.messageType == "s_exception")
            {
                string text = (string)msg.payload;
                Console.WriteLine("Exception: " + text);
                throw new Exception(text);
            }

            return msg;
        }

        private void HandleGeneralMessage(MessageHeader msg)
        {
            switch(msg.messageType)
            {
                case "s_script_run": engine.RunScript(msg.payload); break;
            }
        }

        public void SendMessage(object obj, string type)
        {
            Console.WriteLine("Sending message: " + type);
            MessageHeader msg = new MessageHeader();
            msg.payload = obj;
            msg.messageType = type;
            handler.Send(Serialize(msg));
        }

        public static object Deserialize(byte[] data)
        {
            using (var memoryStream = new MemoryStream(data))
                return (new BinaryFormatter()).Deserialize(memoryStream);
        }

        public static byte[] Serialize(MessageHeader obj)
        {
            using (var memoryStream = new MemoryStream())
            {
                (new BinaryFormatter()).Serialize(memoryStream, obj);
                return memoryStream.ToArray();
            }
        }
    }
}
