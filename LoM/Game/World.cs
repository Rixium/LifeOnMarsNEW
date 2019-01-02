using System;
using System.Collections.Generic;
using LoM.Game.Jobs;
using LoM.Managers;

namespace LoM.Game
{
    public class World
    {

        public Func<Character, Job> OnJobRequest;
        public Action<Tile> OnTileChanged;
        public int Width { get; }
        public int Height { get; }
        public Tile[,] Tiles;

        public List<Character> Characters { get; } = new List<Character>();

        public World(int width, int height)
        {
            Width = width;
            Height = height;

            Tiles = new Tile[width, height];

            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
            {
                Tiles[x, y] = new Tile(x, y, this)
                {
                    OnTileChanged = TileChanged
                };
            }
            
            Characters.Add(new Character(Tiles[width / 2, height / 2], "Dan"));
            Characters.Add(new Character(Tiles[width / 2 + 2, height / 2], "Tiffany"));
            Characters.Add(new Character(Tiles[width / 2 - 2, height / 2], "Mario"));
            Characters.Add(new Character(Tiles[width / 2, height / 2 + 2], "Bran"));
            Characters.Add(new Character(Tiles[width / 2, height / 2 - 2], "Grace"));
            Characters.Add(new Character(Tiles[width / 2 - 2, height / 2 + 2], "Lara"));
        }

        public void Update(float deltaTime)
        {
            foreach(var character in Characters)
                character.Update(deltaTime);
        }

        public Tile GetTileAt(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return null;
            return Tiles[x, y];
        }

        public void TileChanged(Tile tile)
        {
            OnTileChanged?.Invoke(tile);
        }

        public Job GetJob(Character character)
        {
            return OnJobRequest?.Invoke(character);
        }

    }
}