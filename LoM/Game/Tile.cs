namespace LoM.Game
{
    public class Tile
    {
        public TileType Type;
        public int X;
        public int Y;

        public Tile(int x, int y)
        {
            X = x;
            Y = y;
            Type = TileType.None;
        }

    }
}