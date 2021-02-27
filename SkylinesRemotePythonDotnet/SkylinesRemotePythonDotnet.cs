using SkylinesRemotePython;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkylinesRemotePythonDotnet
{
    public class SkylinesRemotePythonDotnet
    {
        public static readonly string VERSION = "0.0.0";

        private static ManualResetEvent manualResetEvt = new ManualResetEvent(false);
        static void Main(string[] args)
        {
            Console.WriteLine("This is remote python engine window for Cities:Skylines");
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

            socket.Listen(10);
            Console.WriteLine("Listening on " + port + "...");

            while (true) {
                manualResetEvt.Reset();
                socket.BeginAccept(new AsyncCallback(AcceptCallback), socket);
                manualResetEvt.WaitOne();
            }

            /*Socket accepteddata = socket.Accept(); // 5
            data = new byte[accepteddata.SendBufferSize]; // 6
            int j = accepteddata.Receive(data); // 7
            byte[] adata = new byte[j];         // 7
            for (int i = 0; i < j; i++)         // 7
                adata[i] = data[i];             // 7
            string dat = Encoding.Default.GetString(adata); // 8
            Console.WriteLine(dat);                         // 8*/
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
