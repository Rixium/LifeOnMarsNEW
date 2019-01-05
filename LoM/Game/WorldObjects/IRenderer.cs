using LoM.Util;
using Microsoft.Xna.Framework.Graphics;

namespace LoM.Game.WorldObjects
{
    public interface IRenderer
    {

        void Draw(SpriteBatch spriteBatch, ContentChest contentChest);
        IRenderer Clone();
        void SetOwner(WorldObject owner);

    }
}