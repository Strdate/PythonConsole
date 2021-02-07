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
                    case "c_callfunc_CreateNode": GameAPI.CreateNode(msg, client); break;
                    case "c_callfunc_ExistsPrefab": GameAPI.ExistsPrefab(msg, client); break;
                    case "c_callfunc_GetNodeFromId": GameAPI.GetNodeFromId(msg, client); break;
                    case "c_callfunc_GetPropFromId": GameAPI.GetPropFromId(msg, client); break;
                }
            }
            catch(Exception ex)
            {
                client.SendMessage(ex.Message, "s_exception");
            }
        }

        public static UnityEngine.Vector3 ConvertVector(SkylinesPythonShared.API.Vector vect)
        {
            return new UnityEngine.Vector3((float)vect.x, (float)vect.y, (float)vect.z);
        }

        public static SkylinesPythonShared.API.Vector ConvertVectorBack(UnityEngine.Vector3 vect)
        {
            return new SkylinesPythonShared.API.Vector(vect.x, vect.y, vect.z);
        }
    }
}
