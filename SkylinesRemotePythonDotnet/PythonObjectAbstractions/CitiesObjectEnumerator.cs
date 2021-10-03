using SkylinesPythonShared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkylinesRemotePython.API
{
    public class CitiesObjectEnumerable<U, T> : IEnumerable
        where T : InstanceData
        where U : CitiesObject<T, U>, new()
    {
        public IEnumerator GetEnumerator()
        {
            return new CitiesObjectEnumerator<T, U>();
        }
    }

    public class CitiesObjectEnumerator<T, U> : IEnumerator
        where T : InstanceData
        where U : CitiesObject<T, U>, new()
    {
        private CitiesObjectStorage<T, U, uint> storage;

        public CitiesObjectEnumerator()
        {
            storage = GetStorage();
        }

        private uint pointer;
        private uint gamePointer;
        private bool endOfStream;
        private CallbackHandle handle;
        private string error;

        private Func<object, string, object> GetCallbackMethod()
        {
            return (ret, error) => {
                if (error != null) {
                    this.error = error;
                    return null;
                }
                handle = null;
                var batch = (BatchObjectMessage)ret;
                foreach(var item in batch.array) {
                    T data = (T)item;
                    storage.SaveData(data);
                }

                gamePointer = batch.lastVisitedIndex;
                endOfStream = batch.endOfStream;
                return null;
            };
        }

        private CitiesObjectStorage<T, U, uint> GetStorage()
        {
            // let me die
            if (typeof(T) == typeof(PropData)) {
                return (CitiesObjectStorage<T, U, uint>)(object)ObjectStorage.Instance.Props;
            } else if (typeof(T) == typeof(TreeData)) {
                return (CitiesObjectStorage<T, U, uint>)(object)ObjectStorage.Instance.Trees;
            } else if (typeof(T) == typeof(BuildingData)) {
                return (CitiesObjectStorage<T, U, uint>)(object)ObjectStorage.Instance.Buildings;
            } else if (typeof(T) == typeof(NetNodeData)) {
                return (CitiesObjectStorage<T, U, uint>)(object)ObjectStorage.Instance.Nodes;
            } else if (typeof(T) == typeof(NetSegmentData)) {
                return (CitiesObjectStorage<T, U, uint>)(object)ObjectStorage.Instance.Segments;
            }
            throw new Exception("Engine error (report to developers). Cannot convert unknown type to CitiesObject");
        }

        public object Current {
            get {
                U obj = storage.GetCached(pointer);
                if(obj == null) {
                    throw new Exception("Internal error: null data in Current"); // feature - detect wiped cache and ask for new data
                }
                return obj;
            }
        }


        public bool MoveNext()
        {
            if(error != null) {
                throw new Exception(error);
            }
            if(pointer == gamePointer && endOfStream) {
                return false;
            }
            if(gamePointer - pointer < 100 && handle == null && !endOfStream) {
                AskForData();
            }
            if(pointer == gamePointer) {
                ClientHandler.Instance.WaitOnHandle(handle);
            }
            while(pointer < gamePointer && storage.GetCached(pointer) == null) {
                pointer++;
            }
            return true;
        }

        public void Dispose()
        {
            storage.Clear();
        }

        private void AskForData()
        {
            var param = new GetObjectsFromIndexMessage() { index = gamePointer, type = storage.Type };
            handle = ClientHandler.Instance.RemoteCall(Contracts.GetObjectsStartingFromIndex, param, GetCallbackMethod());
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
