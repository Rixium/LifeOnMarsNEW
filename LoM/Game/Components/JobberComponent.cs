using System;
using System.Collections.Generic;
using LoM.Game.Items;
using LoM.Game.Jobs;
using LoM.Managers;
using LoM.Serialization.Data;

namespace LoM.Game.Components
{
    public class JobberComponent : IComponent
    {

        public Func<Job, bool> VerifyJob;
        public Action<Tile> OnNewPathRequest;
        public Action<Job> OnJobWorked;

        public Character Character { get; set; }
        public JobManager JobManager;

        public JobberComponent(JobManager jobManager)
        {
            JobManager = jobManager;
        }

        private Job _activeJob;

        public Job ActiveJob
        {
            get => _activeJob;
            set
            {
                _activeJob = value;
                if (_activeJob == null) return;
                OnNewPathRequest?.Invoke(_activeJob.Tile);
            }
        }

        public Stack<Job> AssignedJobs = new Stack<Job>();

        public void Update(float deltaTime)
        {
            if (ActiveJob == null)
                GetJob();
        }

        public void DoJob(float deltaTime)
        {
            if (ActiveJob == null) return;
            OnJobWorked?.Invoke(ActiveJob);
            ActiveJob?.DoWork(deltaTime);

            if (ActiveJob?.ItemsRequired() == null) return;

            AssignedJobs.Push(ActiveJob);
            ActiveJob = null;
        }

        private void GetJob()
        {
            if (AssignedJobs.Count > 0)
            {
                GetJobFromStack();
                return;
            }

            var newJob = JobManager.RequestJob(Character);
            if (newJob == null) return;

            newJob.Assigned = true;
            newJob.Assignee = Character;
            newJob.OnJobCancelled += OnJobCancelled;
            newJob.OnJobComplete += OnJobComplete;
            AssignedJobs.Push(newJob);
        }

        private void GetJobFromStack()
        {
            var jobCheck = AssignedJobs.Peek();
            var canDo = VerifyJob?.Invoke(jobCheck);

            if (canDo == null || canDo.Value)
            {
                ActiveJob = AssignedJobs.Pop();
                return;
            }

            GetFetchJob(jobCheck.ItemsRequired());
        }
        
        private void GetFetchJob(ItemRequirements requirements)
        {
            var fetchJob = JobManager.GetFetchJob(requirements);
            if (fetchJob?.Tile == null) return;

            fetchJob.Assigned = true;
            fetchJob.Assignee = Character;
            fetchJob.OnJobCancelled += OnJobCancelled;
            fetchJob.OnJobComplete += OnJobComplete;
            fetchJob.OnJobComplete += OnFetchJobComplete;

            AssignedJobs.Push(fetchJob);
        }

        private void OnFetchJobComplete(Job job)
        {
            var jobItemStack = job.Tile.ItemStack;

            var newFetchItem = new FetchRequest
            {
                Allocated = job.FetchItem.Allocated,
                Amount = job.FetchItem.Allocated,
                ItemType = job.FetchItem.ItemType
            };

            if (Character.CarriedItem != null)
            {
                newFetchItem.Amount -= Character.CarriedItem.Amount;
                newFetchItem.Allocated -= Character.CarriedItem.Amount;
            }
            
            var itemStack = jobItemStack.Take(newFetchItem);

            if (Character.CarriedItem == null)
                Character.CarriedItem = itemStack;
            else Character.CarriedItem.Amount += itemStack.Amount;
            
        }

        private void OnJobCancelled(Job obj)
        {
            
        }

        private void OnJobComplete(Job job)
        {
            ActiveJob = null;
        }

    }
    
}