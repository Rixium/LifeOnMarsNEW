using LoM.Constants;

namespace LoM.Game
{
    public class WorldObject
    {
        public string ObjectName;

        protected WorldObject()
        {
        }

        public Tile Tile { get; private set; }
        public ObjectType ObjectType { get; private set; }

        public bool HollowPlacement { get; set; } = true;
        public bool MergesWithNeighbors { get; set; } = true;
        public bool DragBuild { get; set; }
        public bool Encloses { get; set; }
        public float MovementCost { get; set; }

        public WorldObject Place(Tile tile)
        {
            return new WorldObject
            {
                Tile = tile,
                ObjectName = this.ObjectName,
                ObjectType = this.ObjectType,
                HollowPlacement = this.HollowPlacement,
                MergesWithNeighbors = this.MergesWithNeighbors,
                DragBuild = this.DragBuild,
                Encloses = this.Encloses,
                MovementCost = this.MovementCost
            };
        }

        public static WorldObject CreatePrototype(string objectName, bool hollowPlacement, bool mergeWithNeighbors,
            bool dragBuild,
            bool encloses,
            float movementCost)
        {
            return new WorldObject
            {
                ObjectName = objectName,
                HollowPlacement = hollowPlacement,
                MergesWithNeighbors = mergeWithNeighbors,
                MovementCost = movementCost,
                DragBuild = dragBuild,
                Encloses = encloses
            };
        }
    }
}