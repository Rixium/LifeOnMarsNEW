using System.Collections.Generic;
using LoM.Game;
using LoM.Game.Jobs;

namespace LoM.Managers
{
    public class RegionManager
    {
        
        private readonly List<Region> _regions = new List<Region>();

        private readonly GameManager _gameManager;
        private World World => _gameManager.World;

        public RegionManager(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public Region FloodFillFrom(Tile tile, Tile from)
        {
            var regionTiles = new List<Tile>();
            var newRegion = new Region(regionTiles);
            _regions.Add(newRegion);

            var isEnclosed = true;

            if(tile.Region != null)
                _regions.Remove(tile.Region);

            tile.Region = newRegion;
            
            var queue = new Queue<Tile>();
            queue.Enqueue(tile);

            while (queue.Count > 0)
            {
                var currentTile = queue.Dequeue();
                currentTile.Region = newRegion;
                regionTiles.Add(currentTile);

                var north = currentTile.North();
                var east = currentTile.East();
                var south = currentTile.South();
                var west = currentTile.West();

                if (north?.Type == TileType.None) isEnclosed = false;
                if (east?.Type == TileType.None) isEnclosed = false;
                if (south?.Type == TileType.None) isEnclosed = false;
                if (west?.Type == TileType.None) isEnclosed = false;

                if (ShouldQueue(north, newRegion)) queue.Enqueue(north);
                if (ShouldQueue(east, newRegion)) queue.Enqueue(east);
                if (ShouldQueue(south, newRegion)) queue.Enqueue(south);
                if (ShouldQueue(west, newRegion)) queue.Enqueue(west);
            }

            newRegion.SpaceSafe = isEnclosed;

            return newRegion;
        }

        private static bool ShouldQueue(Tile tile, Region newRegion)
        {
            if (tile == null) return false;
            if (tile.MovementCost == 0)
            {
                tile.Region = newRegion;
                return false;
            }
            if (tile.Region == newRegion) return false;

            tile.Region = newRegion;
            return true;
        }

        public int GetRegionIndexOfTile(Tile tile)
        {
            return _regions.IndexOf(tile.Region);
        }

        public void OnJobComplete(Job job)
        {
            if (job.JobType != JobType.DestroyWorldObject &&
                job.JobType != JobType.WorldObject) return;

            CreateRegions(job.Tile);
        }

        private void CreateRegions(Tile tile)
        {
            var north = tile.North();
            var east = tile.East();
            var south = tile.South();
            var west = tile.West();

            var oldNorthRegion = north?.Region;
            var oldEastRegion = east?.Region;
            var oldSouthRegion = south?.Region;
            var oldWestRegion = west?.Region;


            if (ShouldFloodFill(north, oldNorthRegion)) FloodFillFrom(north, tile);
            if (ShouldFloodFill(east, oldEastRegion)) FloodFillFrom(east, tile);
            if (ShouldFloodFill(south, oldSouthRegion)) FloodFillFrom(south, tile);
            if (ShouldFloodFill(west, oldWestRegion)) FloodFillFrom(west, tile);
        }

        private static bool ShouldFloodFill(Tile tile, Region oldRegion)
        {
            if (tile == null) return false;
            if (tile.MovementCost == 0) return false;
            return tile.Region == oldRegion;
        }

    }
}