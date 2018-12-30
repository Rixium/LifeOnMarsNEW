using Microsoft.Xna.Framework;

namespace LoM.UI
{
    public interface UIElement
    {
        
        ElementType Type();
        Vector2 GetPosition();
        Rectangle GetBounds();
        bool CollidesWith(Rectangle rect);
        void Click();
        void Release();

        ElementSettings GetSettings();

    }
}