using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython
{
    public class CachedObj<T> where T : class
    {
        public delegate T CacheFunc();

        private CacheFunc _func;
        private T _obj;
        public CachedObj(CacheFunc func)
        {
            _func = func;
        }

        public T Get {
            get {
                if(_obj == null) {
                    _obj = _func();
                }
                return _obj;
            }
        }

        public void Invalidate() => _obj = null;
    }
}
