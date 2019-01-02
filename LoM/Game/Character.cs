using System;
using System.Collections.Generic;
using LoM.Game.Jobs;
using LoM.Pathfinding;
using Microsoft.Xna.Framework;

namespace LoM.Game
{
    public class Character
    {

        public static Random Random = new Random();

        public string CharacterType;

        public float X;
        public float Y;

        public Tile Tile { get; private set; }
        private Tile _targetTile;

        public Job CurrentJob { get; private set; }
        public World World => Tile.World;

        private TilePath _tilePath;
        private Stack<Tile> _path;

        private float moveTime = 0;

        public Character(Tile tile, string characterType)
        {
            Tile = tile;
            X = tile.X * 32;
            Y = tile.Y * 32;

            _targetTile = Tile;
            CharacterType = characterType;
        }

        public void Update(float deltaTime)
        {
            WalkToTarget(deltaTime);

            if (CurrentJob == null && Tile == _targetTile) GetJob();
            else DoJob(deltaTime);
        }

        private void WalkToTarget(float deltaTime)
        {
            var direction = new Vector2(_targetTile.X - Tile.X, _targetTile.Y - Tile.Y);
            direction.X = MathHelper.Clamp(direction.X, -1, 1);
            direction.Y = MathHelper.Clamp(direction.Y, -1, 1);


            if (Math.Abs(X - _targetTile.X * 32) < 5f && Math.Abs(Y - _targetTile.Y * 32) < 5f)
            {
                Tile = _targetTile;

                if (_path?.Count > 0)
                    _targetTile = _path.Pop();
                
                return;
            }

            X += direction.X * 50 * deltaTime;
            Y += direction.Y * 50 * deltaTime;
        }


        // TODO Rewrite logic I dont like the tilesize being used here. Should have movementpercentage.

        private void DoJob(float deltaTime)
        {
            if(_path?.Count == 0 && Tile == _targetTile) {
                CurrentJob.DoWork(deltaTime);
            }
        }

        private void GetJob()
        {
            CurrentJob = World.GetJob(this);
            if (CurrentJob == null) return;

            _tilePath = new TilePath(Tile, CurrentJob.Tile, World);
            var path = _tilePath.FindPath();

            if (path != null) { 
                _path = path;
                CurrentJob.Assigned = true;
                CurrentJob.Assignee = this;
                CurrentJob.OnJobComplete += JobComplete;
                CurrentJob.OnJobCancelled += JobComplete;
                return;
            }
            
            CurrentJob = null;
        }

        private void JobComplete(Job obj)
        {
            if (CurrentJob == obj) CurrentJob = null;
        }
    }
}