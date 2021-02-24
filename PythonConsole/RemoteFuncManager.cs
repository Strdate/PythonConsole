using SkylinesPythonShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PythonConsole
{
    public class RemoteFuncManager
    {
        private TcpClient client;

        private delegate object CallMethod(object msg);
        private Dictionary<string,TargetInfo> funcDict = new Dictionary<string, TargetInfo>();

        public RemoteFuncManager(TcpClient client)
        {
            this.client = client;
            foreach (var field in typeof(Contracts).GetFields(BindingFlags.Static | BindingFlags.Public)) {
                Contract contract = (Contract)field.GetValue(null);
                CallMethod del = (CallMethod)Delegate.CreateDelegate(typeof(CallMethod), typeof(GameAPI), contract.FuncName);
                funcDict.Add("c_callfunc_" + contract.FuncName, new TargetInfo(del, contract));
            }
        }

        public void HandleAPICall(object msg, string type)
        {
            object retVal = null;
            TargetInfo info = funcDict[type];
            try
            {
                retVal = info.method(msg);
            }
            catch(Exception ex)
            {
                client.SendMessage(ex.Message + " (source: " + info.contract.FuncName + ")", "s_exception");
            }
            if(info.contract.RetType != null) {
                client.SendMessage(retVal, "s_ret_" + info.contract.RetType);
            }
        }

        private struct TargetInfo
        {
            internal TargetInfo(CallMethod method, Contract contract)
            {
                this.method = method;
                this.contract = contract;
            }

            internal CallMethod method;
            internal Contract contract;
        }
    }
}
