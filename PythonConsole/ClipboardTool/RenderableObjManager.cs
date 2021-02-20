using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PythonConsole
{
    public class RenderableObjManager : IEnumerable<KeyValuePair<int, RenderableObj>>
    {
        private Dictionary<int, RenderableObj> _dict = new Dictionary<int, RenderableObj>();

        private int _identity = 0;

        public int AddObj(RenderableObj obj)
        {
            _dict.Add(++_identity, obj);
            return _identity;
        }

        public bool RemoveObj(int id)
        {
            if(id == 0) {
                _dict.Clear();
                return true;
            }
            return _dict.Remove(id);
        }

        public IEnumerator<KeyValuePair<int, RenderableObj>> GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
