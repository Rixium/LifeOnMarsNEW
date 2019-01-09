using System;
using System.Collections.Generic;
using LoM.Game.Jobs;
using LoM.Managers;

namespace LoM.Game.Components
{
    public class JobberComponent : IComponent
    {

        public Character Character { get; set; }
        public JobManager JobManager;
        public Action<Tile> OnNewPathRequest;

        public JobberComponent(JobManager jobManager)
        {
            JobManager = jobManager;
        }

        public Job ActiveJob;
        public Stack<Job> AssignedJobs;

        public void Update(float deltaTime)
        {
            if (ActiveJob == null)
                GetJob();
        }

        public void DoJob(float deltaTime)
        {
            ActiveJob?.DoWork(deltaTime);
        }

        private void GetJob()
        {
            var newJob = JobManager.RequestJob(Character);
            if (newJob == null) return;
            ActiveJob = newJob;
            ActiveJob.Assigned = true;
            ActiveJob.Assignee = Character;
            ActiveJob.OnJobComplete += OnJobComplete;
            OnNewPathRequest?.Invoke(ActiveJob.Tile);
        }

        private void OnJobComplete(Job job)
        {
            ActiveJob = null;
        }

    }
}