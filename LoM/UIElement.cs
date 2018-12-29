using Microsoft.Xna.Framework;

namespace LoM
{
    public interface UIElement
    {

        ElementType Type();
        Vector2 GetPosition();
        Rectangle GetBounds();
        bool CollidesWith(Rectangle rect);
        void Click();

        ElementSettings GetSettings();

    }
}