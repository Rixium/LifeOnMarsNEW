using System;
using System.Collections.Generic;
using System.Linq;
using LoM.Game.Items;
using LoM.Managers;
using LoM.Serialization.Data;

namespace LoM.Game.Jobs
{
    public class Job
    {

        public HashSet<Character> Blacklist = new HashSet<Character>();

        public bool Assigned;
        public Character Assignee;

        public bool Cancelled;
        public float JobTime;
        public float RequiredJobTime;

        public Action<Job> OnJobCancelled;
        public Action<Job> OnJobComplete;
        public Action<Job> OnRequeueRequest;

        public Tile Tile;
        public bool StandOnTile;
        public JobType JobType { get; set; }

        public WorldObject WorldObject { get; set; }

        public ItemRequirements[] ItemRequirements { get; set; }
        public ItemRequirements CacheItems;

        public FetchRequest FetchItem { get; set; }
        public Action<Job> OnCannotComplete { get; set; }

        public void DoWork(float deltaTime)
        {
            if (Cancelled) return;
            if (ItemsRequired() != null) return;

            JobTime += deltaTime;

            if ((JobTime < RequiredJobTime)) return;
            
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

            foreach (var requirement in ItemRequirements)
            {
                if (requirement.Type != allocateItem.Item.Type) continue;
                var takeAmount = Math.Min(requirement.Amount, allocateItem.Amount);
                requirement.Amount -= takeAmount;
                allocateItem.Amount -= takeAmount;
            }

            CacheItems = null;
        }

        public void Requeue()
        {
            OnRequeueRequest?.Invoke(new Job
            {
                FetchItem = new FetchRequest
                {
                    ItemType = FetchItem.ItemType,
                    Amount = FetchItem.Amount
                },
                StandOnTile = StandOnTile,
                JobType = JobType
            });
        }

    }
}