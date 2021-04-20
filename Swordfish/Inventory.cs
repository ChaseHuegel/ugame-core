using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Swordfish
{

[CreateAssetMenu(fileName = "New Inventory", menuName = "Swordfish/Items/_Inventory")]
public class Inventory : ScriptableObject
{
    public enum InventoryChangeReason
    {
        SET,
        ADD,
        REMOVE
    }

    public event EventHandler<InventoryChangeEvent> OnInventoryChangeEvent;
    public class InventoryChangeEvent : Event
    {
        public InventoryChangeReason reason;
        public Inventory inventory;
        public ItemStack itemStack;
        public int slot;
    }

    [SerializeField] private Entity owner;
    [SerializeField] private ItemStack[] contents = new ItemStack[36];

    public bool IsSlotValid(int slot) { return contents[slot] != null && contents[slot].IsValid(); }
    public bool InBounds(int slot) { return slot < GetSize() && slot >= 0;}
    public int GetSize() { return contents.Length; }
    public ItemStack[] GetContents() { return contents; }
    public void SetContents(ItemStack[] contents) { this.contents = contents; }

    public Inventory(int size = 36)
    {
        contents = new ItemStack[size];
    }

    private ItemStack Get(int slot) { return contents[slot]; }
    private void SetRaw(int slot, ItemStack stack) { contents[slot] = stack; }  //  USE CAREFULLY! VALIDATE SLOTS!
    private void Set(int slot, ItemStack stack)
    {
        //  Invoke an inventory change event
        InventoryChangeEvent e = new InventoryChangeEvent{ reason = InventoryChangeReason.SET, slot = slot, itemStack = stack, inventory = this };
        OnInventoryChangeEvent?.Invoke(this, e);
        if (e.cancel == true) { return; }   //  return if the event has been cancelled by any subscriber

        contents[e.slot] = e.itemStack;
        ValidateSlot(slot);
    }

    //  !!!!!!!!!
    //  IMPORTANT: Validate everytime a slot is modified! This nulls out invalid/empty item stacks.
    //  !!!!!!!!!
    public void ValidateSlot(int slot)
    {
        if (contents[slot] != null)
        {
            if (contents[slot].IsValid() == false) { contents[slot] = null; }
        }
    }

    //  Attempt to add a stack to the inventory. Returns an itemstack representing what couldn't be added.
    public ItemStack Add(Item item, int count = 1) { return Add(new ItemStack(item, count)); }
    public ItemStack Add(ItemStack stack)
    {
        int amount = stack.GetAmount();
        List<int> matchingIndices = new List<int>();

        //  Find all matching stacks
        for (int i = 0; i < contents.Length; i++)
        {
            if (Get(i) != null && Get(i).GetItem() == stack.GetItem())
            {
                matchingIndices.Add(i);
            }
        }

        //  Combine matching stacks until there is none left
        foreach (int index in matchingIndices)
        {
            //  Get overflow/remainder that would be left after adding
            int overflow = Util.GetOverflow( Get(index).GetAmount() + amount, 0, Get(index).GetStackSize() );

            //  Attempt adding the amount to the current matching stack
            Get(index).ModifyAmount( amount );

            //  Update the current stack to the remainder
            stack.SetAmountRaw( overflow );

            //  Invoke an inventory change event
            InventoryChangeEvent e = new InventoryChangeEvent{ reason = InventoryChangeReason.ADD, slot = index, itemStack = stack, inventory = this };
            OnInventoryChangeEvent?.Invoke(this, e);
            if (e.cancel == true) { return e.itemStack; }   //  return if the event has been cancelled by any subscriber

            //  Update the stack with any changes by the event
            stack.SetAmountRaw(e.itemStack.GetAmount());
            stack.SetItem(e.itemStack.GetItem());

            if (stack.IsValid() == false) return stack;    //  Finished, early exit
        }

        //  Attempt to fill empty slots with any remainder until there is none left
        for (int i = 0; i < contents.Length; i++)
        {
            if (IsSlotValid(i) == false)    //  Find empty slots
            {
                //  Add a stack to the empty slot
                Set( i, new ItemStack(stack.GetItem()) );
                Get(i).SetAmount( stack.GetAmount() );

                //  Cover case scenarios where the stack is larger than its size limits by breaking it into multiple stacks
                int remainder = stack.GetAmount() - Get(i).GetAmount();

                //  Update the current stack to the remainder that overflows the stack size
                stack.SetAmountRaw(remainder);

                //  Invoke an inventory change event
                InventoryChangeEvent e = new InventoryChangeEvent{ reason = InventoryChangeReason.ADD, slot = i, itemStack = stack, inventory = this };
                OnInventoryChangeEvent?.Invoke(this, e);
                if (e.cancel == true) { return e.itemStack; }   //  return if the event has been cancelled by any subscriber

                if (stack.IsValid() == false) return stack;    //  Finished, early exit
            }
        }

        return stack;
    }

    //  Attempt to add a stack to a specific slot with no overflow control
    public ItemStack AddAt(int slot, Item item, int count = 1) { return AddAt(slot, new ItemStack(item, count)); }
    public ItemStack AddAt(int slot, ItemStack stack)
    {
        int amount = stack.GetAmount();

        if (Get(slot) != null && Get(slot).GetItem() == stack.GetItem())
        {
            //  Get overflow/remainder that would be left after adding
            int overflow = Util.GetOverflow( Get(slot).GetAmount() + amount, 0, Get(slot).GetStackSize() );

            //  Attempt adding the amount to the current matching stack
            Get(slot).ModifyAmount( amount );
            ValidateSlot(slot);

            //  Update the current stack to the remainder
            stack.SetAmountRaw( overflow );
        }
        else if (Get(slot) == null)
        {
            SetAt(slot, stack);
        }

        //  Invoke an inventory change event
        InventoryChangeEvent e = new InventoryChangeEvent{ reason = InventoryChangeReason.ADD, slot = slot, itemStack = stack, inventory = this };
        OnInventoryChangeEvent?.Invoke(this, e);
        if (e.cancel == true) { return e.itemStack; }   //  return if the event has been cancelled by any subscriber

        //  Update the stack with any changes by the event
        stack?.SetAmountRaw(e.itemStack.GetAmount());
        stack?.SetItem(e.itemStack.GetItem());

        return stack;
    }

    //  Attempt to add a stack to a specific slot with no overflow control
    public ItemStack Combine(int fromSlot, int toSlot)
    {
        int amount = Get(fromSlot).GetAmount();

        if (Get(toSlot) != null && Get(toSlot).GetItem() == Get(fromSlot).GetItem())
        {
            //  Get overflow/remainder that would be left after adding
            int overflow = Util.GetOverflow( Get(toSlot).GetAmount() + amount, 0, Get(toSlot).GetStackSize() );

            //  Attempt adding the amount to the current matching stack
            Get(toSlot).ModifyAmount( amount );
            ValidateSlot(toSlot);

            //  Update the current stack to the remainder
            Get(fromSlot).SetAmountRaw( overflow );
            ValidateSlot(fromSlot);
        }
        else if (Get(toSlot) == null)
        {
            SetAt(toSlot, Get(fromSlot));
        }

        //  Invoke an inventory change event on the from slot
        InventoryChangeEvent e = new InventoryChangeEvent{ reason = InventoryChangeReason.REMOVE, slot = fromSlot, itemStack = Get(toSlot), inventory = this };
        OnInventoryChangeEvent?.Invoke(this, e);
        if (e.cancel == true) { return e.itemStack; }   //  return if the event has been cancelled by any subscriber

        //  Invoke an inventory change event on the to slot
        e = new InventoryChangeEvent{ reason = InventoryChangeReason.ADD, slot = toSlot, itemStack = Get(fromSlot), inventory = this };
        OnInventoryChangeEvent?.Invoke(this, e);
        if (e.cancel == true) { return e.itemStack; }   //  return if the event has been cancelled by any subscriber

        return Get(fromSlot);
    }

    //  Attempt to remove an amount of item from the inventory
    public void Remove(Item item, int amount = 1)
    {
        List<int> matchingIndices = new List<int>();

        //  Find all matching stacks
        for (int i = 0; i < contents.Length; i++)
        {
            if (Get(i) != null && Get(i).GetItem() == item)
            {
                matchingIndices.Add(i);
            }
        }

        //  Remove from matching stacks until there is none left
        foreach (int index in matchingIndices)
        {
            //  Get the overflow/remainder that would be left after removal
            int overflow = Util.GetOverflow( Get(index).GetAmount() - amount, 0, Get(index).GetStackSize() );

            //  Attempt removing the amount from the current matching stack
            Get(index).ModifyAmount( -amount );

            //  Update the current amount to the remainder
            amount = Mathf.Abs(overflow);

            //  Invoke an inventory change event
            InventoryChangeEvent e = new InventoryChangeEvent{ reason = InventoryChangeReason.REMOVE, slot = index, itemStack = Get(index), inventory = this };
            OnInventoryChangeEvent?.Invoke(this, e);
            if (e.cancel == true) { return; }   //  return if the event has been cancelled by any subscriber

            if (amount <= 0) return; //  Finished, early exit
        }
    }

    //  Remove all stacks of an item from the inventory
    public void RemoveAll(Item item, int amount = 1)
    {
        List<int> matchingIndices = new List<int>();

        //  Find all matching stacks
        for (int i = 0; i < contents.Length; i++)
        {
            if (Get(i) != null && Get(i).GetItem() == item)
            {
                Set(i, null);
            }
        }
    }

    //  Attempt to remove an amount from the specified slot
    public void RemoveAt(int slot, int amount = 1)
    {
        List<int> matchingIndices = new List<int>();

        if (Get(slot) != null)
        {
            Get(slot).ModifyAmount(-amount);
            ValidateSlot(slot);
        }

        //  Invoke an inventory change event
        InventoryChangeEvent e = new InventoryChangeEvent{ reason = InventoryChangeReason.REMOVE, slot = slot, itemStack = Get(slot), inventory = this };
        OnInventoryChangeEvent?.Invoke(this, e);
        if (e.cancel == true) { return; }   //  return if the event has been cancelled by any subscriber
    }

    //  Attempt to remove all from the specified slot
    public void RemoveAllAt(int slot)
    {
        Set(slot, null);
    }

    //  Set the specified slot
    public void SetAt(int slot, ItemStack stack)
    {
        Set(slot, stack);
        ValidateSlot(slot);
    }

    public ItemStack GetAt(int slot)
    {
        return Get(slot);
    }

    public void Swap(int slot1, int slot2)
    {
        ItemStack temp = Get(slot1);
        SetRaw(slot1, Get(slot2));
        SetRaw(slot2, temp);

        ValidateSlot(slot1);
        ValidateSlot(slot2);

        //  Invoke an inventory change event on slot1
        InventoryChangeEvent e = new InventoryChangeEvent{ slot = slot1, itemStack = Get(slot1), inventory = this };
        OnInventoryChangeEvent?.Invoke(this, e);
        if (e.cancel == true) { return; }   //  return if the event has been cancelled by any subscriber

        //  Invoke an inventory change event on slot2
        e = new InventoryChangeEvent{ slot = slot2, itemStack = Get(slot2), inventory = this };
        OnInventoryChangeEvent?.Invoke(this, e);
        if (e.cancel == true) { return; }   //  return if the event has been cancelled by any subscriber
    }
}

}