using ColossalFramework;
using SkylinesPythonShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PythonConsole
{
    public class GameAPI
    {
        public static void CreateProp(object msg, TcpClient client)
        {
            var data = (CreatePropMessage)msg;
			ushort id;
            PropInfo info = PrefabCollection<PropInfo>.FindLoaded(data.Type);
            if (Singleton<PropManager>.instance.CreateProp(out id, ref Singleton<SimulationManager>.instance.m_randomizer, info, HandleCall.ConvertVector(data.Position), (float)data.Angle, true))
			{
                client.SendMessage((int)id, "s_ret_integer");
                return;
			}
            client.SendMessage("Failed to create prop", "s_exception");
        }
    }
}
