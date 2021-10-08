using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkylinesRemotePython.API
{
    public class PythonList<T> : ShellObject<IList<T>>, IList<T>
    {
        private IList<T> _internalList { get; set; }

        internal Action CacheFunc { get; set; }

        protected override IList<T> Retrieve()
        {
            if (_internalList == null) {
                CacheFunc();
            }
            return _internalList;
        }

        internal virtual void AssignData(IList<T> list, string initializationErrorMsg = null)
        {
            Initialize(initializationErrorMsg);
            _internalList = list;
        }

        internal PythonList() { }

        public T this[int index] { get => _[index]; }

        public int Count => _.Count;

        public bool IsReadOnly => _.IsReadOnly;

        T IList<T>.this[int index] { get => _[index]; set => _[index] = value; }

        public void Add(T item)
        {
            _.Add(item);
        }

        public void Clear()
        {
            _.Clear();
        }

        public bool Contains(T item)
        {
            return _.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return _.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _.Insert(index, item);
        }

        public bool Remove(T item)
        {
            return _.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _.GetEnumerator();
        }
    }
}
