using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PythonConsole
{
    public class HandleCall
    {
        public static void HandleAPICall(object msg, string type, TcpClient client)
        {
            try
            {
                switch (type)
                {
                    case "c_callfunc_CreateProp": GameAPI.CreateProp(msg, client); break;
                }
            }
            catch(Exception ex)
            {
                client.SendMessage(ex.Message, "s_exception");
            }
        }

        public static UnityEngine.Vector3 ConvertVector(SkylinesPythonShared.API.Vector3 vect)
        {
            return new UnityEngine.Vector3((float)vect.X, (float)vect.Y, (float)vect.Z);
        }
    }
}
