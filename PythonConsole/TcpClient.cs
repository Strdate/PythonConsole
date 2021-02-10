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
        public static TcpClient CreateClient()
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s.Connect(IPAddress.Parse("127.0.0.1"), 6672);
            return new TcpClient(s);
        }
        protected TcpClient(Socket s) : base(s)
        {
            
        }

        public static void StartUpServer()
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

        public MessageHeader GetMessageSync()
        {
            MessageHeader msg = AwaitMessage();
            return msg;
        }
    }
}
