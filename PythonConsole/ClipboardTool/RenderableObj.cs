using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PythonConsole
{
    public abstract class RenderableObj
    {
        public abstract void Render(RenderManager.CameraInfo cameraInfo);
    }
}
