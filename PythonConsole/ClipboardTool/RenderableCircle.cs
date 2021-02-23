using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework.Math;
using SkylinesPythonShared;
using UnityEngine;

namespace PythonConsole
{
    public class RenderableCircle : RenderableObj
    {
        private Vector3 _position;
        private Color _color;
        private float _radius;
        public RenderableCircle(Vector3 vector, float radius, Color color)
        {
            _position = vector;
            _color = color;
            _radius = radius;
        }

        public RenderableCircle(RenderCircleMessage msg) : this(msg.position.ToUnity(), msg.radius, ToUnityColor(msg.color))
        {

        }

        public override void Render(RenderManager.CameraInfo cameraInfo)
        {
            RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo, _color, _position, _radius, _position.y - 1f, _position.y + 1f, true, true);
        }
    }
}
