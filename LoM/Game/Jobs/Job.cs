using System;
using System.Linq;
using LoM.Game.Items;
using LoM.Managers;
using LoM.Serialization.Data;

namespace LoM.Game.Jobs
{
    public class Job
    {
        public bool Assigned;
        public Character Assignee;

        public bool Cancelled;
        public float JobTime;
        public Action<Job> OnJobCancelled;

        public Action<Job> OnJobComplete;
        public float RequiredJobTime;

        public Tile Tile;
        public bool StandOnTile;
        public JobType JobType { get; set; }

        public WorldObject WorldObject { get; set; }
        public ItemRequirements[] ItemRequirements { get; set; }
        public ItemRequirements FetchItem { get; set; }

        public ItemRequirements CacheItems;


        public Action<Job> OnRequeueRequest;

        public void DoWork(float deltaTime)
        {
            if (Cancelled) return;
            if (ItemsRequired() != null) return;

            JobTime += deltaTime;

            if (JobTime >= RequiredJobTime)
                OnJobComplete?.Invoke(this);
        }

        public ItemRequirements ItemsRequired()
        {
            return CacheItems ?? (CacheItems = ItemRequirements?.FirstOrDefault(item => item.Amount > 0));
        }

        public void Cancel()
        {
            Cancelled = true;
            OnJobCancelled?.Invoke(this);
        }

        public void AllocateItem(ItemStack allocateItem)
        {
            if (ItemRequirements == null) return;

            if (CacheItems != null && CacheItems.Type == allocateItem.Item.Type)
            {
                var takeAmount = Math.Min(allocateItem.Amount, CacheItems.Amount);
                CacheItems.Amount -= takeAmount;
                allocateItem.Amount -= takeAmount;

                if (CacheItems.Amount <= 0)
                    CacheItems = null;

                return;
            }

            foreach (var item in ItemRequirements)
            {
                if (item.Type != allocateItem.Item.Type) continue;
                var takeAmount = Math.Min(allocateItem.Amount, item.Amount);
                item.Amount -= takeAmount;
                allocateItem.Amount -= takeAmount;
            }
        }

        public void Requeue()
        {
            OnRequeueRequest?.Invoke(new Job
            {
                FetchItem = new ItemRequirements
                {
                    Type = FetchItem.Type,
                    Amount = FetchItem.Amount
                },
                StandOnTile = StandOnTile,
                JobType = JobType
            });
        }

    }
}