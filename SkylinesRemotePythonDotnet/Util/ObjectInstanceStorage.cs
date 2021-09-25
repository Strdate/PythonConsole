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
        where U : CitiesObject<T,U>, new()
    {
        private Dictionary<uint, T> _dict = new Dictionary<uint, T>();

        private string _type;

        public ObjectInstanceStorage(string type)
        {
            _type = type;
        }

        public U GetById(uint id, bool forceRefresh = false)
        {
            U val = CreateShell();
            // issue - make accelerated
            T data = GetData(id);
            val.AssignData(data);
            return val;
        }

        public void RefreshInstance(uint id)
        {
            // issue - make accelerated
            T data = (T)ClientHandler.Instance.SynchronousCall<InstanceData>(
                    Contracts.GetObjectFromId,
                    new GetObjectMessage() {
                        id = id,
                        type = _type
                    });
            _dict[id] = data;
        }

        public U SaveData(T data)
        {
            U val = CreateShell();
            val.AssignData(data);
            _dict[data.id] = data;
            return val;
        }

        public T GetData(uint id)
        {
            T val;
            if(!_dict.TryGetValue(id, out val)) {
                val = (T)ClientHandler.Instance.SynchronousCall<InstanceData>(
                    Contracts.GetObjectFromId,
                    new GetObjectMessage() {
                        id = id,
                        type = _type
                    });
                _dict[id] = val;
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

        public void AddDataToDictionary(uint id, T data)
        {
            _dict[id] = data;
        }
    }
}
