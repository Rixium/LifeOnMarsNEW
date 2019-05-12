using System.Collections.Generic;
using LoM.Game;
using LoM.Game.Items;
using LoM.Serialization.Data;

namespace LoM.Managers
{
    public class ItemManager
    {
        public List<ItemStack> ItemStacks = new List<ItemStack>();
        public List<Tile> Stockpiles = new List<Tile>();

        public void OnStockpileCreated(Tile tile)
        {
            Stockpiles.Add(tile);
        }

        public void AddItems(ItemStack itemStack)
        {
            itemStack.OnItemStackChanged = OnStackChange;
            ItemStacks.Add(itemStack);
        }

        public Tile FindItem(ItemRequirements itemRequirements)
        {
            foreach (var item in ItemStacks)
            {
                if (item.Tile == null) continue;
                if (item.TotalAllocated == item.Amount) continue;
                if (item.Item.Type == itemRequirements.Type) return item.Tile;
            }

            return null;
        }

        public void OnStackChange(ItemStack itemStack)
        {
            if (itemStack.Amount > 0) return;
            if (itemStack.Tile != null)
                itemStack.Tile.ItemStack = null;

            ItemStacks.Remove(itemStack);
        }

        public void DeallocateAll()
        {
            foreach (var stack in ItemStacks)
                stack.TotalAllocated = 0;
        }

        public Tile GetStockpile()
        {
            foreach (var stockpile in Stockpiles)
            {
                if (stockpile.ItemStack != null &&
                    stockpile.ItemStack.SpaceLeft <= 0) continue;
                return stockpile;
            }

            return null;
        }
    }
}