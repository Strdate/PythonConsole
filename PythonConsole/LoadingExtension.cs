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
        }

        public void OnLevelUnloading()
        {

        }

        public void OnReleased()
        {

        }
    }
}
