using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkylinesRemotePython.API
{
    public class RemoteMethod
    {
        public string type_name { get; private set; }

        public bool is_static { get; private set; }

        public object invoke(params object[] parameters)
        {

        }
    }
}
