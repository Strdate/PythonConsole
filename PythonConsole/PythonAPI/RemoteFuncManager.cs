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
                MethodInfo method = typeof(GameAPI).GetMethod(contract.FuncName, BindingFlags.Public | BindingFlags.Static);
                CallMethod del = (CallMethod)Delegate.CreateDelegate(typeof(CallMethod), method);
                funcDict.Add("c_callfunc_" + contract.FuncName, new TargetInfo(del, contract));
            }
        }

        public void HandleAPICall(object msg, string type, long requestId)
        {
            object retVal = null;
            TargetInfo info = funcDict[type];
            try
            {
                retVal = info.method(msg);
                if (info.contract.RetType != null) {
                    client.SendMsg(retVal, "s_ret_" + info.contract.RetType, requestId);
                }
            }
            catch(Exception ex)
            {
                client.SendMsg(ex.Message + " (source: " + info.contract.FuncName + ")", "s_exception", requestId);
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
