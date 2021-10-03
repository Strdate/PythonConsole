using ColossalFramework.IO;
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
    public class TcpClient : TcpConversation
    {
        public static Process process;
        public static TcpClient CreateClient()
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s.Connect(IPAddress.Parse("127.0.0.1"), GetPortNumber());
            return new TcpClient(s);
        }
        protected TcpClient(Socket s) : base(s)
        {
            
        }

        public void SendMsg(object obj, string type, long requestId = 0)
        {
            base.SendMessage(obj, type, requestId);
        }

        public static void StartUpServer()
        {
            string archivePath = Path.Combine(ModPath.Instsance.AssemblyPath,"SkylinesRemotePython.zip");
            bool useShellExecute = ModInfo.ShowRemoteConsole.value;
            bool createNoWindow = !ModInfo.ShowRemoteConsole.value;

            using (var unzip = new Unzip(archivePath))
            {
                unzip.ExtractToDirectory(ModInfo.RemotePythonFolder);
            }

            if(ModInfo.DoNotLaunchRemoteConsole.value) {
                return;
            }

            process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(ModInfo.RemotePythonFolder, "SkylinesRemotePythonDotnet.exe"),
                    Arguments = "-port " + GetPortNumber(),
                    UseShellExecute = useShellExecute,
                    CreateNoWindow = createNoWindow,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false
                }

            };
            process.Start();
        }

        private static int GetPortNumber()
        {
            int port = UnityPythonObject.Instance.Config.tcpPort;
            if (port < 1 || port > 65535) {
                port = ModInfo.DEF_PORT;
            }
            return port;
        }

        public void CloseSocket()
        {
            _client.Close();
        }

        public MessageHeader GetMessageSync()
        {
            MessageHeader msg = AwaitMessage();
            return msg;
        }
    }
}
