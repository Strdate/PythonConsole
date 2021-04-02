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

        public bool AsynchronousMode { get; set; }

        [ThreadStatic]
        public static ClientHandler Instance;
        public static void Accept(Socket listener, Socket handler)
        {
            ClientHandler client = new ClientHandler(handler);
            client.HandleClient();
        }

        protected ClientHandler(Socket client) : base(client)
        {
            Instance = this;
        }

        private void HandleClient()
        {
            try {
                engine = new PythonEngine(this);
                while (true) {
                    try {
                        HandleGeneralMessage(GetMessage());
                    }
                    catch (AbortScriptException) {
                        SendMessage(null, "c_ready");
                    }

                }
            } catch(Exception ex) {
                Console.WriteLine(ex);
                SkylinesRemotePythonDotnet.exitEvent.Set();
            }
        }

        public MessageHeader GetMessage(string assertedType = null)
        {
            MessageHeader msg = AwaitMessage();
#if DEBUG
            Console.WriteLine("In: " + msg.messageType);
#endif

            if (msg.messageType == "s_script_abort") {
                Console.WriteLine("Abort script");
                throw new AbortScriptException();
            }

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
            bool isAsync = AsynchronousMode && contract.CanRunAsync;
            SendMessage(parameters, "c_callfunc_" + contract.FuncName, isAsync);
            if(isAsync || contract.RetType == null) {
                return default(T);
            }
            MessageHeader retMsg = GetMessage("s_ret_" + contract.RetType);
            return (T)retMsg.payload;
        }

        public override void SendMessage(object obj, string type, bool ignoreReturnValue = false)
        {
            base.SendMessage(obj, type, ignoreReturnValue);
#if DEBUG
            Console.WriteLine("Out: " + type);
#endif
        }

        private void HandleGeneralMessage(MessageHeader msg)
        {
            switch(msg.messageType)
            {
                case "s_script_run": engine.RunScript(msg.payload); break;
            }
        }
    }

    public class AbortScriptException : Exception
    {

    }
}
