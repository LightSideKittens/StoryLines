using System;
using LSCore;

public class ErrorScreen : BaseWindow<ErrorScreen>
{
    public LocalizationText text;
    public LSButton button;
    public override WindowManager Manager { get; } = new NotRecordableWindowManager();

    public static void SetText(LocalizationData data)
    {
        Instance.text.SetLocalizationData(data);
    }
    
    public new static void Show()
    {
        Instance.Manager.OnlyShow();
        Instance.Internal_Show();
    }

    private void Internal_Show()
    {
        button.gameObject.SetActive(false);
    }
    
    public static void Show(Action retry)
    {
        Instance.Manager.OnlyShow();
        Instance.Internal_Show(retry);
    }

    public static void Hide()
    {
        Instance.Manager.OnlyHide();
    }

    private void Internal_Show(Action retry)
    {
        button.gameObject.SetActive(true);
        retry += Hide;
        button.Clicked = retry;
    }
}