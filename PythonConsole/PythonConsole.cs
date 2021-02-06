using SkylinesPythonShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PythonConsole
{
    public class PythonConsole
    {
        private static PythonConsole _instance;
        public static PythonConsole Instsance
        {
            get
            {
                if (_instance == null)
                    _instance = new PythonConsole();
                return _instance;
            }
        }

        private TcpClient _client;
        private string _scheduledScript;
        private bool _isRunning;

        public PythonConsole()
        {
            _client = new TcpClient();
        }

        public void ScheduleExecution(string script)
        {
            _scheduledScript = script;
        }

        public void SimulationStep()
        {
            if(_scheduledScript != null)
            {
                RunScriptMessage msg = new RunScriptMessage();
                msg.script = _scheduledScript;
                _client.SendMessage(msg, "s_script_run");
                _isRunning = true;
                while(_isRunning)
                {
                    MessageHeader header = _client.AwaitMessage();
                    switch(header.messageType)
                    {
                        case "c_output_message": UIWindow.Instance.Log((string)header.payload); break;
                        case "c_failed_to_compile":
                            UIWindow.Instance.Log("Failed to compile:" + (string)header.payload + "\n");
                            _isRunning = false; break;
                        case "c_script_end":
                            UIWindow.Instance.Log("== EXECUTION COMPLETE ==\n");
                            _isRunning = false; break;
                        default:
                            if (header.messageType.StartsWith("c_callfunc_"))
                            {
                                HandleCall.HandleAPICall(header.payload, header.messageType);
                            }
                            break;
                    }
                }
            }
        }
    }
}
