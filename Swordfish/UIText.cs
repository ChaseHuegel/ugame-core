using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Swordfish
{

public enum UITextType
{
    NOTIFICATION,
    SUBTEXT,
    ACTION,
    TITLE,
    CHAT
}

public class UIText
{
    public UITextType type = UITextType.NOTIFICATION;
    public string text = "";

    public UIText(UITextType type, string text)
    {
        this.type = type;
        this.text = text;
    }
}

}