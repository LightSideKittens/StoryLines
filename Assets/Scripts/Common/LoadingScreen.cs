﻿using System;
using LSCore;

public class LoadingScreen : BaseWindow<LoadingScreen>
{
    public LSSlider slider;
    public override WindowManager Manager { get; } = new NotRecordableWindowManager();
    
    public static void ShowLoop()
    {
        Instance.Manager.OnlyShow();
        Instance.Internal_ShowLoop();
    }
    
    public static void ShowPercentProgress(out Action<float> progress)
    {
        Instance.Manager.OnlyShow();
        Instance.Internal_ShowPercentProgress(out progress);
    }
    
    public static void ShowProgress(float maxValue, out Action<float> progress)
    {
        Instance.Manager.OnlyShow();
        Instance.Internal_ShowProgress(maxValue, out progress);
    }
    
    public static void Hide()
    {
        Instance.Manager.OnlyHide();
    }

    private void Internal_ShowLoop()
    {
        slider.gameObject.SetActive(false);
    }
    
    private void Internal_ShowPercentProgress(out Action<float> progress)
    {
        slider.gameObject.SetActive(true);
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.TextModee = LSSlider.TextMode.NormalizedPercent;
        progress = x =>
        {
            slider.normalizedValue = x;
        };
    }
    
    private void Internal_ShowProgress(float maxValue, out Action<float> progress)
    {
        slider.gameObject.SetActive(true);
        slider.minValue = 0;
        slider.maxValue = maxValue;
        slider.TextModee = LSSlider.TextMode.ValueToMax;
        progress = x =>
        {
            slider.value = x;
        };
    }
}