using System;
using System.Collections.Generic;
using LoM.Game.Items;
using LoM.Game.Jobs;
using LoM.Serialization.Data;

namespace LoM.Game
{
    public class World
    {
        public Func<ItemRequirements, Job> OnFetchJobRequest;
        public Func<ItemRequirements, Tile> OnFindItemRequest;

        public Func<Character, Job> OnJobRequest;

        public Action<Tile> OnTileChanged;
        public Action<ItemStack> OnItemStackChange;
        public Action<WorldObject> OnWorldObjectPlaced;
        public Action<WorldObject> OnWorldObjectDestroyed;

        public Tile[,] Tiles;

        public World(int width, int height)
        {
            Width = width;
            Height = height;

            Tiles = new Tile[width, height];

            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
                Tiles[x, y] = new Tile(x, y, this)
                {
                    OnTileChanged = TileChanged,
                    OnWorldObjectDestroyed = OnWorldObjectDestroyed
                };
        }

        public int Width { get; }
        public int Height { get; }

        public List<Character> Characters { get; } = new List<Character>();
        public List<WorldObject> WorldObjects { get; } = new List<WorldObject>();

        public void Update(float deltaTime)
        {
            foreach (var character in Characters)
                character.Update(deltaTime);

            foreach (var worldObject in WorldObjects)
                worldObject.Update(deltaTime);
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

        public void PlaceWorldObject(Tile tile, WorldObject worldObject)
        {
            if (tile.PlaceObject(worldObject))
            {
                tile.SetType(TileType.Ground);
                OnWorldObjectPlaced?.Invoke(worldObject);
                WorldObjects.Add(worldObject);
            }
        }

        public Job GetFetchJob(ItemRequirements itemsRequired)
        {
            return OnFetchJobRequest?.Invoke(itemsRequired);
        }

        public Tile FindItemTile(ItemRequirements requiredItem)
        {
            return OnFindItemRequest?.Invoke(requiredItem);
        }

        public void OnItemStackChanged(ItemStack itemStack)
        {
            OnItemStackChange?.Invoke(itemStack);
        }

    }
}