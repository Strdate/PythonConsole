using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PythonConsole
{
    public class UIWindow : MonoBehaviour
    {
        private static UIWindow _instance;
        
        private string scriptText = "print(\"Hello world! :)\")";
        private Vector2 scrollPosScript;
        private Vector2 scrollPosOut;
        public static UIWindow Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = FindObjectOfType<UIWindow>();
                }
                if (!_instance)
                {
                    _instance = new GameObject("Python Console Panel").AddComponent<UIWindow>();
                    _instance.enabled = false;
                }
                return _instance;
            }
        }

        private string OutputText { get; set; } = "";

        void OnGUI()
        {
            float Width = 500;
            float Height = 500;

            Rect windowRect = new Rect((Screen.width - Width) / 2, (Screen.height - Height) / 2, Width, Height);
            GUI.Window(664, windowRect, _populateWindow, "Python Console");
        }

        public void Log(string text)
        {
            OutputText = OutputText + text;
        }

        private void _populateWindow(int num)
        {
            GUILayout.BeginVertical();

            scrollPosScript = GUILayout.BeginScrollView(scrollPosScript);
            scriptText = GUILayout.TextArea(scriptText, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUILayout.EndScrollView();

            if(GUILayout.Button(PythonConsole.Instsance.CanExecuteScript ? "Execute" : "Busy..."))
            {
                PythonConsole.Instsance.ScheduleExecution(scriptText);
            }

            scrollPosOut = GUILayout.BeginScrollView(scrollPosOut);
            GUILayout.TextArea(OutputText, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }
    }
}
