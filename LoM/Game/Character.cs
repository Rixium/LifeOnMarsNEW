using System.Collections.Generic;
using LoM.Game.Jobs;
using LoM.Pathfinding;
using Microsoft.Xna.Framework;

namespace LoM.Game
{
    public class Character
    {
        private float _jobGetCoolDown;
        private Stack<Tile> _path;
        private bool _pathfinding;

        private TilePath _tilePath;

        public string CharacterType;

        public float MovementPercentage;

        public float Speed = 10f;

        public Character(Tile tile, string characterType)
        {
            Tile = tile;
            TargetTile = Tile;
            CharacterType = characterType;
        }

        public Tile Tile { get; private set; }
        public Tile TargetTile { get; private set; }

        public Job CurrentJob { get; private set; }
        public World World => Tile.World;

        public void Update(float deltaTime)
        {
            WalkToTarget(deltaTime);

            if (CurrentJob == null && Tile == TargetTile) GetJob(deltaTime);
            else DoJob(deltaTime);
        }

        /// <summary>
        ///     If the character has a tile that isn't the target tile then they can progress towards the tile using movement
        ///     percentage.
        ///     Movement percentage can then be used later to calculate the position to render the character.
        /// </summary>
        private void WalkToTarget(float deltaTime)
        {
            if (Tile == TargetTile)
            {
                MovementPercentage = 1;
                GetNextTile();
                return;
            }
            
            if(TargetTile.WorldObject != null)
                if (TargetTile.WorldObject.IsPassable == false)
                    return;

            MovementPercentage += Speed * deltaTime;
            MovementPercentage = MathHelper.Clamp(MovementPercentage, 0, 1);

            if (MovementPercentage < 1) return;

            Tile = TargetTile;
            GetNextTile();
        }

        /// <summary>
        ///     If the player has a path, and they're currently not moving, then they can get the next tile in the path
        ///     ready to move there on the next update.
        /// </summary>
        private void GetNextTile()
        {
            if (_path == null) return;
            if (_path.Count == 0) return;

            MovementPercentage = 0;
            TargetTile = _path.Pop();
        }

        /// <summary>
        ///     Invalidates the path that the character is currently following. This is usually called when the character is
        ///     travelling to a job tile, and another tile has changed that means the
        ///     characters path could be invalid. (Such as a wall being built, and the character can no longer traverse the wall
        ///     tile, therefore needs to recalculate the path.
        /// </summary>
        public void InvalidatePath()
        {
            if (_path == null || _path.Count == 0) return;
            _path = null;

            if (CurrentJob == null) return;

            CurrentJob.Assigned = false;
            CurrentJob.Assignee = null;
            CurrentJob = null;
        }

        /// <summary>
        ///     If the character has a job, and they are at the job tile, then the character will do work on the job.
        /// </summary>
        private void DoJob(float deltaTime)
        {
            if (CurrentJob == null) return;
            if (_path?.Count == 0) CurrentJob.DoWork(deltaTime);
            if (CurrentJob?.JobTime >= CurrentJob?.RequiredJobTime &&
                CurrentJob?.Tile == Tile) ClearJob();
        }

        private void ClearJob()
        {
            CurrentJob = null;
            _path = null;
        }

        /// <summary>
        ///     The character will request a job from the world and try to get a path to the job.
        ///     If the character cant find a path, then they will ignore the job.
        /// </summary>
        private void GetJob(float deltaTime)
        {
            if (_pathfinding) return;
            _jobGetCoolDown -= deltaTime;

            if (_jobGetCoolDown > 0) return;
            CurrentJob = World.GetJob(this);
            if (CurrentJob == null) return;

            CurrentJob.Assigned = true;
            CurrentJob.Assignee = this;
            TargetTile = Tile;

            _pathfinding = true;
            FindPath();
        }

        private void FindPath()
        {
            _tilePath = new TilePath(Tile, CurrentJob.Tile);
            var path = _tilePath.FindPath(CurrentJob.JobType == JobType.Move);

            if (path != null)
            {
                _path = path;
                CurrentJob.OnJobComplete += JobComplete;
                CurrentJob.OnJobCancelled += JobComplete;
            }
            else
            {
                _path = null;
                CurrentJob.Assigned = false;
                CurrentJob.Assignee = null;
                TargetTile = Tile;
                _jobGetCoolDown = 2.0f;
                CurrentJob = null;
            }

            _pathfinding = false;
        }

        /// <summary>
        ///     If the job that the character is currently assigned to is either completed or cancelled, then we set the job to
        ///     null here, so the character can later request a new job.
        /// </summary>
        private void JobComplete(Job obj)
        {
            if (CurrentJob == obj) CurrentJob = null;
        }

        public void SetJob(Job job)
        {
            CurrentJob = null;
            _path = null;
            TargetTile = Tile;
            CurrentJob = job;
            FindPath();
        }

    }
}