using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Swordfish;

public class PlayerBobController : GentleBob
{
    public Entity player;

    private void Update()
    {
        bobSpeed = 0.0f;

        if (player != null && player.GetCurrentSpeed() > 0)
        {
            bobSpeed = player.GetCurrentSpeed() + 1;
            positionStrength.x = player.GetCurrentSpeed() * 0.01f;
        }
    }
}
