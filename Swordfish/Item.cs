using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Swordfish
{

[CreateAssetMenu(fileName = "New Item", menuName = "Swordfish/Items/Generic")]
public class Item : ScriptableObject
{
    [SerializeField] private ItemType type = ItemType.GENERIC;
    [SerializeField] private int defaultStackSize = 1;

    [SerializeField] private Sprite thumbnail;
    [SerializeField] private GameObject viewModel;
    [SerializeField] private GameObject worldModel;

    [SerializeField] private string displayName = "Mysterious Object";
    [TextArea(15, 20)]
    [SerializeField] private string description = "Someone hasn't given this a description!";

    public ItemType GetItemType() { return type; }
    public int GetDefaultStackSize() { return defaultStackSize; }

    public string GetDisplayName() { return displayName; }
    public string GetDescription() { return description; }

    public Sprite GetThumbnail() { return thumbnail; }
    public GameObject GetViewModel() { return viewModel; }
    public GameObject GetWorldModel() { return worldModel; }
}

}