using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Swordfish
{

public class ConstantMovement : MonoBehaviour
{
    [SerializeField] protected Vector3 unitsPerSecond;
    [SerializeField] protected Vector3 variation;
    [SerializeField] protected bool bidirectionalVariation = false;

    private Vector3 movement;

    public void Start()
    {
        movement = unitsPerSecond;

        if (variation.x > 0) movement.x += Random.Range( bidirectionalVariation ? -variation.x : 0, variation.x );
        if (variation.y > 0) movement.y += Random.Range( bidirectionalVariation ? -variation.y : 0, variation.y );
        if (variation.z > 0) movement.z += Random.Range( bidirectionalVariation ? -variation.z : 0, variation.z );
    }

    public void Update()
    {
        this.transform.Translate(movement * Time.deltaTime, Space.Self);
    }
}

}