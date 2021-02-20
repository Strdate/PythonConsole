using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework.Math;
using SkylinesPythonShared;
using UnityEngine;

namespace PythonConsole
{
    public class RenderableVector : RenderableObj
    {
        private Bezier3 _bezier;
        private Color _color;
        private float _size;
        public RenderableVector(Vector3 vector, Vector3 origin, Color color, float length = 20, float size = 0.1f)
        {
            Vector2 vector2d = new Vector2(vector.x, vector.z).normalized * length;
            Vector2 a = new Vector2(origin.x, origin.z);
            Vector2 b = a + (vector2d * (1 / 3));
            Vector2 c = a + (vector2d * (2 / 3));
            Vector2 d = a + vector2d;
            _size = size;
            _color = color;
            _bezier = new Bezier2(a, b, c, d).ShiftTo3D();
        }

        public RenderableVector(RenderVectorMessage msg) : this(msg.vector.ToUnity(), msg.origin.ToUnity(), Color.red, msg.length, msg.size)
        {
            
        }

        public override void Render(RenderManager.CameraInfo cameraInfo)
        {
            RenderManager.instance.OverlayEffect.DrawBezier(cameraInfo, _color, _bezier, _size, 0, 0, -1f, 1280f, false, true);
        }
    }
}
