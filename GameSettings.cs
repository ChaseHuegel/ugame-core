using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Swordfish;

[CreateAssetMenu(fileName = "New Game Settings", menuName = "Swordfish/Settings/Game")]
public class GameSettings : ScriptableObject
{
    public RangeF ItemPickupDelay = new RangeF(0.3f, 1.0f);
}