using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Swordfish
{

[System.Serializable]
public class ItemStack
{
    [SerializeField] private Item item;
    [SerializeField] private int amount = 1;
    [SerializeField] private int stackSize = 0; //  0 defaults to a stack size determined by the item

    public bool IsValid() { return amount > 0 && item != null; }

    public Item GetItem() { return item; }
    public int GetAmount() { return amount; }
    public int GetStackSize() { return stackSize == 0 ? item.GetDefaultStackSize() : stackSize; }   //  Return the item's stack size if it isn't overriden

    public void SetItem(Item item) { this.item = item; }

    public void SetAmount(int amount) { this.amount = Mathf.Clamp(amount, 0, GetStackSize()); }
    public void SetAmountRaw(int amount) { this.amount = amount; }

    public void ModifyAmount(int value) { this.amount = Mathf.Clamp(this.amount + value, 0, GetStackSize()); }
    public void ModifyAmountRaw(int value) { this.amount += value; }

    public ItemStack Clone()
    {
        return new ItemStack(item, amount, stackSize);
    }

    public ItemStack(Item item, int amount = 1, int stackSize = 0)
    {
        this.item = item;
        this.amount = amount;
        this.stackSize = stackSize;
    }

    public override bool Equals(System.Object obj)
    {
        ItemStack stack = obj as ItemStack;

        if (stack == null)
        {
            return false;
        }

        return (this.item == stack.item && this.amount == stack.amount);
    }

    public override int GetHashCode()
    {
        return item.GetHashCode() ^ amount.GetHashCode();
    }
}

}