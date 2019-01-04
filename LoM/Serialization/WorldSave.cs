using System.Collections.Generic;

namespace LoM.Serialization
{
    public class WorldSave
    {

        public List<TileData> Tiles { get; set; } = new List<TileData>();
        public List<WorldObjectData> WorldObjects { get; set; } = new List<WorldObjectData>();
    }
}