using Microsoft.Xna.Framework.Graphics;

namespace LoM.Game.WorldObjects
{
    public interface IRenderer
    {

        bool Rotated { get; set; }
        void Draw(SpriteBatch spriteBatch, WorldObject owner);
        void Update(float deltaTime);
        IRenderer Clone();
    }
}