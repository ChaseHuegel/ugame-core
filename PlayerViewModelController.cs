using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Swordfish;

public class PlayerViewModelController : ViewModelController
{
    [Header("Player View Model")]
    [SerializeField] private UIHookHotbar hotbarHook;
    [SerializeField] private GameObject fpMainHandRoot;
    [SerializeField] private GameObject fpOffHandRoot;

    private Item previousItem;

    private void Start()
    {
        hotbarHook.OnHotbarChangeEvent += OnHotbarChange;

        UpdateViewModel();
    }

    private void OnHotbarChange(object sender, UIHookHotbar.HotbarChangeEvent e)
    {
        UpdateViewModel();

        if (e.itemStack != null)
        {
            //  Relay the item name to the action bar
            if (previousItem != e.itemStack.GetItem()) UIMaster.SendText( UITextType.ACTION, e.itemStack.GetItem().GetDisplayName() );

            int viewCount = e.itemStack.GetAmount() / 8;
            if (viewCount < 1) viewCount = 1;
            Transform root = fpMainHandRoot.transform;

            float previousX = 0.0f;
            for (int i = 0; i < viewCount; i++)
            {
                GameObject obj = Instantiate(e.itemStack.GetItem().GetWorldModel(), Vector3.zero, Quaternion.identity, root);
                if (i == 0) root = obj.transform;

                obj.transform.localPosition = e.itemStack.GetItem().GetViewModel().transform.localPosition;
                obj.transform.localRotation = e.itemStack.GetItem().GetViewModel().transform.localRotation;
                obj.transform.localScale = e.itemStack.GetItem().GetViewModel().transform.localScale;

                if (viewCount > 1 && i > 0)
                {
                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localRotation = Quaternion.identity;
                    obj.transform.localScale = Vector3.one;

                    obj.transform.localPosition += new Vector3(
                        previousX += (0.2f / viewCount * 2),
                        Random.value * 0.5f - 0.25f,
                        Random.value * 0.5f - 0.25f
                        );
                }
            }
        }

        previousItem = e.itemStack?.GetItem();
    }

    protected override void UpdateViewModel()
    {
        base.UpdateViewModel();

        foreach (Transform t in fpMainHandRoot.transform)
        {
            Destroy(t.gameObject);
        }
    }
}
