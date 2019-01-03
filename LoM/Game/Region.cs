using System;
using System.Collections.Generic;

namespace LoM.Game
{
    public class Region
    {

        public List<Tile> Tiles { get; private set; }
        public bool SpaceSafe;

        public Region(List<Tile> tiles)
        {
            Tiles = tiles;
        }

    }
}