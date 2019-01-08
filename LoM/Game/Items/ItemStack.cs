using Microsoft.Xna.Framework;

namespace LoM.Game.Items
{
    public class ItemStack
    {

        public Tile Tile;
        public int Amount;
        public Item Item;
        public int MaxStack;

        public ItemStack(Item item)
        {
            Item = item;
        }

        public int SpaceLeft => MaxStack - Amount;
        public int TotalAllocated { get; set; } = 0;

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

    }
}