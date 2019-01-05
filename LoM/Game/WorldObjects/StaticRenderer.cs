using LoM.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LoM.Game.WorldObjects
{
    public class StaticRenderer : IRenderer
    {
        public WorldObject Owner;

        public void Draw(SpriteBatch spriteBatch, ContentChest contentChest)
        {
            var objectType = Owner.ObjectName;
            var name = $"{objectType}";

            if (Owner.MergesWithNeighbors)
            {
                var neighborString = RenderHelper.CreateNeighborString(Owner);
                if (!string.IsNullOrWhiteSpace(neighborString))
                    name = $"{objectType}_{neighborString}";
            }

            spriteBatch.Draw(contentChest.WorldObjects[name], new Vector2(Owner.Tile.X * 32, Owner.Tile.Y * 32),
                Color.White);
        }

        public IRenderer Clone()
        {
            return new StaticRenderer();
        }

        public void SetOwner(WorldObject owner)
        {
            Owner = owner;
        }
    }
}