using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PythonConsole
{
    public class Threading : ThreadingExtensionBase
    {

        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            PythonConsole.Instance.SimulationStep();
        }
    }
}