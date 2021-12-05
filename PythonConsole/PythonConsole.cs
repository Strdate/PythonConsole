﻿using ColossalFramework.Threading;
using PythonConsole.MoveIt;
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

        public RenderableObjManager RenderManager { get; private set; } = new RenderableObjManager();

        private int _startUpTrials;
        private TcpClient _client;
        private Queue _simulationQueue;
        private RemoteFuncManager _remoteFuncManager;

        private volatile ConsoleState _state = ConsoleState.Initializing;

        public ConsoleState State {
            get => _state;
            private set => _state = value;
        }
        public bool ExecuteSynchronously { get; private set; }
        private Thread _thread;
        private Stopwatch _stopWatch;

        public PythonConsole(bool executeSynchronously)
        {
            Queue q = new Queue();
            ExecuteSynchronously = executeSynchronously;
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
            if(ModInfo.DoNotLaunchRemoteConsole.value) {
                PrintAsync("Warning: Option 'Do not launch remote python console server' is enabled in settings. Turn it off if you don't know what you are doing.\n");
            }
            TcpClient.StartUpServer();
            while(_startUpTrials < 10) {
                try {
                    if (State == ConsoleState.Initializing) {
                        _client = TcpClient.CreateClient();
                        try {
                            _remoteFuncManager = new RemoteFuncManager(_client);
                        } catch(Exception ex) {
                            PrintAsync("Critical error in the python console mod\n" + ex);
                            break;
                        }
                        
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
                    //UnityEngine.Debug.Log("In: " + header.messageType);
                    if(header.messageType == "c_exception") {
                        State = ConsoleState.Ready;
                        if(State != ConsoleState.ScriptAborting) {
                            PrintErrorAsync((string)header.payload);
                        }
                        continue;
                    }

                    if(header.messageType == "c_ready") {
                        State = ConsoleState.Ready;
                        continue;
                    }

                    if(State == ConsoleState.ScriptAborting) {
                        _client.SendMsg(null, "s_script_abort");
                        continue;
                    }

                    switch (header.messageType) {
                        case "c_output_message":
                            PrintAsync((string)header.payload);
                            break;
                        case "c_failed_to_compile":
                            State = ConsoleState.Ready;
                            PrintErrorAsync("Failed to compile: " + (string)header.payload);
                            break;
                        default:
                            if (header.messageType.StartsWith("c_callfunc_") || header.messageType == "c_script_end") {
                                _simulationQueue.Enqueue(header);
                            }
                            break;
                    }
                }
            } catch(Exception ex) {
                try { PrintAsync("Python engine crashed. Message: " + ex.Message + "\n"); } catch { }
            }
            State = ConsoleState.Dead;
            try { _client.CloseSocket(); } catch { }
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
                    RunScriptMessage msg = new RunScriptMessage() {
                        script = script,
                        clipboard = GetClipboardObjects(),
                        searchPaths = GetSearchPaths()
                    };
                    _stopWatch = new Stopwatch();
                    _stopWatch.Start();
                    _client.SendMsg(msg, "s_script_run");
                    State = ConsoleState.ScriptRunning;
                }
            }
            catch(Exception e) {
                State = ConsoleState.Dead;
                UnityPythonObject.Instance.PrintError(e.ToString());
            }
        }

        public void AbortScript()
        {
            State = ConsoleState.ScriptAborting;
            _client.SendMsg(null, "s_script_abort");
        }

        private List<object> GetClipboardObjects()
        {
            return SelectionTool.Instance.Clipboard.Where((obj) => obj.Exists).Select((obj) => obj.ToMessage()).ToList();
        }

        private List<string> GetSearchPaths()
        {
            return new List<string>{
                UnityPythonObject.Instance.scriptEditor.projectWorkspacePath,
                Path.Combine(UnityPythonObject.Instance.scriptEditor.projectWorkspacePath, "imports"),
                Path.Combine(UnityPythonObject.Instance.scriptEditor.projectWorkspacePath, "examples"),
                Path.Combine(ModInfo.RemotePythonFolder, "imports"),
                Path.Combine(ModInfo.RemotePythonFolder, "pypy")
            };
        }

        public void SimulationStep()
        {
            do {
                if (State == ConsoleState.ScriptRunning) {
                    while (_simulationQueue.Count > 0) {
                        MessageHeader header = (MessageHeader)_simulationQueue.Dequeue();
                        switch(header.messageType) {
                            case "c_script_end":
                                _stopWatch.Stop();
                                State = ConsoleState.Ready;
                                PrintAsync("Execution took " + _stopWatch.ElapsedMilliseconds + " ms\n");
                                break;
                            default:
                                _remoteFuncManager.HandleAPICall(header.payload, header.messageType, long.Parse(header.requestId));
                                break;
                        }
                    }
                    if(ExecuteSynchronously && State == ConsoleState.ScriptRunning) {
                        Thread.Sleep(1);
                    }
                    MoveItTool.instance.SimulationStep();
                }
            } while (State == ConsoleState.ScriptRunning && ExecuteSynchronously);
        }

        public static void KillInstance()
        {
            if (_instance != null) {
                _instance.State = ConsoleState.Dead;
                try {
                    TcpClient.process.Kill();
                }
                catch { }
            }
        }

        public static void CreateInstance()
        {
            KillInstance();
            _instance = new PythonConsole(ModInfo.SyncExecution.value);
        }
    }

    public enum ConsoleState
    {
        Initializing,
        Ready,
        ScriptRunning,
        ScriptAborting,
        Dead
    }
}
