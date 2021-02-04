using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private ScriptEngine _engine;
        private ScriptScope _scope;
        private string _scheduledScript;

        public PythonConsole()
        {
            _engine = Python.CreateEngine();
            _scope = _engine.CreateScope();

            var outputStream = new MemoryStream();
            var outputStreamWriter = new TextAreaStreamWriter(outputStream);
            _engine.Runtime.IO.SetOutput(outputStream, outputStreamWriter);
        }

        public void ScheduleExecution(string script)
        {
            _scheduledScript = script;
        }

        public void SimulationStep()
        {
            if(_scheduledScript != null)
            {
                var source = _engine.CreateScriptSourceFromString(_scheduledScript, SourceCodeKind.Statements);
                var compiled = source.Compile();
                compiled.Execute(_scope);
            }
        }
    }
}
