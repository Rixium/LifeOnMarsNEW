using Microsoft.Xna.Framework;

namespace LoM
{
    public class Camera
    {
        public int X;
        public int Y;

        public Camera(int startX, int startY)
        {
            X = startX;
            Y = startY;
        }

        public void Move(int x, int y)
        {
            X += x;
            Y += y;
        }

        public Matrix Get()
        {
            return Matrix.CreateTranslation(X, Y, 0);
        }
        
    }
}