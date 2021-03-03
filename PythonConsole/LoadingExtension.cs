using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PythonConsole
{
    public class LoadingExtension : ILoadingExtension
    {
        public void OnCreated(ILoading loading)
        {

        }

        public void OnLevelLoaded(LoadMode mode)
        {
            if (UnityPythonObject.Instance == null)
            {
                UnityPythonObject.Instance = new GameObject("UnityPythonObject").AddComponent<UnityPythonObject>();
            }
            var selectionToolGo = new GameObject("SelectionToolControl");
            selectionToolGo.transform.parent = UnityPythonObject.Instance.transform;
            selectionToolGo.AddComponent<SelectionToolControl>();
        }

        public void OnLevelUnloading()
        {
            try {
                UnityPythonObject.Instance.scriptEditor.Visible = false;
            } catch { }
            var go = UnityEngine.Object.FindObjectOfType<SelectionToolControl>();
            if (go != null) {
                UnityEngine.Object.Destroy(go);
            }
            PythonConsole.KillInstance();
        }

        public void OnReleased()
        {

        }
    }
}
