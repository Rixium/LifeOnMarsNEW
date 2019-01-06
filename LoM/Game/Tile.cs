using System;
using LoM.Game.Items;

namespace LoM.Game
{
    public class Tile
    {
        public Action<Tile> OnTileChanged;

        public Region Region;

        public int X;
        public int Y;

        public Tile(int x, int y, World world)
        {
            World = world;
            X = x;
            Y = y;
            Type = TileType.None;
        }

        public ItemStack ItemStack;
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
            tiles[0] = North();
            tiles[1] = East();
            tiles[2] = South();
            tiles[3] = West();
            return tiles;
        }

        public Tile North()
        {
            return World.GetTileAt(X, Y - 1);
        }

        public Tile South()
        {
            return World.GetTileAt(X, Y + 1);
        }

        public Tile East()
        {
            return World.GetTileAt(X + 1, Y);
        }

        public Tile West()
        {
            return World.GetTileAt(X - 1, Y);
        }

        public ItemStack DropItem(ItemStack stack)
        {
            if (ItemStack != null)
                return ItemStack.MergeWith(stack);

            ItemStack = stack;
            return null;
        } 

    }
}