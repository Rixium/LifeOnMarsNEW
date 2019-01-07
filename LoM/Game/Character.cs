using System.Collections.Generic;
using LoM.Game.Items;
using LoM.Game.Jobs;
using LoM.Pathfinding;
using LoM.Serialization.Data;
using Microsoft.Xna.Framework;

namespace LoM.Game
{
    public class Character
    {
        private float _jobGetCoolDown;
        private Stack<Tile> _path;

        private TilePath _tilePath;

        public ItemStack CarriedItem;

        public string CharacterType;

        public float MovementPercentage;

        public float Speed = 5f;

        public Character(Tile tile, string characterType)
        {
            Tile = tile;
            TargetTile = Tile;
            CharacterType = characterType;
        }

        public Tile Tile { get; private set; }
        public Tile TargetTile { get; private set; }

        public Job CurrentJob { get; private set; }
        public Stack<Job> JobStack { get; } = new Stack<Job>();

        public World World => Tile.World;

        public void Update(float deltaTime)
        {
            WalkToTarget(deltaTime);

            switch (CurrentJob)
            {
                case null when Tile == TargetTile && JobStack.Count == 0:
                    GetJob();
                    break;
                case null when Tile == TargetTile && JobStack.Count > 0:
                    NextJob();
                    break;
                default:
                    DoJob(deltaTime);
                    break;
            }
        }

        private void NextJob()
        {
            var nextJob = JobStack.Pop();

            if (nextJob.Cancelled) return;
            CurrentJob = nextJob;

            ValidateJob();
        }
        
        private void WalkToTarget(float deltaTime)
        {
            if (Tile == TargetTile)
            {
                MovementPercentage = 1;
                GetNextTile();
                return;
            }

            if (TargetTile.WorldObject != null)
                if (TargetTile.WorldObject.IsPassable == false)
                    return;

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

            if (CurrentJob == null) return;

            CurrentJob.Assigned = false;
            CurrentJob.Assignee = null;
            CurrentJob = null;
        }
        
        private void DoJob(float deltaTime)
        {
            if (CurrentJob == null) return;
            if (_path?.Count != 0) return;

            GiveJobHeldItems();
            CurrentJob.DoWork(deltaTime);
        }

        private void GiveJobHeldItems()
        {
            if (CarriedItem == null) return;
            CurrentJob.AllocateItem(CarriedItem);
            if (CarriedItem.Amount <= 0)
                CarriedItem = null;
        }
        
        private void GetJob()
        {
            CurrentJob = World.GetJob(this);
            if (CurrentJob == null) return;

            CurrentJob.Assigned = true;
            CurrentJob.Assignee = this;

            ValidateJob();
        }

        private void ValidateJob()
        {
            if (FetchJobItems())
                return;

            TargetTile = Tile;
            FindPath();
        }

        private bool FetchJobItems()
        {
            var itemsRequired = CurrentJob.ItemsRequired();
            return ShouldGetFetchJob(itemsRequired) && 
                   GetFetchJob(itemsRequired);
        }

        private bool GetFetchJob(ItemRequirements itemsRequired)
        {
            var fetchJob = World.GetFetchJob(itemsRequired);

            if (fetchJob == null)
                return false;

            StackCurrentJob();
            fetchJob.Tile = World.FindItemTile(fetchJob.FetchItem);
            fetchJob.OnJobComplete += OnFetchJobComplete;
            JobStack.Push(fetchJob);
            return true;
        }

        private bool ShouldGetFetchJob(ItemRequirements itemsRequired)
        {
            if (itemsRequired == null)
                return false;
            if (CarriedItem == null)
                return true;
            if (CarriedItem.Item.Type != itemsRequired.Type)
                return true;

            var carried = CarriedItem.Amount;
            var totalRequired = itemsRequired.Amount;
            return totalRequired - carried > 0;
        }

        private void StackCurrentJob()
        {
            JobStack.Push(CurrentJob);
            CurrentJob = null;
        }

        private void FindPath()
        {
            _tilePath = new TilePath(Tile, CurrentJob.Tile);
            var path = _tilePath.FindPath(CurrentJob.StandOnTile);

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
        }
        
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

        public void OnFetchJobComplete(Job job)
        {
            if (CarriedItem != null && job.FetchItem.Type != CarriedItem.Item.Type) return;

            var requiredAmount = job.FetchItem.Amount;

            if (CarriedItem != null)
                requiredAmount -= CarriedItem.Amount;

            if (job.Tile?.ItemStack == null)
            {
                CurrentJob = null;
                TargetTile = Tile;
                return;
            }

            var availableAmount = job.Tile.ItemStack.Amount;
            var takeAmount = MathHelper.Min(requiredAmount, availableAmount);
            job.Tile.ItemStack.Amount -= takeAmount;

            if (job.FetchItem.Amount > 0) job.Requeue();

            World.OnItemStackChanged(job.Tile.ItemStack);

            if (CarriedItem == null)
                CarriedItem = new ItemStack(new Item
                {
                    Type = job.FetchItem.Type
                })
                {
                    Amount = takeAmount
                };
            else
                CarriedItem.Amount += takeAmount;
        }
    }
}