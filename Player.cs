using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Swordfish
{

public class Player : Entity
{
    public void DropItem(int slot, int amount = 0)
    {
        Vector3 pos = transform.position + (transform.forward * 3) + (Vector3.up * 2);

        if (amount == 0)
        {
            GameMaster.DropItem( pos, GetInventory().GetAt(slot).Clone() );
            GetInventory().RemoveAllAt(slot);
        }
        else
        {
            GameMaster.DropItem( pos, GetInventory().GetAt(slot).GetItem(), amount );
            GetInventory().RemoveAt(slot, amount);
        }
    }
}

}