using System;
using LoM.Constants;
using LoM.Game.WorldObjects;
using LoM.Serialization.Data;
using Microsoft.Xna.Framework.Graphics;

namespace LoM.Game
{
    public class WorldObject
    {
        public IBehaviour Behaviour;

        public ItemRequirements[] ItemRequirements;
        public string ObjectName;

        public Action<WorldObject> OnChange;
        public IRenderer Renderer;

        protected WorldObject()
        {
        }

        public Tile Tile { get; private set; }
        public ObjectType ObjectType { get; private set; }

        // TODO We can move all this data in to a new class (WorldObjectSettings?)
        public bool HollowPlacement { get; set; }
        public bool MergesWithNeighbors { get; set; }
        public bool DragBuild { get; set; }
        public bool Encloses { get; set; }
        public float MovementCost { get; set; }
        public bool CanRotate { get; set; }
        public bool StoresItems { get; set; }
        public bool DestroyOnPlace { get; set; }

        public bool IsPassable => Behaviour == null || Behaviour.IsPassable();
        public Texture2D Image { get; set; }

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
                Renderer = Renderer?.Clone(),
                DestroyOnPlace = DestroyOnPlace,
                StoresItems = StoresItems
            };

            if (ItemRequirements != null)
            {
                CreateItemRequirementCopy(clonedCopy);
            }

            clonedCopy.Behaviour = Behaviour?.Clone(clonedCopy.Renderer);
            clonedCopy.Behaviour?.SetOwner(clonedCopy);

            return clonedCopy;
        }

        private void CreateItemRequirementCopy(WorldObject clonedCopy)
        {
            var newArray = new ItemRequirements[ItemRequirements.Length];
            int curr = 0;
            foreach (var item in ItemRequirements)
            {
                var newItem = new ItemRequirements
                {
                    Amount = item.Amount,
                    Type = item.Type
                };
                newArray[curr++] = newItem;

            }

            clonedCopy.ItemRequirements = newArray;
        }

        public static WorldObject CreatePrototype(string objectName, bool hollowPlacement, bool mergeWithNeighbors,
            bool dragBuild,
            bool encloses,
            float movementCost, bool canRotate, bool destroyOnPlace, bool storesItems, IBehaviour behaviour, IRenderer renderer)
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
                DestroyOnPlace = destroyOnPlace,
                StoresItems = storesItems,
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