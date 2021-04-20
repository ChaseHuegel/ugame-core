using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Swordfish
{

public enum BuildingStructureType
{
    FOUNDATION,
    FLOOR,
    WALL,
    DECORATION
}

public class BuildingStructure : MonoBehaviour
{
    public BuildingStructureType type = BuildingStructureType.FOUNDATION;
}

}