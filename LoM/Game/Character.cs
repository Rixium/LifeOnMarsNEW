using System.Collections.Generic;
using LoM.Game.Components;
using LoM.Game.Items;
using LoM.Game.Jobs;
using LoM.Serialization.Data;
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
            if (Tile?.Character == this)
                Tile.Character = null;

            Tile = newTile;
            Tile.Character = this;
        }

        public bool OnVerifyJob(Job job)
        {
            var itemRequirements = job.ItemsRequired();
            if (itemRequirements == null) return true;
            if (CarriedItem == null) return false;
            if (CarriedItem.Item.Type != itemRequirements.Type) return false;
            return CarriedItem.Amount >= itemRequirements.Amount;
        }

        public void OnJobWorked(Job job)
        {
            var itemRequirements = job.ItemsRequired();
            if (itemRequirements == null) return;
            if (CarriedItem == null) return;
            if (CarriedItem.Item.Type != itemRequirements.Type) return;
            
            job.AllocateItem(CarriedItem);

            if (CarriedItem.Amount <= 0)
                CarriedItem = null;
        }

        public ItemRequirements OnRequirementCheck(ItemRequirements requirements)
        {
            var actualRequirements = new ItemRequirements
            {
                Amount = requirements.Amount,
                Type = requirements.Type
            };

            if (CarriedItem != null && CarriedItem.Item.Type == actualRequirements.Type)
                actualRequirements.Amount -= CarriedItem.Amount;

            return actualRequirements;
        }

        public void OnPickupItemStack(ItemStack itemStack)
        {
            if (CarriedItem == null)
                CarriedItem = itemStack;
            else if (CarriedItem.Item.Type == itemStack.Item.Type)
                CarriedItem.Amount += itemStack.Amount;
        }
    }
}