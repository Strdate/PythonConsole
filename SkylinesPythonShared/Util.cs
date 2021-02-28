using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared
{
    public class Util
    {
        internal static string RuntimeToString(object obj)
        {
            string res = obj.GetType().Name;
            try {
                res = Type.GetType("SkylinesRemotePython.PythonHelp, SkylinesRemotePythonDotnet").GetMethod("RuntimeToString", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Invoke(null, new[] { obj }).ToString();
            }
            catch(Exception e) {
                Console.WriteLine("RuntimeToString failed: " + e.Message);
            }
            return res;
        }
    }
}
