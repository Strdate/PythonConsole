using SkylinesPythonShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkylinesRemotePython.API
{
    public abstract class CitiesObjectBase<T,U,V> : ShellObject<T>
        where T : InstanceDataBase<V>
        where U : CitiesObjectBase<T,U,V>, new()
    {
        private V _internalId { get; set; }

        protected override T Retrieve() => GetStorage().GetData(_internalId);

        private protected abstract CitiesObjectStorage<T, U, V> GetStorage();

        internal virtual void AssignData(InstanceDataBase<V> msg, string initializationErrorMsg = null)
        {
            Initialize(initializationErrorMsg);
            if(msg != null) {
                _internalId = ((T)msg).id;
            }
        }

        [Doc("Object type (node, buidling, prop etc.)")]
        public abstract string type { get; }


        public override string ToString()
        {
            return PythonHelp.RuntimeToString(this);
        }
    }

    internal class CitiesObjectController
    {
        // hack - types with new() constraint cannot have internal constructor which would prevent calling them from Python code
        [ThreadStatic]
        internal static bool AllowInstantiation;
    }
}
