using SkylinesPythonShared;
using SkylinesPythonShared.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkylinesRemotePython
{
    internal class AsyncCallbackHandler
    {
        private ClientHandler client;
        private Dictionary<long, Action<object>> callbackDict = new Dictionary<long, Action<object>>();
        long counter = 0;

        [ThreadStatic]
        public static AsyncCallbackHandler Instance;

        public AsyncCallbackHandler(ClientHandler client)
        {
            this.client = client;
            Instance = this;
        }

        public void HandleAsyncCallback(MessageHeader msg)
        {
            var callbackMsg = msg.payload as AsyncCallbackMessage;
            var callback = callbackDict[callbackMsg.callbackObjectKey];
            callbackDict.Remove(callbackMsg.callbackObjectKey);
            callback.Invoke(callbackMsg.payload);
        }

        public long Call(Action<object> callback, Contract contract, object param)
        {
            if(!contract.IsBackgroundAsync) {
                throw new Exception("Engine error (report to developers). Function cannot be executed as the contract is not background async");
            }
            callbackDict.Add(counter, callback);
            var msg = new AsyncCallbackMessage();
            msg.callbackObjectKey = counter;
            msg.payload = param;
            counter++;
            client.RemoteCall<object>(contract, msg);
            return msg.callbackObjectKey;
        }

        public bool WaitOnHandle(long handle)
        {
            if(!callbackDict.ContainsKey(handle)) {
                return false;
            }
            var newMsg = (AsyncCallbackMessage)client.GetMessage(waitingForBackgroundCallback: true).payload;
            if (newMsg.callbackObjectKey != handle) {
                throw new Exception("Engine error (report to developers). Received different callback than expected");
            }
            return true;
        }
    }
}
