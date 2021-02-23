using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PythonConsole
{
    public abstract class RenderableObj
    {
        public abstract void Render(RenderManager.CameraInfo cameraInfo);

        protected static Color ToUnityColor(string name)
        {
            Color color;
            switch (name) {
                case "cyan": color = Color.cyan; break;
                case "green": color = Color.green; break;
                case "red": color = Color.red; break;
                case "black": color = Color.black; break;
                case "yellow": color = Color.yellow; break;
                case "blue": color = Color.blue; break;
                case "magenta": color = Color.magenta; break;
                case "gray": color = Color.grey; break;
                case "white": color = Color.white; break;
                case "clear": color = Color.clear; break;
                case "grey": color = Color.grey; break;
                default: color = Color.black; break;
            }

            return color;
        }
    }
}
