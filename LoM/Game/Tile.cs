using System;

namespace LoM.Game
{
    public class Tile
    {
        public TileType Type { get; private set; }
        public WorldObject WorldObject { get; private set; }

        public Action<Tile> OnTileChanged;

        public int X;
        public int Y;

        public Tile(int x, int y)
        {
            X = x;
            Y = y;
            Type = TileType.None;
        }

        public void SetType(TileType tileType)
        {
            if (tileType != Type)
                OnTileChanged?.Invoke(this);
            Type = tileType;
        }

        public bool PlaceObject(WorldObject worldObject)
        {
            if (WorldObject != null) return false;

            WorldObject = worldObject;
            OnTileChanged?.Invoke(this);

            return true;
        }
        
    }
}