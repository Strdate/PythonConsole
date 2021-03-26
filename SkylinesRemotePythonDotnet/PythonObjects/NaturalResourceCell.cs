using SkylinesPythonShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkylinesRemotePython.API
{
    [Doc("Information about natural resources on current map position")]
    public class NaturalResourceCell : ISimpleToString
    {
        [Doc("Internal cell id")]
        public int natural_resources_cell_id { get; private set; }

        [Doc("0-256")]
        public int ore => _base.ore;

        [Doc("0-256")]
        public byte oil => _base.oil;

        [Doc("0-256")]
        public byte forest => _base.forest;

        [Doc("0-256")]
        public byte fertility => _base.fertility;

        [Doc("0-256")]
        public byte pollution => _base.pollution;

        [Doc("0-256")]
        public byte water => _base.water;

        [Doc("Reloads cached values from the game")]
        public void refresh() => CachedObjects.Instance.NaturalResources.InvalidateCache();

        public string SimpleToString()
        {
            return PythonHelp.RuntimeToString(this);
        }

        private NaturalResourceCellBase _base => CachedObjects.Instance.NaturalResources.FromId(natural_resources_cell_id);

        internal NaturalResourceCell(int id)
        {
            natural_resources_cell_id = id;
        }
    }
}
