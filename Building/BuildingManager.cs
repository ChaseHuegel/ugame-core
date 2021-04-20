using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Swordfish
{

public class BuildingManager : Singleton<BuildingManager>
{

    [Header("References")]
    [SerializeField] protected GameObject structurePreview;

    [Header("Prefabs")]
    [SerializeField] protected List<GameObject> prefabs;

    [Header("Settings")]
    [SerializeField] protected float snapTolerance = 1.5f;
    [SerializeField] protected float buildReach = 1.0f;
    [SerializeField] protected float buildRange = 1.0f;
    [SerializeField] protected LayerMask buildOnMask;

    protected Transform snappedTarget;
    protected int selectedPrefabIndex = 0;
    protected bool isBuilding = false;
    protected bool isSnapped = false;
    protected bool canPlace = false;

    public static bool IsBuilding() { return Instance.isBuilding; }
    public static GameObject GetSelectedPrefab() { return Instance.prefabs[Instance.selectedPrefabIndex]; }

    public static void Unsnap() { Instance.isSnapped = false; }
    public static void SnapAt(Transform target, Vector3 offset)
    {
        if (Instance.isSnapped) return;  //  Don't resnap if we're already snapped

        Instance.structurePreview.transform.position = target.rotation * (offset + target.position);
        Instance.snappedTarget = target;
        Instance.isSnapped = true;
        Instance.canPlace = true;
    }

    public static void Place()
    {
        if (Instance.canPlace == false) return;

        Instance.isSnapped = false;
        Instance.canPlace = false;

        if (Instance.snappedTarget != null) Instance.snappedTarget.gameObject.SetActive(false);
        Instance.snappedTarget = null;

        GameObject obj = Instantiate(GetSelectedPrefab(), Instance.structurePreview.transform.position, Instance.structurePreview.transform.rotation);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            isBuilding = !isBuilding;

            structurePreview.SetActive(isBuilding);
        }

        if (isBuilding)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Place();
            }

            if (isSnapped && (Mathf.Abs(Input.GetAxis("Mouse X")) > snapTolerance || Mathf.Abs(Input.GetAxis("Mouse Y")) > snapTolerance))
            {
                isSnapped = false;
            }

            if (isSnapped == false)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, buildRange, buildOnMask) == true)
                {
                    canPlace = true;
                    structurePreview.transform.position = hit.point;
                    structurePreview.SetActive(true);
                }
                else
                {
                    canPlace = false;
                    structurePreview.SetActive(false);
                }
            }
        }
    }
}

}