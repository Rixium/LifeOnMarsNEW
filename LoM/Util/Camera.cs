﻿using LoM.Constants;
using Microsoft.Xna.Framework;

namespace LoM.Util
{
    public class Camera
    {

        public int X;
        public int Y;
        public float Scale = 2;
        public int MaxZoom = 3;


        public Vector2 ViewportCenter => new Vector2(Screen.Width * 0.5f, Screen.Height * 0.5f);


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
            return
                Matrix.CreateTranslation(-X, -Y, 0) *
                Matrix.CreateScale(Scale, Scale, 1) *
                Matrix.CreateTranslation(new Vector3(ViewportCenter, 0));
        }

        public void Zoom(float delta)
        {
            Scale += delta;
            Scale = MathHelper.Clamp(Scale, 1, MaxZoom);
        }

        public Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            return Vector2.Transform(screenPosition,
                Matrix.Invert(Get()));
        }

        public Vector2 WorldToScreen(Vector2 worldPosition)
        {
            return Vector2.Transform(worldPosition, Get());
        }
    }
}