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

        private Dictionary<long, Func<object, string, object>> callbackDict = new Dictionary<long, Func<object, string, object>>();

        private long counter = 1;
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
                        GetMessage(out long _1, out object _2);
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

        private void GetMessage(out long requestId, out object result)
        {
            requestId = 0;
            result = null;

            MessageHeader msg = AwaitMessage();
#if DEBUG
            Console.WriteLine("In: " + msg.messageType);
#endif
            if (msg.messageType == "s_script_run") {
                engine.RunScript(msg.payload);
                return;
            }

            if (msg.messageType == "s_script_abort") {
                Console.WriteLine("Abort script");
                throw new AbortScriptException();
            }

            if (msg.messageType == "s_exception") {
                string text = (string)msg.payload;
                Console.WriteLine("Exception: " + text);
                throw new Exception(text);
            }

            // todo handle exception !!
            if(msg.requestId != 0) {
                var callback = callbackDict[msg.requestId];
                callbackDict.Remove(msg.requestId);
                requestId = msg.requestId;
                result = callback.Invoke(msg.payload, null);
            }

            throw new Exception("Received unknown message: '" + msg.messageType + "'");
        }

        public long RemoteCall(Contract contract, object param, Func<object, string, object> callback)
        {
            // todo synchronous by default - wait for the answer
            long requestId = counter;
            counter++;
            callbackDict.Add(requestId, callback);
            SendMessage(param, "c_callfunc_" + contract.FuncName);
            return requestId;
        }

        public T SynchronousCall<T>(Contract contract, object param)
        {
            long handle = RemoteCall(contract, param, (ret, error) => {
                return ret;
            });
            return (T)WaitOnHandle(handle);
        }

        public object WaitOnHandle(long handle)
        {
            if (!callbackDict.ContainsKey(handle)) {
                throw new Exception("Engine error (report to developers). Cannot wait on invalid handle");
            }
            while (true) {
                GetMessage(out long requestId, out object result);
                if(requestId == handle) {
                    return result;
                }
                // feature - add infinite loop check
            }
        }

        public void WaitForNextMessage()
        {
            GetMessage(out long requestId, out object result);
        }

        public override void SendMessage(object obj, string type, long requestId = 0, bool ignoreReturnValue = false)
        {
            // todo - make inaccessible
            base.SendMessage(obj, type, requestId, ignoreReturnValue);
#if DEBUG
            Console.WriteLine("Out: " + type);
#endif
        }
    }

    public class AbortScriptException : Exception
    {

    }
}
