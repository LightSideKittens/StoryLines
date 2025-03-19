﻿using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore.AnimationsModule.Animations.Options
{
    [Serializable]
    public struct SetEase : IOption
    {
        [SerializeField] private bool useCustom;

        [ShowIf(nameof(useCustom))]
        [SerializeField]
        private AnimationCurve curve;
        
        [HideIf(nameof(useCustom))]
        public Ease ease;
        
        public void ApplyTo(Tween tween)
        {
            if (useCustom)
            {
                tween.SetEase(curve);
            }
            else
            {
                tween.SetEase(ease);
            }
        }
    }
}