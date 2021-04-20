using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Swordfish;

public enum UIState
{
    NONE,
    FOCUSED
}

public class UIMaster : Singleton<UIMaster>
{
    [Header("Info")]
    [SerializeField] private UIState state = UIState.NONE;

    [Header("Prefabs")]
    [SerializeField] private GameObject floatingIndicatorPrefab;

    [Header("References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasScaler scaler;

    [SerializeField] private List<UIHook> hooks;

    private List<UIText> textToSend = new List<UIText>();

    private void Update()
    {
        SetState(UIState.NONE);
        foreach (UIHook hook in hooks)
        {
            hook.Handle();
            if (hook.IsBlockingInput()) SetState(UIState.FOCUSED);
        }
    }

    public static UIState GetState() { return Instance.state; }
    public static void SetState(UIState state) { Instance.state = state; }

    public static CanvasScaler GetScaler() { return Instance.scaler; }
    public static void GetScaler(CanvasScaler scaler) { Instance.scaler = scaler; }

    public static Canvas GetCanvas() { return Instance.canvas; }
    public static void GetCanvas(Canvas canvas) { Instance.canvas = canvas; }

    public static void SendText(UITextType type, string text) { Instance.textToSend.Add(new UIText(type, text)); }
    public static List<UIText> GetWaitingTexts() { return Instance.textToSend; }

    public static void SendFloatingIndicator(Vector3 pos, string text, Color color)
    {
        GameObject obj = Instantiate(Instance.floatingIndicatorPrefab, pos, Quaternion.identity);
        obj.GetComponent<TextMesh>().text = text;
        obj.GetComponent<TextMesh>().color = color;
    }

    public static UIHook GetHook(string identifier)
    {
        foreach (UIHook hook in Instance.hooks)
        {
            if (hook.GetIdentifier() == identifier)
            {
                return hook;
            }
        }

        return null;
    }
}
