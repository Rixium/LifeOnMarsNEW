using System;
using LoM.Managers;

namespace LoM.Game.Build
{
    public class BuildManager
    {

        public BuildMode BuildMode;

        public Action OnBuildModeChange;

        private readonly GameManager _gameManager;
        private readonly InputManager _inputManager;

        public BuildManager(GameManager gameManager, InputManager inputManager)
        {
            _gameManager = gameManager;
            _inputManager = inputManager;
            
        }

        public void SetMode(BuildMode activeMode)
        {
            BuildMode = activeMode;
        }

        public void SetBuildMode()
        {
            if (BuildMode == BuildMode.Tile) return;

            SetMode(BuildMode.Tile);
            OnBuildModeChange?.Invoke();
        }

        public BuildMode GetMode()
        {
            return BuildMode;
        }

        public void SetDestroyMode()
        {
            if (BuildMode == BuildMode.Destroy) return;

            SetMode(BuildMode.Destroy);
            OnBuildModeChange?.Invoke();
        }

        public void ClearMode()
        {
            if (BuildMode == BuildMode.None) return;

            SetMode(BuildMode.None);
            OnBuildModeChange?.Invoke();
        }
    }
}