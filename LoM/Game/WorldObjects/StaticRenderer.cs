using LoM.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LoM.Game.WorldObjects
{
    public class StaticRenderer : IRenderer
    {
        public ContentChest ContentChest;

        public bool Rotated { get; set; }

        public void Draw(SpriteBatch spriteBatch, WorldObject owner)
        {
            var objectType = owner.ObjectName;
            var name = $"{objectType}";

            if (owner.MergesWithNeighbors)
            {
                var neighborString = RenderHelper.CreateNeighborString(owner);
                if (!string.IsNullOrWhiteSpace(neighborString))
                    name = $"{objectType}_{neighborString}";
            }

            if (ContentChest.WorldObjects.ContainsKey(name) == false)
                return;

            // TODO THIS TILE SIZE.
            spriteBatch.Draw(ContentChest.WorldObjects[name], new Vector2(owner.Tile.X * 32, owner.Tile.Y * 32),
                Color.White);
        }

        public void Update(float deltaTime)
        {
        }

        public IRenderer Clone()
        {
            return new StaticRenderer
            {
                ContentChest = ContentChest
            };
        }
    }
}