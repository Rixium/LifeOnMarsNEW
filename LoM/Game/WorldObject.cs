using System.Text;
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

        public bool HollowPlacement { get; set; } = true;
        public bool MergesWithNeighbors { get; set; } = true;
        public float MovementCost { get; set; } = 0;

    }
}