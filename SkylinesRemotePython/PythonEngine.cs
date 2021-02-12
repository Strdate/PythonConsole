using IronPython.Hosting;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using SkylinesPythonShared;
using SkylinesPythonShared.API;
using SkylinesRemotePython.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SkylinesRemotePython
{
    public class PythonEngine
    {
        private ClientHandler client;

        private ScriptEngine _engine;
        private ScriptScope _scope;

        public PythonEngine(ClientHandler client)
        {
            this.client = client;
            _engine = Python.CreateEngine();
            _scope = _engine.CreateScope();

            _scope.SetVariable("Vector", DynamicHelpers.GetPythonTypeFromType(typeof(Vector)));
            _scope.SetVariable("NetOptions", DynamicHelpers.GetPythonTypeFromType(typeof(NetOptions)));
            _scope.SetVariable("vector_xz", new Func<double,double, Vector>(Vector.vector_xz));
            _scope.SetVariable("game", new GameAPI(client));

            var outputStream = new MemoryStream();
            var outputStreamWriter = new TcpStreamWriter(outputStream, client);
            _engine.Runtime.IO.SetOutput(outputStream, outputStreamWriter);
        }

        public void RunScript(object obj)
        {
            RunScriptMessage msg = (RunScriptMessage)obj;

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
                    client.SendMessage(ex.Message, "c_exception");
                }
            }
            catch(Exception ex)
            {
                client.SendMessage(ex.Message, "c_failed_to_compile");
            }
        }
    }
}
