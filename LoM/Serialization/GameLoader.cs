using System.IO;
using LoM.Constants;
using LoM.Game;
using Newtonsoft.Json;

namespace LoM.Serialization
{
    public class GameLoader
    {
        public static World LoadWorld(string savePath)
        {
            if (File.Exists(savePath) == false) return null;
            var worldData = File.ReadAllText(savePath);
            var save = JsonConvert.DeserializeObject<WorldSave>(worldData);

            var world = new World(50, 50);

            var worldTiles = world.Tiles;

            foreach (var tile in save.Tiles)
            {
                var x = tile.X;
                var y = tile.Y;
                var tileType = (TileType) tile.TileType;
                worldTiles[x, y].SetType(tileType);
            }

            foreach (var worldObject in save.WorldObjects)
            {
                var x = worldObject.X;
                var y = worldObject.Y;
                var objectName = worldObject.ObjectName;

                var prototype = WorldObjectChest.WorldObjectPrototypes[objectName];
                var newObject = prototype.Place(worldTiles[x, y]);
                worldTiles[x, y].PlaceObject(newObject);
            }

            return world;
        }

    }
}