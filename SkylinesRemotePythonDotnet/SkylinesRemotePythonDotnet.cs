using SkylinesRemotePython;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkylinesRemotePython
{
    public class SkylinesRemotePythonDotnet
    {
        public static readonly string VERSION = "0.0.0";

        public static ManualResetEvent exitEvent = new ManualResetEvent(false);

        private static ManualResetEvent manualResetEvt = new ManualResetEvent(false);
        static void Main(string[] args)
        {
            try {
                Console.WriteLine("\nThis is remote python engine window for Cities:Skylines\n");
                Console.WriteLine("You can change the default tcp port in Cities_Skylines\\PythonConsoleMod.xml");
                Console.WriteLine("Startup...");
                int port = 0;
                for (var i = 0; i < args.Length; i++) {
                    if (args[i] == "-port" && i + 1 < args.Length) {
                        if (!Int32.TryParse(args[i + 1], out port)) {
                            throw new Exception("Argument '" + args[i + 1] + "' is not a valid port");
                        }
                    }
                }
                port = port == 0 ? 6672 : port;

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));

                socket.Listen(1);
                Console.WriteLine("Listening on " + port + "...");

                //while (true) {
                    manualResetEvt.Reset();
                    socket.BeginAccept(new AsyncCallback(AcceptCallback), socket);
                    manualResetEvt.WaitOne();
                //}
            } catch(Exception ex) {
                Console.WriteLine(ex);
            }

            exitEvent.WaitOne();
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            manualResetEvt.Set();
            Console.WriteLine("Accepted new connection");
            ClientHandler.Accept(listener, handler);
        }
    }
}
