using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using SkylinesPythonShared;
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

            var outputStream = new MemoryStream();
            var outputStreamWriter = new TcpStreamWriter(outputStream, client);
            _engine.Runtime.IO.SetOutput(outputStream, outputStreamWriter);
        }

        public void RunScript(object obj)
        {
            RunScriptMessage msg = (RunScriptMessage)obj;

            // TODO check if state is valid

            var source = _engine.CreateScriptSourceFromString(msg.script, SourceCodeKind.Statements);
            var compiled = source.Compile();
            compiled.Execute(_scope);
        }
    }
}
