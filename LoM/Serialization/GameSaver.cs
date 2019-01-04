using System;
using System.Collections.Generic;
using System.IO;
using LoM.Game;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LoM.Serialization
{
    public class GameSaver
    {
        public static void SaveGame(World world, string name)
        {
            var saveDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/LifeOnMars";

            if(Directory.Exists(saveDirectory) == false)
                Directory.CreateDirectory(saveDirectory);

            var worldSave = new WorldSave();

            SaveTiles(worldSave, world);
            SaveWorldObjects(worldSave, world);
            
            var json = JsonConvert.SerializeObject(worldSave);
            json = JToken.Parse(json).ToString(Formatting.Indented);
            
            if (File.Exists($"{saveDirectory}/{name}2.lom"))
                File.Delete($"{saveDirectory}/{name}.lom");

            File.WriteAllText($"{saveDirectory}/{name}2.lom", json);
        }

        private static void SaveWorldObjects(WorldSave worldSave, World world)
        {
            var tiles = world.Tiles;
            var worldObjectData = new List<WorldObjectData>();

            foreach (var tile in tiles)
            {
                if (tile.WorldObject == null) continue;
                var data = new WorldObjectData
                {
                    X = tile.X,
                    Y = tile.Y,
                    ObjectName = tile.WorldObject.ObjectName
                };

                worldObjectData.Add(data);
            }

            worldSave.WorldObjects = worldObjectData;
        }

        private static void SaveTiles(WorldSave worldSave, World world)
        {
            var tiles = world.Tiles;
            var tileData = new List<TileData>();

            foreach (var tile in tiles)
            {
                var data = new TileData
                {
                    X = tile.X,
                    Y = tile.Y,
                    TileType = (int)tile.Type
                };
                tileData.Add(data);
            }

            worldSave.Tiles = tileData;
        }

    }
}