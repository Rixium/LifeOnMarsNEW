using System;
using System.Collections.Generic;
using LoM.Constants;
using LoM.Game;
using LoM.Game.Build;
using LoM.Game.Job;

namespace LoM.Managers
{
    public class JobManager
    {
        
        private readonly List<Job> _activeJobs = new List<Job>();
        private readonly BuildManager _buildManager;

        public Action OnJobsComplete;

        public JobManager(BuildManager buildManager)
        {
            _buildManager = buildManager;
        }

        public void AddJob(Job job)
        {
            _activeJobs.Add(job);
        }

        public Job GetJob()
        {
            return _activeJobs.Count == 0 ? 
                null : 
                _activeJobs[0];
        }

        public int JobCount()
        {
            return _activeJobs.Count;
        }

        public void CancelTileJob(Tile tile)
        {
            foreach (var job in _activeJobs)
            {
                if (job.Tile != tile) continue;

                job.Cancel();
                _activeJobs.Remove(job);
                return;
            }
        }

        public void CreateDestroyJobs(List<Tile> buildTiles)
        {
            if (buildTiles == null || buildTiles.Count == 0) return;

            foreach (var tile in buildTiles)
            {
                AddJob(new Job
                {
                    JobType = JobType.Destroy,
                    RequiredJobTime = 0.2f,
                    Tile = tile,
                    OnJobComplete = JobComplete
                });
            }
        }

        public void CreateTileJobs(List<Tile> buildTiles)
        {
            if (buildTiles == null || buildTiles.Count == 0) return;

            foreach (var tile in buildTiles)
            {
                AddJob(new Job
                {
                    JobType = JobType.Build,
                    RequiredJobTime = 0.02f,
                    Tile = tile,
                    OnJobComplete = JobComplete
                });
            }
        }


        private void JobComplete(Job job)
        {
            var jobTile = job.Tile;
            _activeJobs.Remove(job);

            if (job.Cancelled) return;

            if (job.JobType == JobType.Build)
                jobTile.SetType(TileType.Ground);
            else if (job.JobType == JobType.Destroy)
                jobTile.SetType(TileType.None);
            else if (job.JobType == JobType.WorldObject)
            {
                var newWorldObject = CreateWorldObject(job);
                job.Tile.PlaceObject(newWorldObject);
            }

            if (_activeJobs.Count == 0)
                OnJobsComplete?.Invoke();
        }

        private WorldObject CreateWorldObject(Job job)
        {
            return new WorldObject(job.Tile, job.ObjectType);
        }

        public void Update(float deltaTime)
        {
            if (_activeJobs.Count == 0) return;

            var currentJob = _activeJobs[0];
            currentJob.DoWork(deltaTime);

            if (currentJob.Cancelled)
                currentJob.OnJobComplete(currentJob);
        }

        public List<Job> GetJobs()
        {
            return _activeJobs;
        }

        public void CreateBuildJobs(List<Tile> buildTiles)
        {
            if (buildTiles == null || buildTiles.Count == 0) return;

            foreach (var tile in buildTiles)
            {
                AddJob(new Job
                {
                    JobType = JobType.WorldObject,
                    ObjectType = _buildManager.BuildObject,
                    RequiredJobTime = 0.2f,
                    Tile = tile,
                    OnJobComplete = JobComplete
                });
            }
        }
    }
}