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
            /*if(PythonConsole.Instance != null) {
                PythonConsole.Instance.SimulationStep();
            }*/
        }

        public override void OnBeforeSimulationTick()
        {
            if (PythonConsole.Instance != null) {
                PythonConsole.Instance.SimulationStep();
            }
        }
    }
}