using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Swordfish;

[RequireComponent(typeof(Text))]
public class UIHookTextReciever : UIHook
{
    [Header("Notifcation")]
    [SerializeField] private UITextType type;
    [SerializeField] private bool useQueue = true;
    [SerializeField] private float duration = 1.0f;
    [SerializeField] private Text textElement;

    private float lifeTime = 0f;

    public override void Handle()
    {
        if (useQueue)
        {
            //  Tick lifetime if active
            if (IsActive())
            {
                if (lifeTime >= duration)
                {
                    Disable();
                }

                lifeTime += Time.deltaTime;
            }

            //  If next in queue is a matching type, pull it
            if (UIMaster.GetWaitingTexts().Count > 0 && UIMaster.GetWaitingTexts()[0].type == type)
            {
                lifeTime = 0f;

                textElement.text = UIMaster.GetWaitingTexts()[0].text;
                UIMaster.GetWaitingTexts().RemoveAt(0);

                Enable();
            }
        }
        else
        {
            for (int i = 0; i < UIMaster.GetWaitingTexts().Count; i++)
            {
                UIText waitingText = UIMaster.GetWaitingTexts()[i];

                if (waitingText.type == type)
                {
                    textElement.text = waitingText.text;
                    UIMaster.GetWaitingTexts().RemoveAt(i);

                    Enable();
                    return;
                }
            }

            //  Fallback to disabling
            Disable();
        }
    }
}
