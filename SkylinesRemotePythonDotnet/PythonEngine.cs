using IronPython.Hosting;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using SkylinesPythonShared;
using SkylinesPythonShared.API;
using SkylinesRemotePython.API;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SkylinesRemotePython
{
    public class PythonEngine
    {
        private ClientHandler client;

        private ScriptEngine _engine;
        private ScriptScope _scope;
        private GameAPI _gameAPI;
        private CachedObjects _cachedObjects;

        public PythonEngine(ClientHandler client)
        {
            this.client = client;
            _engine = Python.CreateEngine();
            _scope = _engine.CreateScope();
            _gameAPI = new GameAPI(client, _scope);
            _cachedObjects = new CachedObjects(client);

            PrepareStaticLocals();
            
            var outputStream = new MemoryStream();
            var outputStreamWriter = new TcpStreamWriter(outputStream, client);
            _engine.Runtime.IO.SetOutput(outputStream, outputStreamWriter);
        }

        public void RunScript(object obj)
        {
            RunScriptMessage msg = (RunScriptMessage)obj;
            PrepareDynamicLocals(msg.clipboard);
            _engine.SetSearchPaths(msg.searchPaths.ToList());

            try
            {
                var source = _engine.CreateScriptSourceFromString(msg.script, SourceCodeKind.Statements);
                var compiled = source.Compile();
                try
                {
                    compiled.Execute(_scope);
                    client.SendMessage(null, "c_script_end");
                }
                catch(Exception ex)
                {
                    string details = "";
                    try {
                        var frame = PythonOps.GetDynamicStackFrames(ex).First();
                        string fileName = frame.GetFileName();
                        details = " (" + (fileName == "<string>" ? "script" : fileName) + ": line " + frame.GetFileLineNumber() + ")";
                    } catch { }
                    
                    client.SendMessage(ex.Message + details, "c_exception");
                }
            }
            catch(Exception ex)
            {
                client.SendMessage(ex.Message, "c_failed_to_compile");
            }
        }

        private void PrepareStaticLocals()
        {
            _scope.SetVariable("Vector", DynamicHelpers.GetPythonTypeFromType(typeof(Vector)));
            _scope.SetVariable("Point", DynamicHelpers.GetPythonTypeFromType(typeof(Point)));
            _scope.SetVariable("NetOptions", DynamicHelpers.GetPythonTypeFromType(typeof(NetOptions)));
            _scope.SetVariable("vector_xz", new Func<double, double, Vector>(Vector.vector_xz));
            _scope.SetVariable("print_list", new Action<IEnumerable>(_gameAPI.print_list));
            MethodInfo method = typeof(GameAPI).GetMethod("help", BindingFlags.Public | BindingFlags.Instance);
            _scope.SetVariable("help", Delegate.CreateDelegate(typeof(GameAPI.__HelpDeleg), _gameAPI, method));
            _scope.SetVariable("help_all", new Action(_gameAPI.help_all));
            _scope.SetVariable("list_globals", new Action(_gameAPI.list_globals));
            _scope.SetVariable("g", _gameAPI);
            _scope.SetVariable("game", _gameAPI);
        }

        private void PrepareDynamicLocals(InstanceMessage[] arr)
        {
            List<object> res = new List<object>();
            object obj;
            for(int i = 0; i < arr.Length; i++) {
                if(arr[i] is Vector) {
                    obj = new Point((Vector)arr[i]);
                } else if(arr[i] is NetNodeMessage) {
                    obj = new Node((NetNodeMessage)arr[i], _gameAPI);
                } else if (arr[i] is NetSegmentMessage) {
                    obj = new Segment((NetSegmentMessage)arr[i], _gameAPI);
                } else if (arr[i] is BuildingMessage) {
                    obj = new Building((BuildingMessage)arr[i], _gameAPI);
                } else if (arr[i] is PropMessage) {
                    obj = new Prop((PropMessage)arr[i], _gameAPI);
                } else /*if (arr[i] is TreeMessage*/ {
                    obj = new Tree((TreeMessage)arr[i], _gameAPI);
                }
                res.Add(obj);
            }

            _scope.SetVariable("cba", res);
            if (res.Count > 0) {
                _scope.SetVariable("cb", res[0]);
            }
        }
    }
}
