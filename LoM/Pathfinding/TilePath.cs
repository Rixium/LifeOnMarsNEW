﻿using System;
using System.Collections.Generic;
using ConcurrentPriorityQueue;
using LoM.Game;
using Microsoft.Xna.Framework;

namespace LoM.Pathfinding
{
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
            if (_startTile.Region != _endTile.Region &&
                !RegionTileNextTo(_startTile, _endTile)) return null;

            if (_startTile == _endTile)
            {
                var newStack = new Stack<Tile>();
                newStack.Push(_endTile);
                return newStack;
            }

            _openList.Enqueue(_startTile, 0);

            do
            {
                var bestFScoreTile = _openList.Dequeue();
                _closedList.Add(bestFScoreTile);

                var neighbors = bestFScoreTile.GetNeighbors();

                if (bestFScoreTile == _endTile) return ConstructPath(includeLast);

                foreach (var neighbor in neighbors)
                {
                    if (neighbor == null) continue;
                    if (_closedList.Contains(neighbor)) continue;
                    if (!_openList.Contains(neighbor))
                    {
                        if (neighbor.MovementCost == 0 &&
                            neighbor != _endTile) continue;

                        var fScore = CalculateFScore(neighbor, bestFScoreTile);
                        _fScores[neighbor] = fScore;
                        _cameFrom[neighbor] = bestFScoreTile;
                        _openList.Enqueue(neighbor, -fScore);
                        continue;
                    }

                    var currentFScore = _fScores[neighbor];
                    var newFScore = CalculateFScore(bestFScoreTile, neighbor);

                    if (newFScore > currentFScore) continue;

                    _cameFrom[neighbor] = bestFScoreTile;
                    _fScores[neighbor] = newFScore;
                }
            } while (_openList.Count > 0);

            return null;
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

        private int CalculateFScore(Tile child, Tile parent)
        {
            var parentFScore = 0;

            if (_fScores.ContainsKey(parent))
                parentFScore = _fScores[parent];

            var distance = new Vector2(Math.Abs(_endTile.X - child.X), Math.Abs(_endTile.Y - child.Y));
            var totalDistance = distance.X + distance.Y;
            var fScore = parentFScore + totalDistance;
            return (int) fScore;
        }
    }
}