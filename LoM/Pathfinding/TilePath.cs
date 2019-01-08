using System;
using System.Collections.Generic;
using ConcurrentPriorityQueue;
using LoM.Game;

namespace LoM.Pathfinding
{

    public enum Neighbor
    {
        NORTH,
        EAST,
        SOUTH,
        WEST,
        NW,
        NE,
        SW,
        SE
    }

    public class TilePath
    {
        private readonly Tile _endTile;

        private readonly Tile _startTile;
        private readonly Dictionary<Tile, Tile> _cameFrom = new Dictionary<Tile, Tile>();

        private readonly HashSet<Tile> _closedList = new HashSet<Tile>();
        private readonly Dictionary<Tile, int> _fScores = new Dictionary<Tile, int>();
        private readonly ConcurrentPriorityQueue<Tile, int> _openList = new ConcurrentPriorityQueue<Tile, int>();

        public TilePath(Tile startTile, Tile endTile)
        {
            _startTile = startTile;
            _endTile = endTile;
        }

        public Stack<Tile> FindPath(bool includeLast)
        {
            if (_startTile == null || _endTile == null) return null;

            if (_startTile.Region != _endTile.Region &&
                !RegionTileNextTo(_startTile, _endTile)) return null;

            if (_startTile == _endTile)
            {
                var newStack = new Stack<Tile>();
                newStack.Push(_endTile);
                return newStack;
            }

            _openList.Enqueue(_startTile, 0);
            _fScores[_startTile] = 0;

            do
            {
                var bestFScoreTile = _openList.Dequeue();
                _closedList.Add(bestFScoreTile);

                var neighbors = bestFScoreTile.GetNeighbors();
                
                if (bestFScoreTile == _endTile) return ConstructPath(includeLast);

                foreach (var neighbor in neighbors)
                {
                    if (neighbor == null) continue;
                    if (neighbor.MovementCost == 0 && neighbor != _endTile) continue;
                    if (_closedList.Contains(neighbor)) continue;
                    if (CanMoveToNeighbour(neighbor, neighbors) == false) continue;

                    int fScore = _fScores[bestFScoreTile] + Cost(bestFScoreTile, neighbor);
                    
                    if (!_openList.Contains(neighbor) || fScore < _fScores[neighbor])
                    {

                        _fScores[neighbor] =  fScore;
                        _cameFrom[neighbor] = bestFScoreTile;
                        var priority = (int)(fScore + Heuristic(neighbor, _endTile));
                        _openList.Enqueue(neighbor, -priority);
                    }
                }
            } while (_openList.Count > 0);

            return null;
        }

        private bool CanMoveToNeighbour(Tile neighbor, Tile[] tiles)
        {
            if (neighbor == tiles[(int) Neighbor.NW])
                return tiles[(int) Neighbor.NORTH]?.MovementCost != 0 &&
                       tiles[(int) Neighbor.WEST]?.MovementCost != 0;
            if (neighbor == tiles[(int)Neighbor.NE])
                return tiles[(int)Neighbor.NORTH]?.MovementCost != 0 &&
                       tiles[(int)Neighbor.EAST]?.MovementCost != 0;
            if (neighbor == tiles[(int)Neighbor.SE])
                return tiles[(int)Neighbor.SOUTH]?.MovementCost != 0 &&
                       tiles[(int)Neighbor.EAST]?.MovementCost != 0;
            if (neighbor == tiles[(int)Neighbor.SW])
                return tiles[(int)Neighbor.SOUTH]?.MovementCost != 0 &&
                       tiles[(int)Neighbor.WEST]?.MovementCost != 0;

            return true;
        }

        /// <summary>
        /// This allows us to traverse tiles next to each other, even if they are in different regions.
        /// </summary
        private bool RegionTileNextTo(Tile startTile, Tile endTile)
        {
            var endTileRegion = endTile.Region;
            foreach (var neighbor in startTile.GetNeighbors())
            {
                if (neighbor == null) continue;
                if (neighbor.Region == null || endTile.Region == null) continue;
                if (neighbor == endTile) return true;

                // TODO Make sure that this works or optimise.
                foreach (var tile in neighbor.GetNeighbors())
                {
                    if (tile == startTile) continue;
                    if (neighbor.Region == startTile.Region) return true;
                }
                if (neighbor.MovementCost == 0) continue;
                if (neighbor.Region == endTileRegion) return true;
            }

            return false;
        }

        private Stack<Tile> ConstructPath(bool includeLast)
        {
            var tile = _cameFrom[_endTile];

            if (includeLast && _endTile.MovementCost != 0) tile = _endTile;

            var stack = new Stack<Tile>();

            do
            {
                stack.Push(tile);

                if (_cameFrom.ContainsKey(tile))
                    tile = _cameFrom[tile];
                else return stack;
            } while (tile != _startTile);

            return stack;
        }

        private int Cost(Tile child, Tile parent)
        {
            var currCost = child.MovementCost;

            var distance = Math.Abs(child.X - parent.X) + Math.Abs(child.Y - parent.Y);
            currCost += distance;
            return (int) currCost;
        }

        private static float Heuristic(Tile a, Tile b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }
    }
}