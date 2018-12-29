﻿using System;

namespace LoM
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

        public void ClearMode()
        {
            if (BuildMode == BuildMode.None) return;

            SetMode(BuildMode.None);
            OnBuildModeChange?.Invoke();
        }
    }
}