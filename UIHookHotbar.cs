using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Swordfish;

public class UIHookHotbar : UIHook
{
    public event EventHandler<HotbarChangeEvent> OnHotbarChangeEvent;
    public class HotbarChangeEvent : Swordfish.Event
    {
        public Inventory inventory;
        public ItemStack itemStack;
        public int slot;
    }

    [Header("Inventory")]
    [SerializeField] private Entity inventoryHolder;
    [SerializeField] private Inventory inventory;

    [Header("Hotbar")]
    [SerializeField] private int hotbarSelectedIndex;
    [SerializeField] private int hotbarSize;
    [SerializeField] private Sprite hotbarBGSprite;
    [SerializeField] private Sprite hotbarSelectedSprite;

    [Header("Slots")]
    [SerializeField] private GameObject[] slotContainers;

    private List<UIHolderSlot> slotHolders = new List<UIHolderSlot>();
    private bool updateHotbar = false;

    public Inventory GetInventory() { return inventory; }
    public int GetHotbarIndex() { return hotbarSelectedIndex; }
    public ItemStack GetSelectedStack() { return inventory?.GetAt(hotbarSelectedIndex); }

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

        UpdateHotbar();
    }

    public override void Handle()
    {
        if (Input.GetButtonDown("Drop Item"))
        {
            if (Input.GetKey(KeyCode.LeftControl))
                GameMaster.GetPlayer().DropItem(hotbarSelectedIndex);
            else
                GameMaster.GetPlayer().DropItem(hotbarSelectedIndex, 1);
        }

        if (UIMaster.GetState() == UIState.NONE) // Only handle the hotbar when UI isn't open
        {
            //  Scrolling through hotbar
            if (Input.mouseScrollDelta.y != 0)
            {
                hotbarSelectedIndex -= Mathf.RoundToInt(Input.mouseScrollDelta.y);
                hotbarSelectedIndex = Util.WrapInt(hotbarSelectedIndex, 0, hotbarSize - 1);

                updateHotbar = true;
            }

            //  Use number keys to switch selected hotbar index
            for (int i = 0; i < hotbarSize; i++)
            {
                KeyCode key = (KeyCode)System.Enum.Parse(typeof(KeyCode), "Alpha" + (i + 1), true);
                if ( Input.GetKeyDown(key) )
                {
                    hotbarSelectedIndex = i;
                    updateHotbar = true;
                }
            }
        }

        if (updateHotbar)
        {
            UpdateHotbar();
            updateHotbar = false;
        }
    }

    public void OnInventoryChange(object sender, Inventory.InventoryChangeEvent e)
    {
        updateHotbar = true;
    }

    public void UpdateHotbar()
    {
        //  Invoke an hotbar change event
        HotbarChangeEvent e = new HotbarChangeEvent{ slot = hotbarSelectedIndex, itemStack = GetSelectedStack(), inventory = GetInventory() };
        OnHotbarChangeEvent?.Invoke(this, e);

        for (int i = 0; i < hotbarSize; i++)
        {
            if (i == hotbarSelectedIndex)
                slotHolders[i].GetComponent<Image>().sprite = hotbarSelectedSprite;
            else
                slotHolders[i].GetComponent<Image>().sprite = hotbarBGSprite;
        }
    }
}
