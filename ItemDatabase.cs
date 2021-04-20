using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Swordfish;

[CreateAssetMenu(fileName = "New Database", menuName = "Swordfish/Database/Items")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<Item> database = new List<Item>();

    public Item Get(string name)
    {
        foreach (Item item in database)
        {
            if (item.name == name)
            {
                return item;
            }
        }

        return new Item();
    }
}