using System;

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
        public string ObjectName { get; set; }

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