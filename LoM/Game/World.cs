using System;

namespace LoM.Game
{
    public class World
    {

        public Action<Tile> OnTileChanged;
        public int Width { get; }
        public int Height { get; }
        public Tile[,] Tiles;

        public World(int width, int height)
        {
            Width = width;
            Height = height;

            Tiles = new Tile[width, height];

            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
            {
                Tiles[x, y] = new Tile(x, y)
                {
                    OnTileChanged = TileChanged
                };
            }
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
    }
}