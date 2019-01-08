using System;
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
        private Random random = new Random();
        public Vector2 CurrVector;
        public Vector2 PathVector = Vector2.Zero;

        private TilePath _tilePath;

        public ItemStack CarriedItem;

        public string CharacterType;

        public float MovementPercentage;

        public float Speed = 5f;

        public Character(Tile tile, string characterType)
        {
            Tile = tile;
            Tile.Character = this;
            CharacterType = characterType;
            CurrVector = new Vector2(Tile.X * 32, Tile.Y * 32);
            PathVector = new Vector2(Tile.X * 32, Tile.Y * 32);
            _targetTile = Tile;
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
                SetVectors();
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


            var slowDownSpeed = 1f;

            if (TargetTile.Character != null &&
                TargetTile.Character != this)
                slowDownSpeed = 0.2f;

            // TODO Move percentage equipment
            MovementPercentage += (Speed * slowDownSpeed / Tile.MovementCost) * deltaTime;
            MovementPercentage = MathHelper.Clamp(MovementPercentage, 0, 1);

            if (MovementPercentage < 1) return;

            Tile.Character = null;
            Tile = TargetTile;
            Tile.Character = this;

            GetNextTile();
        }

        static float NextFloat(Random random, int max, int min)
        {
            double range = (double)max - (double)min;
            double sample = random.NextDouble();
            double scaled = (sample * range) + min;
            float f = (float)scaled;

            return f;
        }

        private void GetNextTile()
        {
            if (_path == null) return;
            if (_path.Count == 0) return;

            MovementPercentage = 0;
            TargetTile = _path.Pop();
        }

        private void SetVectors()
        {
            CurrVector = new Vector2(PathVector.X, PathVector.Y);

            if (random.Next(0, 50) < 30)
            {
                PathVector = new Vector2
                {
                    X = NextFloat(random, TargetTile.X * 32 - 12, TargetTile.X * 32 + 12),
                    Y = NextFloat(random, TargetTile.Y * 32 - 12, TargetTile.Y * 32 + 12)
                };
                return;
            }

            PathVector = new Vector2
            {
                X = TargetTile.X * 32,
                Y = TargetTile.Y * 32
            };
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

            if (!FindPath()) return;
            FetchJobItems();
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

            var requiredAmount = CarriedItem == null || CarriedItem.Item.Type != itemsRequired.Type ?
                itemsRequired.Amount : itemsRequired.Amount - CarriedItem.Amount;

            var allocationAmount = Math.Min(requiredAmount, fetchJob.Tile.ItemStack.Amount - fetchJob.Tile.ItemStack.TotalAllocated);
            fetchJob.Tile.ItemStack.AddAllocation(allocationAmount);
            
            StackCurrentJob();
            
            fetchJob.OnJobComplete += OnFetchJobComplete;
            fetchJob.Assigned = true;
            fetchJob.Assignee = this;

            CurrentJob = fetchJob;

            // Cannot get to items, but can get to the job. So need to remove access from both or will freeze character and job.
            if (FindPath()) return;
            JobStack.Push(fetchJob);
            DestroyAllJobs();
        }

        private void DestroyAllJobs()
        {
            while (JobStack.Count > 0)
            {
                var oldJob = JobStack.Pop();
                oldJob.Assigned = false;
                oldJob.Assignee = null;
                ClearJob();

                if (oldJob.JobType != JobType.Fetch) continue;
                oldJob.Tile.ItemStack.TotalAllocated = 0;
                oldJob.Requeue();
                ClearJob();
            }
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

        private bool FindPath()
        {
            _tilePath = new TilePath(Tile, CurrentJob.Tile);
            var path = _tilePath.FindPath(CurrentJob.StandOnTile || CurrentJob.JobType == JobType.Move);

            if (path != null)
            {
                _path = path;
                CurrentJob.OnJobComplete += JobComplete;
                CurrentJob.OnJobCancelled += JobComplete;
            }
            else
            {
                _path = null;
                ClearJob();
                return false;
            }

            return true;
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
        
        // Specific to fetch jobs. if one completes then the item assigned to the job will be given to character to carry.
        public void OnFetchJobComplete(Job job)
        {
            if (CarriedItem != null 
                && job.FetchItem.Type != CarriedItem.Item.Type ||
                job.Tile?.ItemStack == null) return;

            var requiredAmount = CarriedItem == null ? 
                job.FetchItem.Amount : job.FetchItem.Amount - CarriedItem.Amount;

            ClearJob();
            
            var takeAmount = MathHelper.Min(requiredAmount, job.Tile.ItemStack.Amount);
            
            if (takeAmount < 0) takeAmount = 0;

            PickupItem(job.Tile.ItemStack, takeAmount);

            if (job.FetchItem.Amount - CarriedItem.Amount > 0)
                job.Requeue();

            if (takeAmount > 0)
                World.OnItemStackChanged(job.Tile.ItemStack);
        }

        // Pass an item stack to the player and the amount given will be added to the player if possible.
        private void PickupItem(ItemStack stack, int amount)
        {
            if (stack == null) return;
            if (stack.Amount <= 0) return;
            if (amount == 0) return;

            var item = stack.Item;
            if (item == null) return;

            if (CarriedItem != null &&
                CarriedItem.Item?.Type == item?.Type)
                CarriedItem.Amount += amount;
            else
                CarriedItem = new ItemStack(item, amount);
            
            stack.Amount -= amount;
            stack.TotalAllocated = 0;
        }

        // Removes all data from a job and un-assigns it.
        private void ClearJob()
        {
            if (CurrentJob == null) return;

            CurrentJob.Assigned = false;
            CurrentJob.Assignee = null;

            CurrentJob.OnJobCancelled -= JobComplete;
            CurrentJob.OnJobComplete -= JobComplete;
            CurrentJob.OnJobComplete -= OnFetchJobComplete;

            if (CurrentJob.JobType == JobType.Fetch &&
                CurrentJob?.Tile?.ItemStack != null)
                CurrentJob.Tile.ItemStack.TotalAllocated = 0;

            TargetTile = Tile;
            CurrentJob = null;
        }

    }
}