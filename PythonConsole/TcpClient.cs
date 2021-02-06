using SkylinesPythonShared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace PythonConsole
{
    public class TcpClient
    {
        private Socket socket;
        public TcpClient()
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                s.Connect(IPAddress.Parse("127.0.0.1"), 6672);
            }
            catch
            {
                StartUpServer();
                throw;
            }
            s.ReceiveTimeout = 5000;
            socket = s;
        }

        private void StartUpServer()
        {
            string archivePath = Path.Combine(ModPath.Instsance.AssemblyPath,"SkylinesRemotePython.zip");
            string destPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SkylinesRemotePython");

            using (var unzip = new Unzip(archivePath))
            {
                unzip.ExtractToDirectory(destPath);
            }
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "\"" + Path.Combine(destPath, "SkylinesRemotePython.dll") + "\"",
                    UseShellExecute = true,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    CreateNoWindow = false
                }

            };
            process.Start();
        }

        public MessageHeader AwaitMessage()
        {
            byte[] data = new byte[socket.SendBufferSize];
            int j = socket.Receive(data);

            byte[] adata = new byte[j];
            for (int i = 0; i < j; i++)
                adata[i] = data[i];

            MessageHeader msg = (MessageHeader)Deserialize(adata);

            UnityEngine.Debug.Log("In: " + msg.messageType);

            if (msg.messageType == "s_exception")
            {
                string text = (string)msg.payload;
                throw new Exception(text);
            }

            return msg;
        }

        public void SendMessage(object obj, string type)
        {
            MessageHeader msg = new MessageHeader();
            msg.payload = obj;
            msg.messageType = type;
            socket.Send(Serialize(msg));
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
