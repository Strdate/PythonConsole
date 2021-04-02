using SkylinesPythonShared;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkylinesRemotePython.API
{
    [Doc("Information about natural resources on current map position. Resources are saved in discrete square grid (64*64 cells per tile). " +
        "If you modify the cells, game must be unpaused so the changes take effect")]
    public class NaturalResourceCell : ISimpleToString
    {
        [Doc("Internal cell id")]
        public int natural_resources_cell_id { get; private set; }

        [Doc("Internal row id")]
        public int natural_resources_row_id => natural_resources_cell_id % 512;

        [Doc("Internal column id")]
        public int natural_resources_column_id => natural_resources_cell_id / 512;

        [ToStringIgnore()]
        [Doc("Default position of the resource cell")]
        public Vector default_position {
            get {
                double x = (natural_resources_cell_id % 512 - 256) * 33.75d;
                double z = (natural_resources_cell_id / 512 - 256) * 33.75d;
                return Vector.vector_xz(x, z);
            }
        }

        [Doc("0-256")]
        public byte ore {
            get => _base.ore;
            set {
                _base.ore = value;
                ClientHandler.Instance.RemoteCall<object>(Contracts.SetNaturalResource, new SetNaturalResourceMessage {
                    cell_id = natural_resources_cell_id,
                    type = "ore",
                    value = value
                });
            }
        }

        [Doc("0-256")]
        public byte oil {
            get => _base.oil;
            set {
                _base.oil = value;
                ClientHandler.Instance.RemoteCall<object>(Contracts.SetNaturalResource, new SetNaturalResourceMessage {
                    cell_id = natural_resources_cell_id,
                    type = "oil",
                    value = value
                });
            }
        }

        [Doc("0-256")]
        public byte forest => _base.forest;

        [Doc("0-256")]
        public byte fertility {
            get => _base.fertility;
            set {
                _base.fertility = value;
                ClientHandler.Instance.RemoteCall<object>(Contracts.SetNaturalResource, new SetNaturalResourceMessage {
                    cell_id = natural_resources_cell_id,
                    type = "fertility",
                    value = value
                });
            }
        }

        [Doc("0-256")]
        public byte pollution {
            get => _base.pollution;
            set {
                _base.pollution = value;
                ClientHandler.Instance.RemoteCall<object>(Contracts.SetNaturalResource, new SetNaturalResourceMessage {
                    cell_id = natural_resources_cell_id,
                    type = "pollution",
                    value = value
                });
            }
        }

        [Doc("0-256")]
        public byte water => _base.water;

        [Doc("Reloads cached values from the game")]
        public void refresh() => CachedObjects.Instance.NaturalResources.InvalidateCache();

        public string SimpleToString()
        {
            return PythonHelp.RuntimeToString(this);
        }

        private ref NaturalResourceCellBase _base => ref CachedObjects.Instance.NaturalResources.FromId(natural_resources_cell_id);

        [Doc("Returns row ID of the natural resources coordinate system")]
        public static int row_id_from_vector(Vector pos)
        {
            return Clamp((int)(pos.x / 33.75f + 256f), 0, 511);
        }

        [Doc("Returns column ID of the natural resources coordinate system")]
        public static int column_id_from_vector(Vector pos)
        {
            return Clamp((int)(pos.z / 33.75f + 256f), 0, 511);
        }

        [Doc("Retrieves resource cell from internal coorinates")]
        public NaturalResourceCell(int rowID, int columnID)
        {
            if (rowID < 0 || rowID > 511 || columnID < 0 || columnID > 511) {
                throw new Exception("Column and row ID of resource cell must be between 0 and 511");
            }
            natural_resources_cell_id = columnID * 512 + rowID;
        }

        [Doc("Retrieves resource cell from vector")]
        public NaturalResourceCell(Vector pos)
        {
            int rowID = Clamp((int)(pos.x / 33.75f + 256f), 0, 511);
            int columnID = Clamp((int)(pos.z / 33.75f + 256f), 0, 511);
            natural_resources_cell_id = columnID * 512 + rowID;
        }

        private static int Clamp(int val, int min, int max)
        {
            if (min > val) return min;
            else if (val > max) return max;
            else return val;
        }
    }
}
