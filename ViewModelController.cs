using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Swordfish;

public class ViewModelController : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private Entity inventoryHolder;
    [SerializeField] private Inventory inventory;

    [Header("View Model")]
    [SerializeField] private GameObject offHandRoot;
    [SerializeField] private GameObject mainHandRoot;

    protected virtual void UpdateViewModel()
    {

    }
}
