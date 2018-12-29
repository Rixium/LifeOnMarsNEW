using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LoM
{
    public class Button : UIElement
    {

        public string Text;
        public int X;
        public int Y;

        public int TextX;
        public int TextY;

        private Rectangle _rectangle;
        private readonly ElementSettings _settings;
        
        public SpriteFont Font;

        public Button(SpriteFont font, string text, int x, int y, ElementSettings settings)
        {
            Font = font;
            Text = text;
            X = x;
            Y = y;
            _settings = settings;

            CreateBounds();
        }

        private void CreateBounds()
        {
            var textWidth = (int) Font.MeasureString(Text).X;
            var textHeight = (int) Font.MeasureString(Text).Y;

            TextX = X + _settings.Padding;
            TextY = Y + _settings.Padding;

            _rectangle = new Rectangle(X, Y, textWidth + (_settings.Padding * 2), textHeight + (_settings.Padding * 2));
        }

        public ElementType Type()
        {
            return ElementType.Button;
        }

        public Vector2 GetPosition()
        {
            return new Vector2(X, Y);
        }

        public bool CollidesWith(Rectangle position)
        {
            return _rectangle.Contains(position);
        }

        public void Click()
        {
            
        }

        public ElementSettings GetSettings()
        {
            return _settings;
        }

        public Rectangle GetBounds()
        {
            return _rectangle;
        }

        public Vector2 GetTextPosition()
        {
            return new Vector2(TextX, TextY);
        }
    }
}