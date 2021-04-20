using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Swordfish
{

public class SelfDestructTimer : MonoBehaviour
{
    [SerializeField] private float maxLifetime = 1.0f;

    private float lifetime = 0f;

    public void Update()
    {
        if (lifetime >= maxLifetime) Destroy(this.gameObject);

        lifetime += Time.deltaTime;
    }
}

}