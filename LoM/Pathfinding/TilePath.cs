using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices.WindowsRuntime;
using ConcurrentPriorityQueue;
using LoM.Game;
using Microsoft.Xna.Framework;

namespace LoM.Pathfinding
{
    public class TilePath
    {

        private readonly Tile _startTile;
        private readonly Tile _endTile;
        private readonly World _world;
        
        private List<Tile> _closedList = new List<Tile>();
        private Dictionary<Tile, int> _fScores = new Dictionary<Tile, int>();
        private Dictionary<Tile, Tile> _cameFrom = new Dictionary<Tile, Tile>();
        private ConcurrentPriorityQueue<Tile, int> _openList = new ConcurrentPriorityQueue<Tile, int>();

        public TilePath(Tile startTile, Tile endTile, World world)
        {
            _startTile = startTile;
            _endTile = endTile;
            _world = world;
        }
        
        public Stack<Tile> FindPath()
        {
            _openList.Enqueue(_startTile, 0);
            
            do
            {
                var bestFScoreTile = _openList.Dequeue();
                _closedList.Add(bestFScoreTile);

                var neighbors = bestFScoreTile.GetNeighbors();

                if (bestFScoreTile == _endTile)
                {
                    break;
                }

                foreach (var neighbor in neighbors)
                {
                    if (neighbor == null) continue;
                    if (_closedList.Contains(neighbor)) continue;
                    if (!_openList.Contains(neighbor))
                    {
                        if (neighbor.MovementCost == 0 && neighbor != _endTile) continue;

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

            return _cameFrom.ContainsKey(_endTile) ? ConstructPath() : null;
        }

        private Stack<Tile> ConstructPath()
        {
            var tile = _endTile;
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