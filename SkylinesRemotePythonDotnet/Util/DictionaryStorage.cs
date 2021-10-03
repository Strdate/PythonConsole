using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkylinesRemotePython
{
    public class DictionaryStorage<V, T> : IStorageStructure<V, T>
    {
        private Dictionary<V, T> _dict = new Dictionary<V, T>();
        public DictionaryStorage()
        {

        }

        public T this[V index] { set => _dict[index] = value; }

        public void Clear()
        {
            _dict.Clear();
        }

        public void Remove(V id)
        {
            _dict.Remove(id);
        }

        public bool TryGetValue(V id, out T data)
        {
            return _dict.TryGetValue(id, out data);
        }
    }
}
