using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LoM
{
    public class Button : UIElement
    {
        
        public int X;
        public int Y;
        
        private bool _pressed;

        private Rectangle _rectangle;
        private readonly ElementSettings _settings;

        public Action<Button> OnClick;
        
        public Button(int x, int y, ElementSettings settings)
        {
            X = x;
            Y = y;
            _settings = settings;

            CreateBounds();
        }

        private void CreateBounds()
        {
            var image = _settings.ImageOff;
            _rectangle = new Rectangle(X, Y, image.Width, image.Height);
        }

        public ElementType Type()
        {
            return ElementType.Button;
        }

        public Texture2D Image
        {
            get
            {
                if (!_pressed) return _settings.ImageOff;
                return _settings.ImagePressed;
            }
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
            if(!_pressed)
                OnClick?.Invoke(this);

            _pressed = true;
        }

        public void Release()
        {
            _pressed = false;
        }


        public ElementSettings GetSettings()
        {
            return _settings;
        }

        public Rectangle GetBounds()
        {
            return _rectangle;
        }
        
    }
}