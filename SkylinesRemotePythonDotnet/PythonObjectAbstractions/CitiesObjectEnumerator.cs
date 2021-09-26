using SkylinesPythonShared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkylinesRemotePython.API
{
    public class CitiesObjectEnumerable<T,U> : IEnumerable
        where T : InstanceData
        where U : CitiesObject<T, U>, new()
    {
        public IEnumerator GetEnumerator()
        {
            return new CitiesObjectEnumerator<T,U>();
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
        private long handle = -1;
        private string error;

        private Func<object, string, object> GetCallbackMethod()
        {
            return (ret, error) => {
                if (error != null) {
                    this.error = error;
                    return null;
                }
                handle = -1;
                var batch = (BatchObjectMessage)ret;

                batch.array.Select((item) => {
                    T data = (T)item;
                    return storage.SaveData(data);
                });

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

        public object Current => {

            }

        public bool MoveNext()
        {

            
          /*if(error != null) {
                throw new Exception(error);
            }
            pointer++;
            if(pointer >= buffer.Length) {
                if(endOfStream) {
                    return false;
                } else {
                    if(handle == -1) {
                        handle = ClientHandler.Instance.RemoteCall(GetContract(), gamePointer, GetCallbackMethod());
                    }
                    ClientHandler.Instance.WaitOnHandle(handle);
                }
            }
            if(buffer.Length - pointer < 100 && handle == -1 && !endOfStream) {
                handle = ClientHandler.Instance.RemoteCall(GetContract(), gamePointer, GetCallbackMethod());
            }
            return true;*/
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
