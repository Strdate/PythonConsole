using SkylinesPythonShared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkylinesRemotePython.API
{
    public class CitiesObjectEnumerable<T,K> : IEnumerable where T : CitiesObject where K : InstanceMessage
    {
        public IEnumerator GetEnumerator()
        {
            return new CitiesObjectEnumerator<T,K>();
        }
    }

    public class CitiesObjectEnumerator<T, K> : IEnumerator where T : CitiesObject where K : InstanceMessage
    {
        private T[] treeBuffer = new T[0];
        private int pointer = -1;
        private uint gamePointer;
        private bool endOfStream;
        private long handle = -1;

        private Action<object> GetCallbackMethod()
        {
            return (obj) => {
                handle = -1;
                var batch = (BatchObjectMessage)obj;
                var trees = MsgToTrees(batch);
                var newTrees = new T[treeBuffer.Length + trees.Length - pointer];
                Array.Copy(treeBuffer, pointer, newTrees, 0, treeBuffer.Length - pointer);
                Array.Copy(trees, 0, newTrees, treeBuffer.Length - pointer, trees.Length);
                treeBuffer = newTrees;
                pointer = 0;
            };
        }

        private T[] MsgToTrees(BatchObjectMessage msg)
        {
            gamePointer = msg.lastVisitedIndex;
            endOfStream = msg.endOfStream;
            var api = PythonEngine.Instance._gameAPI;
            return msg.array.Select((obj) => CreeateObject((K)obj, api)).ToArray();
        }

        private T CreeateObject(K msg, GameAPI api)
        {
            // let me die
            if(typeof(T) == typeof(Tree)) {
                return (T)(object)new Tree((TreeMessage)(object)msg, api);
            }
            return null;
        }

        public CitiesObjectEnumerator() {
            
        }

        public object Current => treeBuffer[pointer];

        public bool MoveNext()
        {
            pointer++;
            if(pointer >= treeBuffer.Length) {
                if(endOfStream) {
                    return false;
                } else {
                    if(handle == -1) {
                        handle = AsyncCallbackHandler.Instance.Call(GetCallbackMethod(), Contracts.GetTreesStartingFromIndex, gamePointer);
                    }
                    AsyncCallbackHandler.Instance.WaitOnHandle(handle);
                }
            }
            if(treeBuffer.Length - pointer < 100 && handle == -1 && !endOfStream) {
                handle = AsyncCallbackHandler.Instance.Call(GetCallbackMethod(), Contracts.GetTreesStartingFromIndex, gamePointer);
            }
            return true;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
