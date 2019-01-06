using System;
using System.Linq;
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
        public JobType JobType { get; set; }

        public WorldObject WorldObject { get; set; }
        public ItemRequirements[] ItemRequirements { get; set; }


        public void DoWork(float deltaTime)
        {
            if (Cancelled) return;
            if (ItemsRequired() != null) return;

            JobTime += deltaTime;

            if (JobTime >= RequiredJobTime)
                OnJobComplete?.Invoke(this);
        }

        private ItemRequirements ItemsRequired()
        {
            return ItemRequirements?.FirstOrDefault(item => item.Amount > 0);
        }

        public void Cancel()
        {
            Cancelled = true;
            OnJobCancelled?.Invoke(this);
        }
    }
}