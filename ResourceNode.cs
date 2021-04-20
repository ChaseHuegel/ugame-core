using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Swordfish;

public class ResourceNode : Damageable
{
    public GameObject onBreakPrefab = null;
    public GameObject dropPrefab = null;
    public int dropPrefabHeight = 1;
    public Item drop;
    public RangeI amountRange;

    private void Start()
    {
        OnDeathEvent += OnBreak;
    }

    private void OnBreak(object sender, DeathEvent e)
    {
        if (onBreakPrefab != null) Instantiate(onBreakPrefab, transform.position, transform.rotation);

        Vector3 offset = new Vector3(
                UnityEngine.Random.value * 2f,
                UnityEngine.Random.value * 2f,
                UnityEngine.Random.value * 2f
                );

        if (dropPrefab != null) Instantiate(dropPrefab, transform.position + new Vector3(0, dropPrefabHeight, 0), Quaternion.Euler(transform.rotation.eulerAngles + offset));

        if (drop != null)
        {
            int amount = amountRange.RandomValue();

            for (int i = 0; i < amount; i++) GameMaster.DropItemNaturally(transform.position, drop);
        }

        Destroy(this.gameObject);
    }
}
