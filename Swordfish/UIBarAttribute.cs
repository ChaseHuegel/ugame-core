using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Swordfish;

public class UIBarAttribute : UIBarElement
{
    public Entity targetEntity;
    public Attributes attribute;

    private void Start()
    {
        UpdateBar( targetEntity.GetAttributeValue(attribute) / targetEntity.GetAttributeMax(attribute) );
    }

    private void LateUpdate()
    {
        UpdateBar( targetEntity.GetAttributeValue(attribute) / targetEntity.GetAttributeMax(attribute) );
    }
}
