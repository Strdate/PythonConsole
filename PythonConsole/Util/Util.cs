using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PythonConsole
{
    public static  class Util
    {
        public static void Assert(object obj, string ex)
        {
            if (obj == null) {
                throw new Exception(ex);
            }
        }
    }
}
