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
        
        public Tile Tile { get; private set; }
        public Tile TargetTile { get; private set; }

        public float MovementPercentage;
        private const float Speed = 5f;

        public Job CurrentJob { get; private set; }
        public World World => Tile.World;

        private TilePath _tilePath;
        private Stack<Tile> _path;

        private float moveTime = 0;

        public Character(Tile tile, string characterType)
        {
            Tile = tile;
            TargetTile = Tile;
            CharacterType = characterType;
        }

        public void Update(float deltaTime)
        {
            WalkToTarget(deltaTime);

            if (CurrentJob == null && Tile == TargetTile) GetJob();
            else DoJob(deltaTime);
        }

        private void WalkToTarget(float deltaTime)
        {
            if (Tile == TargetTile)
            {
                MovementPercentage = 1;
                GetNextTile();
                return;
            }

            MovementPercentage += Speed * deltaTime;
            MovementPercentage = MathHelper.Clamp(MovementPercentage, 0, 1);

            if (MovementPercentage < 1) return;

            Tile = TargetTile;
            GetNextTile();
        }

        private void GetNextTile()
        {
            if (_path == null) return;
            if (_path.Count == 0) return;

            MovementPercentage = 0;
            TargetTile = _path.Pop();
        }

        public void InvalidatePath()
        {
            if (_path == null || _path.Count == 0) return;
            _path = null;

            if (CurrentJob != null)
            {
                CurrentJob.Assigned = false;
                CurrentJob.Assignee = null;
                CurrentJob = null;
            }

            GetJob();
        }

        // TODO Rewrite logic I dont like the tilesize being used here. Should have movementpercentage.

        private void DoJob(float deltaTime)
        {
            if(_path?.Count == 0 && Tile == TargetTile) {
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