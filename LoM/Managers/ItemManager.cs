using System.Collections.Generic;
using LoM.Game;
using LoM.Game.Items;
using LoM.Serialization.Data;

namespace LoM.Managers
{
    public class ItemManager
    {
        
        public List<ItemStack> ItemStacks = new List<ItemStack>();

        public ItemManager()
        {

        }

        public void AddItems(ItemStack itemStack)
        {
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
    }
}