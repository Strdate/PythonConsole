using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PythonConsole
{
    public class UnityPythonObject : MonoBehaviour
    {
        public static UnityPythonObject Instance;

        public const string settingsFileName = "PythonConsoleMod";
        private const string ConfigPath = "PythonConsoleMod.xml";

        internal ScriptEditor scriptEditor;

        public ModConfiguration Config { get; set; } = new ModConfiguration();

        public void Start()
        {
            scriptEditor = gameObject.AddComponent<ScriptEditor>();
            LoadConfig();
        }

        public void Print(string text)
        {
            scriptEditor.PrintOutput(text);
        }

        public void PrintError(string text)
        {
            scriptEditor.PrintError(text);
        }

        public void Update()
        {
            scriptEditor.OnUpdate();
            if (!Input.GetKey(KeyCode.LeftAlt))
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                if(PythonConsole.Instance == null) {
                    PythonConsole.CreateInstance();
                }
                scriptEditor.Visible = !scriptEditor.Visible;
            }
        }

        public void OnGUI()
        {
            SelectionTool.Instance.DrawVarLabels();
        }

        public void SaveConfig()
        {
            if (Config == null)
            {
                return;
            }
            Config.Serialize(ConfigPath);
        }

        public void LoadConfig()
        {
            Config = ModConfiguration.Deserialize(ConfigPath);
            if (Config == null)
            {
                Config = new ModConfiguration();
                SaveConfig();
            }

            scriptEditor.ReloadProjectWorkspace();
        }
    }
}
