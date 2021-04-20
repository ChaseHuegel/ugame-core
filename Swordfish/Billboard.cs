using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Swordfish
{

public class Billboard : MonoBehaviour
{
    public bool invert = false;

    public void Update()
    {
        if (invert) transform.rotation = Quaternion.Euler( Camera.main.transform.rotation.eulerAngles.x, -Camera.main.transform.rotation.eulerAngles.y, Camera.main.transform.rotation.eulerAngles.z );
        else transform.rotation = Camera.main.transform.rotation;
    }
}

}