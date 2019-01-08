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

        private Tile _targetTile;
        public Tile TargetTile
        {
            get => _targetTile;
            private set
            {
                if (_targetTile == value) return;
                MovementPercentage = 0;
                _targetTile = value;
            }
        }

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
            FindPath();
        }
        
        private void DoJob(float deltaTime)
        {
            if (CurrentJob == null) return;
            if (_path?.Count != 0) return;
            GiveJobHeldItems();

            if (CurrentJob.ItemsRequired() == null)
                CurrentJob.DoWork(deltaTime);
            else ValidateJob();
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
            if (JobStack.Count != 0) return;
            if (CurrentJob != null) return;

            var newJob = World.GetJob(this);
            if (newJob == null) return;

            newJob.Assigned = true;
            newJob.Assignee = this;

            CurrentJob = newJob;
            ValidateJob();
        }

        private void ValidateJob()
        {
            TargetTile = Tile;
            FetchJobItems();
            FindPath();
        }

        private void FetchJobItems()
        {
            var itemsRequired = CurrentJob.ItemsRequired();
            if(ShouldGetFetchJob(itemsRequired))
                GetFetchJob(itemsRequired);
        }

        private void GetFetchJob(ItemRequirements itemsRequired)
        {
            var fetchJob = World.GetFetchJob(itemsRequired);

            if (fetchJob == null)
                return;
            
            StackCurrentJob();
            
            fetchJob.OnJobComplete += OnFetchJobComplete;
            fetchJob.Assigned = true;
            fetchJob.Assignee = this;

            CurrentJob = fetchJob;
        }

        private bool ShouldGetFetchJob(ItemRequirements itemsRequired)
        {
            if (itemsRequired == null)
                return false;

            var totalRequired = itemsRequired.Amount;
            if (totalRequired <= 0) return false;

            if (CarriedItem == null)
                return true;
            if (CarriedItem.Item.Type != itemsRequired.Type)
                return true;

            var carried = CarriedItem.Amount;
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
                CurrentJob.OnJobComplete = null;
                CurrentJob.OnJobCancelled = null;
                CurrentJob.Assigned = false;
                CurrentJob.Assignee = null;
                TargetTile = Tile;
                CurrentJob = null;
            }
        }
        
        private void JobComplete(Job obj)
        {
            if (CurrentJob == obj)
                CurrentJob = null;
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
            // TODO REFACTOR THIS :)
            if (CarriedItem != null && job.FetchItem.Type != CarriedItem.Item.Type) return;

            var requiredAmount = job.FetchItem.Amount;

            if (CarriedItem != null)
                requiredAmount -= CarriedItem.Amount;

            if (job.Tile?.ItemStack == null)
            {
                return;
            }
            
            CurrentJob = null;
            TargetTile = Tile;

            var availableAmount = job.Tile.ItemStack.Amount;
            var takeAmount = MathHelper.Min(requiredAmount, availableAmount);

            job.FetchItem.Amount -= takeAmount;
            job.Tile.ItemStack.Amount -= takeAmount;

            if (job.FetchItem.Amount > 0)
                job.Requeue();

            job.Tile.ItemStack.TotalAllocated -= takeAmount;

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