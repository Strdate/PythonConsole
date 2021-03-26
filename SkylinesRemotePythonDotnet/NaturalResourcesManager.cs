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

        public NaturalResourcesManager(ClientHandler client)
        {
            _client = client;
        }

        public static NaturalResourceCell AtVector(Vector pos)
        {
            int num = Clamp((int)(pos.x / 33.75f + 256f), 0, 511);
            int num2 = Clamp((int)(pos.z / 33.75f + 256f), 0, 511);
            return new NaturalResourceCell(num2 * 512 + num);
        }

        public void InvalidateCache()
        {
            _cachedCells = null;
        }

        public NaturalResourceCellBase FromId(int id)
        {
            if(_cachedCells == null) {
                if(PythonHelp.NoCache) {
                   return _client.RemoteCall<NaturalResourceCellBase>(Contracts.GetNaturalResourceCellSingle, id);
                }
                _cachedCells = _client.RemoteCall<NaturalResourceCellBase[]>(Contracts.GetNaturalResourceCells, null);
            }
            return _cachedCells[id];
        }

        private static int Clamp(int val, int min, int max)
        {
            if (min > val) return min;
            else if (val > max) return max;
            else return val;
        }
    }

}
