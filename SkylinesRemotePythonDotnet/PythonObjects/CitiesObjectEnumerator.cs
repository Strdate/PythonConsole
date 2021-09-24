using SkylinesPythonShared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkylinesRemotePython.API
{
    public class CitiesObjectEnumerable<T,K> : IEnumerable where T : CitiesObject where K : InstanceData
    {
        public IEnumerator GetEnumerator()
        {
            return new CitiesObjectEnumerator<T,K>();
        }
    }

    public class CitiesObjectEnumerator<T, K> : IEnumerator where T : CitiesObject where K : InstanceData
    {
        private T[] buffer = new T[0];
        private int pointer = -1;
        private uint gamePointer;
        private bool endOfStream;
        private long handle = -1;

        private Action<object> GetCallbackMethod()
        {
            return (obj) => {
                handle = -1;
                var batch = (BatchObjectMessage)obj;
                var citiesObjects = MsgToCitiesObject(batch);
                var newObjects = new T[buffer.Length + citiesObjects.Length - pointer];
                Array.Copy(buffer, pointer, newObjects, 0, buffer.Length - pointer);
                Array.Copy(citiesObjects, 0, newObjects, buffer.Length - pointer, citiesObjects.Length);
                buffer = newObjects;
                pointer = 0;
            };
        }

        private T[] MsgToCitiesObject(BatchObjectMessage msg)
        {
            gamePointer = msg.lastVisitedIndex;
            endOfStream = msg.endOfStream;
            var api = PythonEngine.Instance._gameAPI;
            return msg.array.Select((obj) => CreeateObject((K)obj, api)).ToArray();
        }

        private T CreeateObject(K msg, GameAPI api)
        {
            // let me die
            if(typeof(T) == typeof(Prop)) {
                return (T)(object)new Prop((PropData)(object)msg, api);
            } else if (typeof(T) == typeof(Tree)) {
                return (T)(object)new Tree((TreeData)(object)msg, api);
            } else if (typeof(T) == typeof(Building)) {
                return (T)(object)new Building((BuildingData)(object)msg, api);
            } else if (typeof(T) == typeof(Node)) {
                return (T)(object)new Node((NetNodeData)(object)msg, api);
            } else if (typeof(T) == typeof(Segment)) {
                return (T)(object)new Segment((NetSegmentData)(object)msg, api);
            }
            throw new Exception("Engine error (report to developers). Cannot convert unknown type to CitiesObject");
        }

        private Contract GetContract()
        {
            if (typeof(T) == typeof(Prop)) {
                return Contracts.GetPropsStartingFromIndex;
            } else if (typeof(T) == typeof(Tree)) {
                return Contracts.GetTreesStartingFromIndex;
            } else if (typeof(T) == typeof(Building)) {
                return Contracts.GetBuildingsStartingFromIndex;
            } else if (typeof(T) == typeof(Node)) {
                return Contracts.GetNodesStartingFromIndex;
            } else if (typeof(T) == typeof(Segment)) {
                return Contracts.GetSegmentsStartingFromIndex;
            }
            throw new Exception("Engine error (report to developers). Cannot convert unknown type to CitiesObject");
        }

        public object Current => buffer[pointer];

        public bool MoveNext()
        {
            pointer++;
            if(pointer >= buffer.Length) {
                if(endOfStream) {
                    return false;
                } else {
                    if(handle == -1) {
                        handle = RemoteCaller.Instance.Call(GetCallbackMethod(), GetContract(), gamePointer);
                    }
                    RemoteCaller.Instance.WaitOnHandle(handle);
                }
            }
            if(buffer.Length - pointer < 100 && handle == -1 && !endOfStream) {
                handle = RemoteCaller.Instance.Call(GetCallbackMethod(), GetContract(), gamePointer);
            }
            return true;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
