using System;
using System.Collections.Generic;
using System.Security.Policy;
using LoM.Pathfinding;
using Microsoft.Xna.Framework;

namespace LoM.Game.Components
{

    public class NavigatorComponent : IComponent
    {

        private Stack<Tile> _path;
        public Tile TargetTile;
        public float MovementPercentage;

        public Character Character { get; set; }
        public Action OnArrivedAtDestination;
        public Action<float> OnAtTargetTile;

        public void Update(float deltaTime)
        {
            Navigate(deltaTime);
        }

        private void Navigate(float deltaTime)
        {
            CheckTargetTile(deltaTime);
            if (_path == null) return;

            TargetTile = _path.Pop();
            Character.SetTile(TargetTile);

            if (_path.Count > 0) return;
            FinishNavigating();
        }

        private void CheckTargetTile(float deltaTime)
        {
            if(_path == null || _path.Count == 0)
                OnAtTargetTile?.Invoke(deltaTime);
        }

        private void FinishNavigating()
        {
            _path = null;
            TargetTile = null;
            OnArrivedAtDestination?.Invoke();
        }

        private void GetPath()
        {
            var newPath = new TilePath(Character.Tile, TargetTile)
                .FindPath(true);
            if (newPath == null) return;
            _path = newPath;
        }

        public void OnNavigationRequest(Tile targetTile)
        {
            TargetTile = targetTile;
            GetPath();
        }

    }

}