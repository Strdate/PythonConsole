using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkylinesRemotePython
{
    public class KeyValCache<K, V> where V: class
    {
        public delegate V CacheFunc(K key);

        private CacheFunc _func;
        private Dictionary<K, V> _dict = new Dictionary<K, V>();
        public KeyValCache(CacheFunc func)
        {
            _func = func;
        }

        public V Get(K key) {
            V _val;
            if(!_dict.TryGetValue(key, out _val)) {
                _val = _func(key);
                _dict[key] = _val;
            }
            return _val;
        }

        public void Invalidate(K key) => _dict.Remove(key);

        public void Invalidate() => _dict.Clear();
    }
}
