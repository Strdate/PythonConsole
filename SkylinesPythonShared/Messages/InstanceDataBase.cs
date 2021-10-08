using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared
{
    [Serializable]
    public abstract class InstanceDataBase<T>
    {
        public T id { get; set; }
    }
}
