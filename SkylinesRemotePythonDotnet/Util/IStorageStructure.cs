using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkylinesRemotePython
{
    public interface IStorageStructure<V,T>
    {
        bool TryGetValue(V id, out T data);

        T this[V index] { set; }

        void Remove(V id);

        void Clear();
    }
}
