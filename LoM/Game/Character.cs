using System.Collections.Generic;
using LoM.Game.Components;
using LoM.Game.Items;
using Microsoft.Xna.Framework;

namespace LoM.Game
{
    public class Character
    {
        private readonly List<IComponent> _characterComponents = new List<IComponent>();
        public ItemStack CarriedItem;
        public Vector2 Position;
        public string CharacterType;
        
        public Character(Tile tile, string characterType)
        {
            Tile = tile;
            Position = new Vector2(Tile.X, Tile.Y);

            Tile.Character = this;
            CharacterType = characterType;
        }

        public Tile Tile { get; private set; }

        public void AddComponent(IComponent component)
        {
            _characterComponents.Add(component);
            component.Character = this;
        }

        public void Update(float deltaTime)
        {
            foreach (var component in _characterComponents)
                component.Update(deltaTime);
        }

        public void OnNewPathRequest(Tile endTile)
        {
        }

        public void SetTile(Tile newTile)
        {
            Tile = newTile;
            Tile.Character = this;
        }
        
    }
}