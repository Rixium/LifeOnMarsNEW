using System.Collections.Generic;
using LoM.Game.Components;
using LoM.Game.Items;
using LoM.Util;
using Microsoft.Xna.Framework;

namespace LoM.Game
{
    public class Character
    {
        public ItemStack CarriedItem;

        public string CharacterType;
        
        private readonly List<IComponent> _characterComponents = new List<IComponent>();

        public float Speed = 5f;

        public Character(Tile tile, string characterType)
        {
            Tile = tile;
            Tile.Character = this;
            CharacterType = characterType;
        }

        public void AddComponent(IComponent component)
        {
            _characterComponents.Add(component);
            component.Character = this;
        }

        public Tile Tile { get; private set; }

        public World World => Tile.World;

        public void Update(float deltaTime)
        {   
            foreach(var component in _characterComponents)
                component.Update(deltaTime);
        }
        
        public void OnNewPathRequest(Tile endTile)
        {

        }

        public void SetTile(Tile newTile)
        {
            Tile = newTile;
        }

    }
}