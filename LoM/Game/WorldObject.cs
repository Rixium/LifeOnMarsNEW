using LoM.Constants;
using LoM.Game.WorldObjects;
using LoM.Util;
using Microsoft.Xna.Framework.Graphics;

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

        public bool IsPassable => Behaviour == null || Behaviour.IsPassable();
        public Texture2D Image { get; set; }

        public IBehaviour Behaviour;
        private IRenderer _renderer;

        public WorldObject Place(Tile tile)
        {
            var clonedCopy = new WorldObject
            {
                Tile = tile,
                ObjectName = ObjectName,
                ObjectType = ObjectType,
                HollowPlacement = HollowPlacement,
                MergesWithNeighbors = MergesWithNeighbors,
                DragBuild = DragBuild,
                Encloses = Encloses,
                MovementCost = MovementCost,
                _renderer = _renderer?.Clone()
            };

            clonedCopy.Behaviour = Behaviour?.Clone(clonedCopy._renderer);
            clonedCopy.Behaviour?.SetOwner(clonedCopy);

            return clonedCopy;
        }

        public static WorldObject CreatePrototype(string objectName, bool hollowPlacement, bool mergeWithNeighbors,
            bool dragBuild,
            bool encloses,
            float movementCost, IBehaviour behaviour, IRenderer renderer)
        {
            return new WorldObject
            {
                ObjectName = objectName,
                HollowPlacement = hollowPlacement,
                MergesWithNeighbors = mergeWithNeighbors,
                MovementCost = movementCost,
                DragBuild = dragBuild,
                Encloses = encloses,
                Behaviour = behaviour,
                _renderer = renderer
            };
        }

        public void Update(float deltaTime)
        {
            _renderer?.Update(deltaTime);
            Behaviour?.Update(deltaTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _renderer?.Draw(spriteBatch, this);
        }

    }
}