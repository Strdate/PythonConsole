using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PythonConsole
{
    public class UIWindow : MonoBehaviour
    {
        private static UIWindow _instance;
        
        private string scriptText = "";
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
                }
                return _instance;
            }
        }

        public string OutputText { get; set; } = "";

        void OnGUI()
        {
            float Width = 500;
            float Height = 500;

            Rect windowRect = new Rect((Screen.width - Width) / 2, (Screen.height - Height) / 2, Width, Height);
            GUI.Window(664, windowRect, _populateWindow, "Python Console");
        }

        private void _populateWindow(int num)
        {
            GUILayout.BeginVertical();

            scriptText = GUILayout.TextArea(scriptText, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if(GUILayout.Button("Execute"))
            {

            }
            GUILayout.TextArea(OutputText, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            GUILayout.EndVertical();
        }
    }
}
