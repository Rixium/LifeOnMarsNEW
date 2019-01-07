using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using LoM.Constants;
using LoM.Game;
using LoM.Game.Build;
using LoM.Game.Jobs;
using LoM.Serialization.Data;

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
                if (job.JobType == JobType.Fetch) continue;

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
                var standOnTile = true;

                if (isDestroyWorldObject)
                    standOnTile = tile.WorldObject?.MovementCost > 0;

                AddJob(new Job
                {
                    JobType = jobType,
                    RequiredJobTime = JobTime,
                    Tile = tile,
                    OnJobComplete = JobComplete,
                    StandOnTile = standOnTile,
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
                    StandOnTile = true,
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
                job.Tile.World.PlaceWorldObject(job.Tile, newWorldObject);
            }

            OnJobComplete?.Invoke(job);

            if (_activeJobs.Count == 0)
                OnJobsComplete?.Invoke();
        }

        private static WorldObject CreateWorldObject(Job job)
        {
            return job.WorldObject?.Place(job.Tile);
        }

        public List<Job> GetJobs()
        {
            return _activeJobs;
        }

        public void CreateBuildJobs(List<Tile> buildTiles)
        {
            if (buildTiles == null || buildTiles.Count == 0) return;

            var worldObjectPrototype = WorldObjectChest.WorldObjectPrototypes[_buildManager.BuildObject];
            
            foreach (var tile in buildTiles)
            {
                var newWorldObject = worldObjectPrototype.Place(tile);
                
                if (newWorldObject.ItemRequirements != null)
                {
                    CreateFetchJobs(newWorldObject.ItemRequirements);
                }

                AddJob(new Job
                {
                    JobType = JobType.WorldObject,
                    WorldObject = worldObjectPrototype,
                    ItemRequirements = newWorldObject.ItemRequirements,
                    RequiredJobTime = JobTime,
                    Tile = tile,
                    StandOnTile = newWorldObject.MovementCost > 0,
                    OnJobComplete = JobComplete,
                    OnJobCancelled = JobCancelled
                });

            }
        }

        private void CreateFetchJobs(ItemRequirements[] itemRequirements)
        {
            if (itemRequirements == null) return;
            foreach (var item in itemRequirements)
            {
                AddJob(new Job
                {
                    JobType = JobType.Fetch,
                    RequiredJobTime = 0,
                    FetchItem = new ItemRequirements
                    {
                        Amount = item.Amount,
                        Type = item.Type
                    },
                    OnJobComplete = JobComplete,
                    OnJobCancelled = JobCancelled,
                    StandOnTile = true,
                    OnRequeueRequest = OnRequeueRequest
                });
            }
        }

        private void OnRequeueRequest(Job job)
        {
            job.OnJobComplete = JobComplete;
            job.OnJobCancelled = JobCancelled;
            job.OnRequeueRequest = OnRequeueRequest;
            AddJob(job);
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
                if (job.Tile == null) continue;

                var jobDistance = Math.Abs(character.Tile.X - job.Tile.X) +
                    Math.Abs(character.Tile.Y - job.Tile.Y);

                if (jobDistance > nearestDistance) continue;
                nearestJob = job;
                nearestDistance = jobDistance;
            }
            
            return nearestJob;
        }

        public Job OnFetchJobRequest(ItemRequirements item)
        {
            if (_activeJobs.Count == 0) return null;

            foreach (var job in _activeJobs)
            {
                if (job.JobType != JobType.Fetch) continue;
                if (job.Assigned) continue;
                if (job.FetchItem.Type != item.Type) continue;
                return job;
            }

            return null;
        }
    }
}