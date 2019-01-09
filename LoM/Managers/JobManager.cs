using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using LoM.Constants;
using LoM.Game;
using LoM.Game.Build;
using LoM.Game.Jobs;
using LoM.Serialization.Data;
using Microsoft.Xna.Framework;

namespace LoM.Managers
{
    public class JobManager
    {

        private ItemManager _itemManager;
        private readonly Dictionary<Character, List<Job>> _unreachables = new Dictionary<Character, List<Job>>();
        private readonly HashSet<Job> _activeJobs = new HashSet<Job>();
        private readonly BuildManager _buildManager;

        public Action OnJobsComplete;
        public Action<Job> OnJobComplete;

        public float JobTime = 2f;

        public JobManager(BuildManager buildManager, ItemManager itemManager)
        {
            _buildManager = buildManager;
            _itemManager = itemManager;
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
                if (job.JobType == JobType.Fetch) continue;

                job.Cancel();
                _activeJobs.Remove(job);
                return;
            }
        }

        public void CreateDestroyJobs(List<BuildRequest> buildTiles, bool isDestroyWorldObject)
        {
            if (buildTiles == null || buildTiles.Count == 0) return;

            var jobType = isDestroyWorldObject ? JobType.DestroyWorldObject : JobType.DestroyTile;

            foreach (var buildRequest in buildTiles)
            {
                var tile = buildRequest.BuildTile;
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

        public void CreateTileJobs(List<BuildRequest> buildTiles)
        {
            if (buildTiles == null || buildTiles.Count == 0) return;

            foreach (var buildRequest in buildTiles)
            {
                var tile = buildRequest.BuildTile;
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

        public HashSet<Job> GetJobs()
        {
            return _activeJobs;
        }

        public void CreateBuildJobs(List<BuildRequest> buildTiles)
        {
            if (buildTiles == null || buildTiles.Count == 0) return;

            var worldObjectPrototype = WorldObjectChest.WorldObjectPrototypes[_buildManager.BuildObject];
            
            
            foreach (var buildRequest in buildTiles)
            {
                if (buildRequest.BuildFloor)
                {
                    CreateTileJobs(new List<BuildRequest>()
                    {
                        buildRequest
                    });
                    continue;
                }
                
                var tile = buildRequest.BuildTile;

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
                var job = new Job
                {
                    JobType = JobType.Fetch,
                    RequiredJobTime = 0,
                    FetchItem = new ItemRequirements
                    {
                        Amount = item.Amount,
                        Type = item.Type
                    },
                    OnJobComplete = JobComplete,
                    OnJobCancelled = OnFetchJobCancel,
                    StandOnTile = true,
                    OnRequeueRequest = OnRequeueRequest
                };

                job.OnJobCancelled += JobCancelled;
                AddJob(job);
            }
        }

        private void OnRequeueRequest(Job job)
        {
            job.OnJobComplete += JobComplete;
            job.OnJobCancelled += JobCancelled;
            job.OnRequeueRequest += OnRequeueRequest;
            AddJob(job);
        }

        public void OnTileChanged(Tile tile)
        {
        }

        public Job RequestJob(Character character)
        {
            var nearestJob = GetJob();
            if (nearestJob == null) return null;

            var nearestDistance = Math.Abs(character.Tile.X - nearestJob.Tile.X) +
                                  Math.Abs(character.Tile.Y - nearestJob.Tile.Y);
            
            foreach (var job in _activeJobs)
            {
                if (job.Assigned) continue;
                if (job.JobType == JobType.Fetch) continue;
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
            if (item == null) return null;
            if (_activeJobs.Count == 0) return null;

            foreach (var job in _activeJobs)
            {
                if (job.JobType != JobType.Fetch) continue;
                if (job.Assigned || job.Assignee != null) continue;
                if (job.FetchItem.Type != item.Type) continue;
                var itemTile = _itemManager.FindItem(item);
                if (itemTile == null) return null;
                job.Tile = itemTile;
                return job;
            }
            
            CreateFetchJobs(new []{ item });
            return OnFetchJobRequest(item);
        }

        public void OnFetchJobCancel(Job job)
        {
            job.Assigned = false;
            job.Assignee = null;
            if (job.Tile?.ItemStack == null) return;
            job.Tile.ItemStack.TotalAllocated = 0;
        }
    }
}