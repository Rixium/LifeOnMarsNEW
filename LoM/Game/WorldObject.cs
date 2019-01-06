using System;
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

        public Action<WorldObject> OnChange;
        public Tile Tile { get; private set; }
        public ObjectType ObjectType { get; private set; }

        public bool HollowPlacement { get; set; } = true;
        public bool MergesWithNeighbors { get; set; } = true;
        public bool DragBuild { get; set; }
        public bool Encloses { get; set; }
        public float MovementCost { get; set; }
        public bool CanRotate { get; set; }

        public bool IsPassable => Behaviour == null || Behaviour.IsPassable();
        public Texture2D Image { get; set; }
        

        public IBehaviour Behaviour;
        public IRenderer Renderer;

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
                CanRotate = CanRotate,
                MovementCost = MovementCost,
                Renderer = Renderer?.Clone()
            };

            clonedCopy.Behaviour = Behaviour?.Clone(clonedCopy.Renderer);
            clonedCopy.Behaviour?.SetOwner(clonedCopy);

            return clonedCopy;
        }

        public static WorldObject CreatePrototype(string objectName, bool hollowPlacement, bool mergeWithNeighbors,
            bool dragBuild,
            bool encloses,
            float movementCost, bool canRotate, IBehaviour behaviour, IRenderer renderer)
        {
            return new WorldObject
            {
                ObjectName = objectName,
                HollowPlacement = hollowPlacement,
                MergesWithNeighbors = mergeWithNeighbors,
                MovementCost = movementCost,
                DragBuild = dragBuild,
                Encloses = encloses,
                CanRotate = canRotate,
                Behaviour = behaviour,
                Renderer = renderer
            };
        }

        public void Update(float deltaTime)
        {
            Renderer?.Update(deltaTime);
            Behaviour?.Update(deltaTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Renderer?.Draw(spriteBatch, this);
        }

    }
}