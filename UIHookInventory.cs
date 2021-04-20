using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Swordfish;

public class UIHookInventory : UIHook
{
    [Header("Inventory")]
    [SerializeField] private Entity inventoryHolder;
    [SerializeField] private Inventory inventory;

    [Header("Slots")]
    [SerializeField] private UIHolderSlot cursorSlot;
    [SerializeField] private GameObject[] slotContainers;

    private List<UIHolderSlot> slotHolders = new List<UIHolderSlot>();
    private bool updateSlots = false;

    public Inventory GetInventory() { return inventory; }
    public UIHolderSlot GetCursorSlot() { return cursorSlot; }

    private void Start()
    {
        if (inventory == null)
        {
            if (inventoryHolder != null) inventory = inventoryHolder.GetInventory();
            else
            {
                Debug.LogError("No inventory or inventory holder defined!! Assigning a new inventory to avoid breaking things.");
                inventory = new Inventory();
                inventoryHolder.SetInventory(inventory);
            }
        }

        inventory.OnInventoryChangeEvent += OnInventoryChange;

        for (int i = 0; i < slotContainers.Length; i++)
        {
            slotHolders.AddRange( slotContainers[i].GetComponentsInChildren<UIHolderSlot>() );
        }

        UpdateSlots();
    }

    public override void Handle()
    {
        if (Input.GetButtonDown("Inventory")) Toggle();
        if (Input.GetButtonDown("Close UI")) Disable();

        if (IsActive())
        {
            Cursor.visible = true; Cursor.lockState = CursorLockMode.None;
        }

        blockInput = IsActive();

        if (updateSlots)
        {
            UpdateSlots();
            updateSlots = false;
        }
    }

    public void OnInventoryChange(object sender, Inventory.InventoryChangeEvent e)
    {
        updateSlots = true;
    }

    public void UpdateSlots()
    {
        Item item;
        ItemStack stack;
        Image img;
        Text count;

        for (int slot = 0; slot < inventory.GetSize(); slot++)  //  Loop the inventory
        {
            if (slot >= 0 && slot < slotHolders.Count) //  Make certain we aren't out of bounds!
            {
                slotHolders[slot].slot = slot;
                slotHolders[slot].inventoryHook = this;

                img = slotHolders[slot].iconImage;
                count = slotHolders[slot].countText;

                stack = inventory.GetAt(slot);
                item = (stack == null ? null : stack.GetItem());    //  Item is null if stack is null

                if (item != null) img.sprite = item.GetThumbnail();
                img.gameObject.SetActive( item != null );   //  Only activate if item isn't null

                if (stack != null) count.text = stack.GetAmount().ToString();
                count.gameObject.SetActive(  stack != null && stack.GetAmount() > 1 );   //  Only activate if count > 1 and stack isnt null
            }
        }
    }
}
