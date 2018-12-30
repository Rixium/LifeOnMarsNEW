using System;
using LoM.Constants;

namespace LoM.Game.Job
{
    public class Job
    {

        public Tile Tile;
        public float JobTime;
        public float RequiredJobTime;

        public bool Cancelled;

        public Action<Job> OnJobComplete;
        public Action<Job> OnJobCancelled;
        public JobType JobType { get; set; }
        public ObjectType ObjectType { get; set; }

        public void DoWork(float deltaTime)
        {
            if (Cancelled) return;

            JobTime += deltaTime;

            if (JobTime >= RequiredJobTime)
                OnJobComplete?.Invoke(this);
        }

        public void Cancel()
        {
            Cancelled = true;
            OnJobCancelled?.Invoke(this);
        }

    }
}