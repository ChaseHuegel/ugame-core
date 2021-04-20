using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Swordfish;

public class UIHolderSlot : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
{
    [Header("Holder Data")]
    public GameObject iconObject;
    public Image iconImage;

    public GameObject countObject;
    public Text countText;

    [Header("Slot Data")]
    public UIHookInventory inventoryHook;
    public int slot = 0;
    public int amount = 0;

    private bool dragging = false;

    public void ResetDrag()
    {
        iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, 1.0f);
        inventoryHook.GetCursorSlot().gameObject.SetActive(false);
        dragging = false;
    }

    public void OnBeginDrag(PointerEventData e)
    {
        if (inventoryHook.GetInventory().GetAt(slot) == null) return;
        if (e.button != PointerEventData.InputButton.Left) return;

        iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, 0.15f);
        inventoryHook.GetCursorSlot().iconImage.sprite = iconImage.sprite;
        inventoryHook.GetCursorSlot().countText.text = countText.text;
        inventoryHook.GetCursorSlot().gameObject.SetActive(true);
        inventoryHook.GetCursorSlot().countObject.SetActive( countText.text != "1" );
        inventoryHook.GetCursorSlot().slot = slot;

        dragging = true;
    }

    public void OnDrag(PointerEventData e)
    {
        if (inventoryHook.GetInventory().GetAt(slot) == null) return;
        if (e.button != PointerEventData.InputButton.Left) return;

        inventoryHook.GetCursorSlot().GetComponent<RectTransform>().position = e.position;
        e.pointerDrag = this.gameObject;
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (e.button != PointerEventData.InputButton.Left) return;

        ResetDrag();

        if (e.pointerCurrentRaycast.isValid == false)
        {
            GameMaster.GetPlayer().DropItem(slot);
        }
    }

    public void OnPointerDown(PointerEventData e)
    {
        if (e.button == PointerEventData.InputButton.Right && inventoryHook.GetCursorSlot().gameObject.activeInHierarchy)
        {
            Inventory inv = inventoryHook.GetInventory();
            ItemStack stack = inv.GetAt(inventoryHook.GetCursorSlot().slot);
            ItemStack thisStack = inv.GetAt(slot);

            inv.AddAt(slot, stack.GetItem(), 1);
            inv.RemoveAt(inventoryHook.GetCursorSlot().slot, 1);

            stack = inv.GetAt(inventoryHook.GetCursorSlot().slot);

            //  Update the cursor count to match
            inventoryHook.GetCursorSlot().countText.text = stack.GetAmount().ToString();
            inventoryHook.GetCursorSlot().countObject.SetActive( stack.GetAmount() > 1 );

            //  Stop dragging if we no longer have a valid item
            if (stack.IsValid() == false)
            {
                ResetDrag();
            }
        }
    }

    public void OnDrop(PointerEventData e)
    {
        if (inventoryHook == null) return;
        if (e.button != PointerEventData.InputButton.Left) return;

        if (e.pointerDrag != null)
        {
            UIHolderSlot droppedSlot = e.pointerDrag.GetComponent<UIHolderSlot>();
            if (droppedSlot != null)
            {
                if (inventoryHook.GetInventory().GetAt(droppedSlot.slot) == null) return;

                Inventory inv = inventoryHook.GetInventory();
                ItemStack stack = inv.GetAt(slot);
                ItemStack destinationStack = inv.GetAt(droppedSlot.slot);

                if (stack != null && destinationStack != null && stack != destinationStack)
                {
                    //  If destination is the same item and isn't fully stacked, fill it.
                    if ( stack.GetItem() == destinationStack.GetItem() && destinationStack.GetAmount() < destinationStack.GetStackSize() )
                    {
                        inv.Combine(droppedSlot.slot, slot);
                        return;
                    }
                }

                //  Fallback or any other case, swap slots
                inv.Swap(slot, droppedSlot.slot);
            }
        }
    }
}
