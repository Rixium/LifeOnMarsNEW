using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace LoM.Game.Items
{
    public class ItemStack
    {

        public Tile Tile;
        public int Amount;
        public Item Item;
        public int MaxStack;
        public int TotalAllocated;

        public Dictionary<Character, int> Allocations = new Dictionary<Character, int>();

        public ItemStack(Item item, int initialAmount)
        {
            Item = item;
            Amount = initialAmount;
        }

        public int SpaceLeft => MaxStack - Amount;

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

        public void AddAllocation(int min)
        {
            TotalAllocated += min;
        }

        public int RetrieveAllocationAmount(Character character)
        {
            return Allocations[character];
        }

    }
}