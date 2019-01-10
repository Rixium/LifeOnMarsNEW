using System;
using System.Collections.Generic;
using LoM.Game.Jobs;
using Microsoft.Xna.Framework;

namespace LoM.Game.Items
{
    public class ItemStack
    {

        public Action<ItemStack> OnItemStackChanged;
        public Tile Tile;
        public int Amount;
        public Item Item;
        public int MaxStack;
        public int TotalAllocated;

        public ItemStack(Item item, int initialAmount)
        {
            Item = item;
            Amount = initialAmount;
        }

        public int SpaceLeft => MaxStack - Amount;
        public int Available => Amount - TotalAllocated;

        public bool AddToStack(int amount)
        {
            if (amount > SpaceLeft) return false;
            Amount += amount;
            return true;
        }

        public ItemStack MergeWith(ItemStack stack)
        {
            if (stack.Item.Type != Item.Type) return stack;
            if (SpaceLeft == 0) return stack;

            var amountToAdd = MathHelper.Min(SpaceLeft, stack.Amount);

            if (AddToStack(amountToAdd) == false)
                return stack;

            stack.Amount -= amountToAdd;
            return stack;
        }
        
        public ItemStack Take(FetchRequest request)
        {
            var takeAmount = request.Allocated;

            Amount -= takeAmount;
            TotalAllocated -= takeAmount;

            OnItemStackChanged?.Invoke(this);
            return new ItemStack(Item, takeAmount);
        }
    }
}