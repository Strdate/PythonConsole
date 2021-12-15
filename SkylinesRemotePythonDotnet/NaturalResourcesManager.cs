using SkylinesPythonShared;
using SkylinesPythonShared.API;
using SkylinesRemotePython.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkylinesRemotePython
{
    public class NaturalResourcesManager
    {
        private ClientHandler _client;

        private NaturalResourceCellBase[] _cachedCells;

        // redundant hack - todo remove
        private Dictionary<int, CellWrapper> _cacheDict = new Dictionary<int, CellWrapper>();

        public NaturalResourcesManager(ClientHandler client)
        {
            _client = client;
        }

        public void InvalidateCache()
        {
            _cachedCells = null;
            _cacheDict.Clear();
        }

        public ref NaturalResourceCellBase FromId(int id)
        {
            if(_cachedCells == null) {
                if(PythonHelp.NoCache) {
                    if(!_cacheDict.TryGetValue(id, out CellWrapper wrapper)) {
                        var value = _client.SynchronousCall<NaturalResourceCellBase>(Contracts.GetNaturalResourceCellSingle, id);
                        wrapper = new CellWrapper(value);
                        _cacheDict[id] = wrapper;
                    }
                    return ref wrapper._base;
                }
                _cachedCells = _client.SynchronousCall<NaturalCellBaseListMessage>(Contracts.GetNaturalResourceCells, null).cells.ToArray();
            }
            return ref _cachedCells[id];
        }
    }

    public class CellWrapper
    {
        public NaturalResourceCellBase _base;
        public CellWrapper(NaturalResourceCellBase _base)
        {
            this._base = _base;
        }
    }
}
