using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Swordfish
{

public class BuildingSnapPoint : MonoBehaviour
{
    public Vector3 origin;

    public List<BuildingStructureType> structureBlacklist;

    public void OnTriggerEnter(Collider other)
    {
        if (this.gameObject.activeInHierarchy && BuildingManager.IsBuilding() && other.gameObject.tag == "Structure")
        {
            BuildingManager.SnapAt(transform, origin);
        }
    }

    public void OnDrawGizmos()
	{
        Gizmos.color = Color.blue;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

        Gizmos.DrawWireSphere(origin, 0.25f);
	}
}

}