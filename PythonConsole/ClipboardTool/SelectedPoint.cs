using UnityEngine;

namespace PythonConsole
{
    public class SelectedPoint
    {
        public Vector3 position { get; private set; }

        public SelectedPoint(Vector3 vector)
        {
            position = vector;
        }

    }
}