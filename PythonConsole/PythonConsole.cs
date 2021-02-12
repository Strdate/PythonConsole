using ColossalFramework.Threading;
using SkylinesPythonShared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace PythonConsole
{
    public class PythonConsole
    {
        private static PythonConsole _instance;

        public static PythonConsole Instance => _instance;

        private int _startUpTrials;
        private TcpClient _client;
        private Queue _simulationQueue;
        private RemoteFuncManager _remoteFuncManager;

        private volatile ConsoleState _state = ConsoleState.Initializing;
        public ConsoleState State {
            get => _state;
            private set => _state = value;
        }
        private Thread _thread;
        private Stopwatch _stopWatch;

        public PythonConsole()
        {
            Queue q = new Queue();
            _simulationQueue = Queue.Synchronized(q);
            _thread = new Thread(new ThreadStart(RemotePythonThread));
            _thread.Name = "RemotePython";
            _thread.Start();
            if (!_thread.IsAlive) {
                throw new Exception("Failed to start RemotePython thread!");
            }
        }

        private void RemotePythonThread()
        {
            TcpClient.StartUpServer();
            while(_startUpTrials < 10) {
                try {
                    if (State == ConsoleState.Initializing) {
                        _client = TcpClient.CreateClient();
                        _remoteFuncManager = new RemoteFuncManager(_client);
                        State = ConsoleState.Ready;
                        PrintAsync("Python engine ready\n");
                        break;
                    }
                }
                catch {
                    _startUpTrials++;
                    Thread.Sleep(500);
                }
            }

            if(State == ConsoleState.Initializing) {
                State = ConsoleState.Dead;
                try { PrintAsync("Failed to start python engine\n"); } catch { }
                return;
            }

            try {
                while (State != ConsoleState.Dead) {
                    MessageHeader header = _client.GetMessageSync();
                    switch (header.messageType) {
                        case "c_output_message":
                            PrintAsync((string)header.payload);
                            break;
                        case "c_exception":
                            State = ConsoleState.Ready;
                            PrintErrorAsync((string)header.payload);
                            break;
                        case "c_failed_to_compile":
                            State = ConsoleState.Ready;
                            PrintErrorAsync("Failed to compile: " + (string)header.payload);
                            break;
                        case "c_script_end":
                            _stopWatch.Stop();
                            State = ConsoleState.Ready;
                            PrintAsync("Execution took " + _stopWatch.ElapsedMilliseconds + " ms\n");
                            break;
                        default:
                            if (header.messageType.StartsWith("c_callfunc_")) {
                                _simulationQueue.Enqueue(header);
                            }
                            break;
                    }
                }
            } catch(Exception ex) {
                try { PrintAsync("Python engine crashed. Message: " + ex.Message + "\n"); } catch { }
            }
            State = ConsoleState.Dead;
        }

        private void PrintAsync(string message)
        {
            ThreadHelper.dispatcher.Dispatch(() => {
                UnityPythonObject.Instance.Print(message);
            });
        }

        private void PrintErrorAsync(string message)
        {
            ThreadHelper.dispatcher.Dispatch(() => {
                UnityPythonObject.Instance.PrintError(message);
            });
        }

        public void ScheduleExecution(string script)
        {
            try {
                if (State == ConsoleState.Ready) {
                    RunScriptMessage msg = new RunScriptMessage();
                    msg.script = script;
                    _stopWatch = new Stopwatch();
                    _stopWatch.Start();
                    _client.SendMessage(msg, "s_script_run");
                    State = ConsoleState.ScriptRunning;
                }
            }
            catch {
                State = ConsoleState.Dead;
                throw;
            }
        }

        public void OnUpdate()
        {
            if (!(UnityPythonObject.Instance?.scriptEditor?.Visible ?? false)) {
                return;
            }


            
        }

        public void SimulationStep()
        {
            if (State == ConsoleState.ScriptRunning) {
                while(_simulationQueue.Count > 0) {
                    MessageHeader header = (MessageHeader) _simulationQueue.Dequeue();
                    _remoteFuncManager.HandleAPICall(header.payload, header.messageType);
                }
            }
        }

        public static void CreateInstance()
        {
            if(_instance != null) {
                _instance.State = ConsoleState.Dead;
            }
            _instance = new PythonConsole();
        }
    }

    public enum ConsoleState
    {
        Initializing,
        Ready,
        ScriptRunning,
        Dead
    }
}
