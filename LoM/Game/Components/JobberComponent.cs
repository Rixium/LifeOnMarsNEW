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

        public Func<ItemRequirements, ItemRequirements> CheckRequirements;
        public Func<Job, bool> VerifyJob;
        public Action<Tile> OnNewPathRequest;
        public Action<Job> OnJobWorked;
        public Action<ItemStack> OnTakeItemStack;

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
                if (_activeJob.Cancelled)
                {
                    _activeJob = null;
                    return;
                }
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
            //
            //            if (ActiveJob?.ItemsRequired() == null) return;
            //
            //            AssignedJobs.Push(ActiveJob);
            //            ActiveJob = null;
        }

        // Gets a new job if the stack is empty and sets assigns it pushing it to the stack, ready for next iteration.
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

        // Gets a job from the stack if it isn't empty. If job cannot be verified for some reason, then it will be resolved
        // and likely a new job will be created to resolve it, pushing the old job to the stack.
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
        
        // Creates a fetch job for the user depending on the required items passed.
        // Any buffs can be added through the CheckRequirements callback, which may also
        // check for the characters carry amount.
        private void GetFetchJob(ItemRequirements requirements)
        {
            var actualRequirements = CheckRequirements?.Invoke(requirements);
            var fetchJob = JobManager.GetFetchJob(actualRequirements);

            if (fetchJob?.Tile == null) return;

            fetchJob.Assigned = true;
            fetchJob.Assignee = Character;
            fetchJob.OnJobCancelled += OnJobCancelled;
            fetchJob.OnJobComplete += OnJobComplete;
            fetchJob.OnJobComplete += OnFetchJobComplete;

            AssignedJobs.Push(fetchJob);
        }

        // When a fetch job is complete further logic is required to add the item from the job
        // to the users hand through the callback Pickup item.
        private void OnFetchJobComplete(Job job)
        {
            var jobItemStack = job.Tile.ItemStack;

            var newFetchItem = new FetchRequest
            {
                Allocated = job.FetchItem.Allocated,
                Amount = job.FetchItem.Allocated,
                ItemType = job.FetchItem.ItemType
            };
            
            var itemStack = jobItemStack.Take(newFetchItem);
            OnTakeItemStack?.Invoke(itemStack);
        }

        private void OnJobCancelled(Job job)
        {
            if (ActiveJob == job)
            {
                ActiveJob = null;
            }
        }

        private void OnJobComplete(Job job)
        {
            ActiveJob = null;
        }

        public void UnAssignJob()
        {
            var oldJob = ActiveJob;
            ActiveJob = null;

            if (oldJob != null)
            {
                oldJob.Blacklist.Add(Character);
                oldJob.Assigned = false;
                oldJob.Assignee = null;
                oldJob.OnCannotComplete?.Invoke(oldJob);
            }

            while (AssignedJobs.Count > 0)
            {
                oldJob = AssignedJobs.Pop();
                oldJob.Blacklist.Add(Character);
                oldJob.Assigned = false;
                oldJob.Assignee = null;
                oldJob.OnCannotComplete?.Invoke(oldJob);
            }
        }

    }
    
}