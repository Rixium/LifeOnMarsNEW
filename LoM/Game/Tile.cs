using System;

namespace LoM.Game
{
    public class Tile
    {
        
        public TileType Type { get; private set; }
        public WorldObject WorldObject { get; private set; }
        public World World { get; set; }

        public float MovementCost
        {
            get
            {
                if (WorldObject != null)
                    return WorldObject.MovementCost;
                return 1;
            }
        }

        public Action<Tile> OnTileChanged;

        public int X;
        public int Y;

        public Tile(int x, int y, World world)
        {
            World = world;
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

        public void RemoveWorldObject()
        {
            if (WorldObject == null) return;

            WorldObject = null;
            OnTileChanged?.Invoke(this);
        }

        public Tile[] GetNeighbors()
        {
            var tiles = new Tile[4];
            tiles[0] = World.GetTileAt(X, Y - 1);
            tiles[1] = World.GetTileAt(X + 1, Y);
            tiles[2] = World.GetTileAt(X, Y + 1);
            tiles[3] = World.GetTileAt(X - 1, Y);

            return tiles;
        }

    }
}