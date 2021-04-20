using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Swordfish
{

public class UIHook : MonoBehaviour
{
    [SerializeField] private string identifier = "";
    public string GetIdentifier() { return identifier; }

    protected bool blockInput = false;
    public bool IsBlockingInput() { return blockInput; }

    public void Toggle()
    {
        gameObject.SetActive( !gameObject.activeInHierarchy );
    }

    public void Enable()
    {
        gameObject.SetActive(true);
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }

    public bool IsActive()
    {
        return gameObject.activeInHierarchy;
    }

    public virtual void Handle()
    {

    }
}

}