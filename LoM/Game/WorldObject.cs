using LoM.Constants;

namespace LoM.Game
{
    public class WorldObject
    {
        public WorldObject(Tile tile, ObjectType objectType)
        {
            Tile = tile;
            ObjectType = objectType;
        }

        public Tile Tile { get; }
        public ObjectType ObjectType { get; }
    }
}