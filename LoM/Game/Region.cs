using System.Collections.Generic;

namespace LoM.Game
{
    public class Region
    {
        public bool SpaceSafe;

        public Region(List<Tile> tiles)
        {
            Tiles = tiles;
        }

        public List<Tile> Tiles { get; }
    }
}