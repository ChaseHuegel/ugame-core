using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Swordfish
{

public class UIBarElement : MonoBehaviour
{
    public RectTransform barTransform;
    public bool flipBar = false;
    public bool isVertical = false;
    public float minSize = 0;
    public float maxSize = 1;

    public void UpdateBar(float percent)
    {
        if (flipBar) percent = 1.0f - percent;   //  Flip the value if we want the bar to invert values
        float value = (maxSize - minSize) * percent + minSize;

        if (isVertical)
        {
            barTransform.sizeDelta = new Vector2(barTransform.rect.width, value);
        }
        else
        {
            barTransform.sizeDelta = new Vector2(value, barTransform.rect.height);
        }
    }
}

}