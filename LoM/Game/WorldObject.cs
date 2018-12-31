using System.Text;
using LoM.Constants;

namespace LoM.Game
{
    public class WorldObject
    {

        private World _world;

        public WorldObject(Tile tile, ObjectType objectType)
        {
            Tile = tile;
            _world = tile.World;
            ObjectType = objectType;
        }

        public Tile Tile { get; }
        public ObjectType ObjectType { get; }
        public bool MergesWithNeighbors { get; set; } = true;

        public string GetNeighborString()
        {
            var tileX = Tile.X;
            var tileY = Tile.Y;

            var stringBuilder = new StringBuilder(4);

            var northTile = _world.GetTileAt(tileX, tileY - 1);
            var eastTile = _world.GetTileAt(tileX + 1, tileY);
            var southTile = _world.GetTileAt(tileX, tileY + 1);
            var westTile = _world.GetTileAt(tileX - 1, tileY);

            if (northTile?.WorldObject?.ObjectType == ObjectType)
                stringBuilder.Append("N");
            if (eastTile?.WorldObject?.ObjectType == ObjectType)
                stringBuilder.Append("E");
            if (southTile?.WorldObject?.ObjectType == ObjectType)
                stringBuilder.Append("S");
            if (westTile?.WorldObject?.ObjectType == ObjectType)
                stringBuilder.Append("W");

            return stringBuilder.ToString();
        }

    }
}