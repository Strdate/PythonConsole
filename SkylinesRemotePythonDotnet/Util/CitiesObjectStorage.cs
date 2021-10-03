using SkylinesPythonShared;
using SkylinesRemotePython.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkylinesRemotePython
{
    public class CitiesObjectStorage<T, U, V> where T : InstanceDataBase<V>
        where U : CitiesObjectBase<T, U, V>, new()
    {
        private IStorageStructure<V, T> _storage;

        private string _type;

        public string Type => _type;
        public CitiesObjectStorage(string type, IStorageStructure<V, T> structure)
        {
            _type = type;
            _storage = structure;
        }

        public U GetById(V id, bool forceRefresh = false)
        {
            U shell = CreateShell();
            T data;
            if (!_storage.TryGetValue(id, out data) || forceRefresh) {
                RefreshInstance(id, shell);
            } else {
                shell.AssignData(data);
            }
            return shell;
        }

        public U GetCached(V id)
        {
            T data;
            if (_storage.TryGetValue(id, out data)) {
                U shell = CreateShell();
                shell.AssignData(data);
                return shell;
            }
            return null;
        }

        public void RefreshInstance(V id, U shell = null)
        {
            WipeFromCache(id);
            ClientHandler.Instance.RemoteCall(
                Contracts.GetObjectFromId,
                new GetObjectMessage() {
                    id = id as uint?,
                    idString = id as string,
                    type = _type
                }, (ret, err) => {
                    T retData = (T)ret;
                    AddDataToDictionary(retData);
                    if (shell != null) {
                        shell.AssignData(retData);
                    }
                    return null;
                });
        }

        public U SaveData(T data)
        {
            if (data == null) {
                throw new Exception("Data cannot be null");
            }
            U val = CreateShell();
            val.AssignData(data);
            _storage[data.id] = data;
            return val;
        }

        public T GetData(V id)
        {
            T val;
            if (!_storage.TryGetValue(id, out val)) {
                val = (T)ClientHandler.Instance.SynchronousCall<InstanceDataBase<V>>(
                    Contracts.GetObjectFromId,
                    new GetObjectMessage() {
                        id = id as uint?,
                        idString = id as string,
                        type = _type
                    });
                _storage[id] = val;
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

        public void AddDataToDictionary(T data)
        {
            _storage[data.id] = data;
        }

        public void WipeFromCache(V id)
        {
            _storage.Remove(id);
        }

        public void Delete(uint id, bool keep_nodes = false)
        {
            WipeFromCache((V)((object)id));
            ClientHandler.Instance.RemoteCall(
                Contracts.DeleteObject,
                    new DeleteObjectMessage() {
                        id = id,
                        type = _type,
                        keep_nodes = keep_nodes
                    }, (ret, err) => {
                        T retData = (T)ret;
                        AddDataToDictionary(retData);
                        return null;
                    });
        }

        public void Clear()
        {
            _storage.Clear();
        }
    }
}
