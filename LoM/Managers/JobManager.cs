using System;
using System.Collections.Generic;
using LoM.Constants;
using LoM.Game;
using LoM.Game.Build;
using LoM.Game.Jobs;

namespace LoM.Managers
{
    public class JobManager
    {
        
        private readonly Dictionary<Character, List<Job>> _unreachables = new Dictionary<Character, List<Job>>();
        private readonly List<Job> _activeJobs = new List<Job>();
        private readonly BuildManager _buildManager;

        public Action OnJobsComplete;
        public Action<Job> OnJobComplete;

        public float JobTime = 2f;

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
            if (_activeJobs.Count == 0) return null;

            foreach (var job in _activeJobs)
            {
                if (job.Assigned) continue;

                return job;
            }

            return null;
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

        public void CreateDestroyJobs(List<Tile> buildTiles, bool isDestroyWorldObject)
        {
            if (buildTiles == null || buildTiles.Count == 0) return;

            var jobType = isDestroyWorldObject ? JobType.DestroyWorldObject : JobType.DestroyTile;

            foreach (var tile in buildTiles)
            {
                AddJob(new Job
                {
                    JobType = jobType,
                    RequiredJobTime = JobTime,
                    Tile = tile,
                    OnJobComplete = JobComplete,
                    OnJobCancelled = JobCancelled
                });
            }
        }

        private void JobCancelled(Job obj)
        {
            _activeJobs.Remove(obj);
        }

        public void CreateTileJobs(List<Tile> buildTiles)
        {
            if (buildTiles == null || buildTiles.Count == 0) return;

            foreach (var tile in buildTiles)
            {
                AddJob(new Job
                {
                    JobType = JobType.Build,
                    RequiredJobTime = JobTime,
                    Tile = tile,
                    OnJobComplete = JobComplete,
                    OnJobCancelled = JobCancelled
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
            else if (job.JobType == JobType.DestroyTile)
                jobTile.SetType(TileType.None);
            else if (job.JobType == JobType.DestroyWorldObject)
                jobTile.RemoveWorldObject();
            else if (job.JobType == JobType.WorldObject)
            {
                var newWorldObject = CreateWorldObject(job);
                job.Tile.PlaceObject(newWorldObject);
            }

            OnJobComplete?.Invoke(job);

            if (_activeJobs.Count == 0)
                OnJobsComplete?.Invoke();
        }

        private static WorldObject CreateWorldObject(Job job)
        {
            if (WorldObjectChest.WorldObjectPrototypes.ContainsKey(job.ObjectName) == false)
            {
                Console.WriteLine($"Prototype with name {job.ObjectName} could not be found.");
                return null;
            }
            var prototype = WorldObjectChest.WorldObjectPrototypes[job.ObjectName];
            return prototype.Place(job.Tile);
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
                    ObjectName = _buildManager.BuildObject,
                    RequiredJobTime = JobTime,
                    Tile = tile,
                    OnJobComplete = JobComplete,
                    OnJobCancelled = JobCancelled
                });
            }
        }

        public void OnTileChanged(Tile tile)
        {
        }

        public Job OnJobRequest(Character character)
        {
            var nearestJob = GetJob();
            if (nearestJob == null) return null;

            var nearestDistance = Math.Abs(character.Tile.X - nearestJob.Tile.X) +
                                  Math.Abs(character.Tile.Y - nearestJob.Tile.Y);
            
            foreach (var job in _activeJobs)
            {
                if (job.Assigned) continue;
                var jobDistance = Math.Abs(character.Tile.X - job.Tile.X) +
                    Math.Abs(character.Tile.Y - job.Tile.Y);

                if (jobDistance > nearestDistance) continue;
                nearestJob = job;
                nearestDistance = jobDistance;
            }
            
            return nearestJob;
        }

    }
}