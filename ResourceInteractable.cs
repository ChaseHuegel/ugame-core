using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Swordfish;

public class ResourceInteractable : Interactable
{
    public GameObject dropPrefab = null;
    public int dropPrefabHeight = 1;
    public Item drop;
    public RangeI amountRange;

    private void Start()
    {
        OnInteractEvent += OnInteract;
    }

    private void OnInteract(object sender, InteractableEvent e)
    {
        if (dropPrefab != null) Instantiate(dropPrefab, transform.position, transform.rotation);

        if (drop != null)
        {
            int amount = amountRange.RandomValue();

            for (int i = 0; i < amount; i++) GameMaster.DropItemNaturally(transform.position, drop);
        }

        Destroy(this.gameObject);
    }
}
