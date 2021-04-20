using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Swordfish
{

public class Interactable : MonoBehaviour
{
   #region Events

    public event EventHandler<InteractableEvent> OnInteractEvent;
    public class InteractableEvent : Event
    {
        public Entity entity;
        public Interactable interactable;
        public Transform transform;
    }
    #endregion

    #region Variables

    [Header("Interactable")]
    [SerializeField] protected bool canInteract = true;
    [SerializeField] protected string displayText = "";
    #endregion

    #region Functions

    public string GetDisplayText() { return displayText; }

    public void Interact(Entity interactor = null)
    {
        //  Invoke an interact event
        InteractableEvent e = new InteractableEvent{ entity = interactor, interactable = this, transform = this.transform };
        OnInteractEvent?.Invoke(this, e);
        if (e.cancel == true) { return; }   //  return if the event has been cancelled by any subscriber
    }
    #endregion
}

}