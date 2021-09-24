using SkylinesPythonShared;
using SkylinesRemotePython.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkylinesRemotePython
{
    public class ObjectInstanceStorage<T, U>  
        where T : InstanceData
        where U : CitiesObject<T>, new()
    {
        private Dictionary<uint, U> _dict = new Dictionary<uint, U>();

        private string _type;

        public ObjectInstanceStorage(string type)
        {
            _type = type;
        }

        public U Get(uint id, bool forceRefresh = false)
        {
            U val = GetShell(id, out bool needsRefresh);
            if(forceRefresh || needsRefresh) {
                T data = (T)ClientHandler.Instance.RemoteCall<InstanceData>(
                    Contracts.GetObjectFromId,
                    new GetObjectMessage() {
                        id = id,
                        type = _type
                    });
                val.AssignData(data);
            }
            return val;
        }

        public void RefreshInstance(U instance)
        {
            // issue - make accelerated
            T data = (T)ClientHandler.Instance.RemoteCall<InstanceData>(
                    Contracts.GetObjectFromId,
                    new GetObjectMessage() {
                        id = instance.id,
                        type = _type
                    });
            instance.AssignData(data);
        }

        public U SaveData(T data)
        {
            U val = GetShell(data.id, out bool needsRefresh);
            val.AssignData(data);
            return val;
        }

        private U GetShell(uint id, out bool needsRefresh)
        {
            U val;
            if (!_dict.TryGetValue(id, out val)) {
                CreateShell();
                AddShellToDictionary(id, val);
                needsRefresh = true;
            } else {
                needsRefresh = false;
            }
            return val;
        }

        public U CreateShell()
        {
            try {
                CitiesObjectController.AllowInstantiation = true;
                return new U();
            }
            finally {
                CitiesObjectController.AllowInstantiation = false;
            }
        }

        public void AddShellToDictionary(uint id, U shell)
        {
            _dict.Add(id, shell);
        }
    }
}
