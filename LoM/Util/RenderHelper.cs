using System.Text;
using LoM.Game;

namespace LoM.Util
{
    public class RenderHelper
    {

        private static StringBuilder _stringBuilder = new StringBuilder();

        public static string CreateNeighborString(WorldObject worldObject)
        {
            _stringBuilder.Clear();
            
            var northTile = worldObject.Tile.North();
            var eastTile = worldObject.Tile.East();
            var southTile = worldObject.Tile.South();
            var westTile = worldObject.Tile.West();

            if (northTile?.WorldObject?.ObjectName == worldObject.ObjectName)
                _stringBuilder.Append("N");
            if (eastTile?.WorldObject?.ObjectName == worldObject.ObjectName)
                _stringBuilder.Append("E");
            if (southTile?.WorldObject?.ObjectName == worldObject.ObjectName)
                _stringBuilder.Append("S");
            if (westTile?.WorldObject?.ObjectName == worldObject.ObjectName)
                _stringBuilder.Append("W");

            return _stringBuilder.ToString();
        }

    }
}