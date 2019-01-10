using System;
using System.Collections.Generic;
using LoM.Pathfinding;
using Microsoft.Xna.Framework;
using static LoM.Util.Randomizer;

namespace LoM.Game.Components
{
    public class NavigatorComponent : IComponent
    {

        private Stack<Tile> _path;
        public float MovementPercentage = 1.0f;
        public Action OnArrivedAtDestination;
        public Action<float> OnAtTargetTile;
        public Tile TargetTile;

        // For a more human-like navigation we can use next position to set a vector for whatever is using the navigation component,
        // This position can then update the position of the user.
        public Vector2 CurrentPosition;
        public Vector2 NextPosition;

        private Character _character;
        public Character Character
        {
            get => _character;
            set
            {
                _character = value;
                NextPosition = value.Position;
                CurrentPosition = value.Position;
            }
        }

        public void Update(float deltaTime)
        {
            Navigate(deltaTime);
        }

        private void Navigate(float deltaTime)
        {
            CheckTargetTile(deltaTime);
            if (_path == null) return;

            ApplyMovement(deltaTime);
            GetNextTile();

            if (_path.Count > 0 || MovementPercentage < 1) return;
            FinishNavigating();
        }

        private void ApplyMovement(float deltaTime)
        {
            if (TargetTile.WorldObject?.IsPassable == false)
                return;

            var speed = 5f / Character.Tile.MovementCost;
            
            if (TargetTile.Character != null &&
                TargetTile.Character != Character ||
                Character.Tile.Character != Character)
                speed *= 0.2f;

            MovementPercentage += speed * deltaTime;
            MovementPercentage = MathHelper.Clamp(MovementPercentage, 0, 1);
            Character.Position = CurrentPosition - (CurrentPosition - NextPosition) * MovementPercentage;
        }

        private void GetNextTile()
        {
            if (MovementPercentage < 1
                || _path.Count < 1) return;

            MovementPercentage = 0;
            TargetTile = _path.Pop();
            Character.SetTile(TargetTile);
            
            var randomX = NextFloat(TargetTile.X - 0.2f, TargetTile.X + 0.2f);
            var randomY = NextFloat(TargetTile.Y - 0.2f, TargetTile.Y + 0.2f);

            CurrentPosition = new Vector2(NextPosition.X, NextPosition.Y);
            NextPosition = 
                NextFloat(0, 1) < 0.5 ? 
                    new Vector2(randomX, randomY) : 
                    new Vector2(TargetTile.X, TargetTile.Y);
                
        }

        private void CheckTargetTile(float deltaTime)
        {
            if (TargetTile == null) return;
            if (_path == null || _path.Count == 0)
                OnAtTargetTile?.Invoke(deltaTime);
        }

        private void FinishNavigating()
        {
            _path = null;
            OnArrivedAtDestination?.Invoke();
        }

        private void GetPath()
        {
            var newPath = new TilePath(Character.Tile, TargetTile)
                .FindPath(true);
            _path = newPath;

            if (_path == null)
                TargetTile = null;
        }

        public void OnNavigationRequest(Tile targetTile)
        {
            TargetTile = targetTile;
            GetPath();
        }

    }
}