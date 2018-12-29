using System;

namespace LoM
{
    public class Job
    {

        public Tile Tile;
        public float JobTime;
        public float RequiredJobTime;

        public bool Cancelled;

        public Action<Job> OnJobComplete;
        public Action<Job> OnJobCancelled;

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