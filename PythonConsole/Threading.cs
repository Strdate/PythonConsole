using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PythonConsole
{
    public class Threading : ThreadingExtensionBase
    {
        private static bool _processed = false;

        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            if (ModInfo.sc_toggleConsole.IsPressed())
            {
                if (!_processed)
                {
                    UIWindow.Instance.enabled = !UIWindow.Instance.enabled;
                    _processed = true;
                }
            }
            else
            {
                _processed = false;
            }

            PythonConsole.Instsance.SimulationStep();
        }
    }
}