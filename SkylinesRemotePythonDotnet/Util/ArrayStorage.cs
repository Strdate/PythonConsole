using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkylinesRemotePython
{
    public class ArrayStorage<T>: IStorageStructure<uint, T>
        where T: class        
    {
        private T[] _arr;
        public ArrayStorage(int capacity)
        {
            _arr = new T[capacity];
        }

        public T this[uint index] { set => _arr[index] = value; }

        public void Clear()
        {
            Array.Clear(_arr, 0, _arr.Length);
        }


        public void Remove(uint id)
        {
            _arr[id] = null;
        }

        public bool TryGetValue(uint id, out T data)
        {
            data = _arr[id];
            return data != null;
        }
    }
}
