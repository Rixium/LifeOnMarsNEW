using LoM.Managers;
using Microsoft.Xna.Framework.Input;

namespace LoM.Util
{
    public class CameraController
    {
        private const float ZoomSpeed = 0.02f;
        private const int CameraSpeed = 5;

        private readonly Camera _camera;

        public CameraController(Camera camera)
        {
            _camera = camera;
        }

        public void MoveCamera(Keys key)
        {
            if (key == Keys.W || key == Keys.Up)
                _camera.Move(0, -CameraSpeed);
            else if (key == Keys.A || key == Keys.Left)
                _camera.Move(-CameraSpeed, 0);
            else if (key == Keys.D || key == Keys.Right)
                _camera.Move(CameraSpeed, 0);
            else if (key == Keys.S || key == Keys.Down) _camera.Move(0, CameraSpeed);
            else if (key == Keys.OemPeriod) _camera.Zoom(ZoomSpeed);
            else if (key == Keys.OemComma) _camera.Zoom(-ZoomSpeed);
        }

        public void SetupKeys(InputManager inputManager)
        {
            inputManager.RegisterOnKeyDown(Keys.W, MoveCamera);
            inputManager.RegisterOnKeyDown(Keys.S, MoveCamera);
            inputManager.RegisterOnKeyDown(Keys.A, MoveCamera);
            inputManager.RegisterOnKeyDown(Keys.D, MoveCamera);
            inputManager.RegisterOnKeyDown(Keys.Up, MoveCamera);
            inputManager.RegisterOnKeyDown(Keys.Down, MoveCamera);
            inputManager.RegisterOnKeyDown(Keys.Left, MoveCamera);
            inputManager.RegisterOnKeyDown(Keys.Right, MoveCamera);
            inputManager.RegisterOnKeyDown(Keys.OemPeriod, MoveCamera);
            inputManager.RegisterOnKeyDown(Keys.OemComma, MoveCamera);
        }

    }
}