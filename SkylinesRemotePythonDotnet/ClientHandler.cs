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

        private Dictionary<long, CallbackHandle> callbackDict = new Dictionary<long, CallbackHandle>();

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
                        GetMessage(out long _1, out object _2, out string _3);
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

        private void GetMessage(out long requestId, out object result, out string error)
        {
            requestId = 0;
            result = null;
            error = null;

            MessageHeader msg = AwaitMessage();
            if (msg.messageType == "s_script_run") {
                engine.RunScript(msg.payload);
                return;
            }

            if (msg.messageType == "s_script_abort") {
                Console.WriteLine("Abort script");
                throw new AbortScriptException();
            }
            if (msg.requestId != 0) {
                var callback = callbackDict[msg.requestId];
                callbackDict.Remove(msg.requestId);
                requestId = msg.requestId;
                callback.Resolved = true;
                if(msg.messageType != "s_exception") {
                    result = callback.Callback.Invoke(msg.payload, null);
                } else {
                    error = (string)msg.payload;
                    callback.Callback.Invoke(null, error);
                    if(!AsynchronousMode) {
                        throw new Exception(error);
                    } else {
                        Print("Exception: " + error + "\n");
                    }
                }
                return;
            }

            if (msg.messageType == "s_exception") {
                string text = (string)msg.payload;
                Console.WriteLine("Exception: " + text);
                throw new Exception(text);
            }

            throw new Exception("Received unknown message: '" + msg.messageType + "'");
        }

        public CallbackHandle RemoteCall(Contract contract, object param, Func<object, string, object> callback)
        {
            var handle = RemoteCallInternal(contract, param, callback);
            if (!AsynchronousMode && !contract.IsAsyncByDefault && contract.RetType != null) {
                WaitOnHandle(handle);
            }
            return handle;
        }

        private CallbackHandle RemoteCallInternal(Contract contract, object param, Func<object, string, object> callback)
        {
            long requestId = counter;
            counter++;
            var handle = new CallbackHandle(requestId, callback);
            callbackDict.Add(requestId, handle);
            SendMessage(param, "c_callfunc_" + contract.FuncName, requestId);
            return handle;
        }

        public T SynchronousCall<T>(Contract contract, object param)
        {
            var handle = RemoteCallInternal(contract, param, (ret, error) => {
                return ret;
            });
            return contract.RetType != null ?(T)WaitOnHandle(handle) : default(T);
        }

        public object WaitOnHandle(CallbackHandle handle)
        {
            if(handle.Resolved) {
                return null;
            }
            if (!callbackDict.ContainsKey(handle.HandleId)) {
                throw new Exception("Engine error (report to developers). Cannot wait on invalid handle");
            }
#if DEBUG
            var callerMethod = (new System.Diagnostics.StackTrace()).GetFrame(1).GetMethod();
            var callerName = (new System.Diagnostics.StackTrace()).GetFrame(1).GetMethod().Name;
            if(callerName == "SynchronousCall") {
                callerMethod = (new System.Diagnostics.StackTrace()).GetFrame(2).GetMethod();
            }
            Console.WriteLine($"Waiting on handle {handle.HandleId}. Caller: {callerMethod.DeclaringType.Name}.{callerMethod.Name}");
#endif
            while (true) {
                GetMessage(out long requestId, out object result, out string error);
                if(requestId == handle.HandleId) {
                    return result;
                }
                // feature - add infinite loop check
            }
        }

        public void WaitForNextMessage()
        {
            GetMessage(out long requestId, out object result, out string error);
        }

        public void Print(string text)
        {
            SendMessage(text, "c_output_message", 0);
        }

        public void SendMsg(object obj, string type)
        {
            SendMessage(obj, type);
        }

        protected override void SendMessage(object obj, string type, long requestId = 0, bool ignoreReturnValue = false)
        {
            base.SendMessage(obj, type, requestId, ignoreReturnValue);
        }
    }

    public class CallbackHandle
    {
        public CallbackHandle(long handleId, Func<object, string, object> callback)
        {
            HandleId = handleId;
            Callback = callback;
        }

        public long HandleId { get; private set; }
        public Func<object, string, object> Callback { get; private set; }
        public bool Resolved { get; set; }
    }

    public class AbortScriptException : Exception
    {

    }
}
