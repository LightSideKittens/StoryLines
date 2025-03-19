﻿using System;
using System.Collections.Generic;
using DG.Tweening;
using LSCore.AnimationsModule.Animations.Options;
using LSCore.Extensions.Unity;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LSCore.AnimationsModule.Animations
{
    [Serializable]
    public abstract class BaseAnim
    { 
        [BoxGroup] [LabelText("ID")] public string id;
        [HideIf("IsDurationZero")]
        [SerializeReference] private List<IOption> mainOptions;

        public abstract bool NeedInit { get; set; }
        public abstract float Duration { get; set; }
        
        public Tween Anim { get; private set; }
        public bool IsDurationZero => Duration == 0;
        private bool HideDuration => GetType() == typeof(AlphaAnim);

        public void TryInit()
        {
            if (NeedInit || IsDurationZero)
            {
                Internal_Init();
            }
        }

        protected abstract void Internal_Init();
        
        protected abstract Tween Internal_Animate();
        
        public Tween Animate()
        {
            TryInit();
            Anim = IsDurationZero ? DOTween.Sequence() : ApplyOptions(Internal_Animate(), mainOptions);
            return Anim;
        }

        protected static Tween ApplyOptions(Tween tween, List<IOption> options)
        {
            if (options is {Count: > 0})
            {
                for (int i = 0; i < options.Count; i++)
                {
                    options[i].ApplyTo(tween);
                }
            }

            return tween;
        }
        
        protected virtual void Bind(){}
        protected virtual void UnbindAll(){}

        public void ResolveBinds<T>(string key, T target)
        {
            Bind();
            Binder<T>.Resolve(GetBindKey($"${key}"), target);
            UnbindAll();
        }

        protected string GetBindKey(string key) => $"{key}_{GetHashCode()}";
    }

    public static class Binder<TTarget>
    {
        private static readonly Dictionary<string, Action<TTarget>> binds = new();

        public static void Bind(string key, Action<TTarget> action)
        {
            binds.TryGetValue(key, out var existAction);
            existAction += action;
            binds[key] = existAction;
        }
        
        public static void Unbind(string key, Action<TTarget> action)
        {
            binds.TryGetValue(key, out var existAction);
            existAction -= action;
            binds[key] = existAction;
        }

        public static void Resolve(string key, TTarget target)
        {
            if (binds.TryGetValue(key, out var action))
            {
                action(target);
            }
        }
    }
    
    [Serializable]
    public abstract class BaseAnim<T, TTarget> : BaseAnim, ISerializationCallbackReceiver where TTarget : Object
    {
        [field: SerializeField] public override bool NeedInit { get; set; }

        [field: HideIf("HideDuration")]
        [field: SerializeField] public override float Duration { get; set; }
        
        [ShowIf("ShowStartValue")]
        public T startValue;
        
        [ShowIf("ShowEndValue")]
        public T endValue;
        
        public bool useTargetPath;
        public bool useMultiple;
        
        [HideIf("@IsDurationZero || !useMultiple")]
        [SerializeReference] public List<IOption> options;
        [HideIf("ShowTargets")] public TTarget target;
        [ShowIf("ShowTargets")] public List<TTarget> targets;
        
        [ShowIf("useTargetPath")] public Transform root;
        [HideIf("ShowTargetsPaths")] public string targetPath;
        [ShowIf("ShowTargetsPaths")] public List<string> targetsPaths;
        
        [ShowIf("useMultiple")] public float timeOffsetPerTarget = 0.1f;

        public List<Tween> Tweens { get; private set; }
        private bool ShowTargets => useMultiple && !useTargetPath;
        private bool ShowTargetsPaths => useMultiple && useTargetPath;
        protected virtual bool ShowStartValue => NeedInit;
        protected virtual bool ShowEndValue => !IsDurationZero;
        
        protected abstract void InitAction(TTarget target);
        protected abstract Tween AnimAction(TTarget target);
        
        protected override void Internal_Init()
        {
            if (useMultiple)
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    InitAction(targets[i]);
                }
                
                return;
            }
            
            InitAction(target);
        }

        protected override Tween Internal_Animate()
        {
            Tweens ??= new();
            Tweens.Clear();
            
            if (useMultiple)
            {
                var sequence = DOTween.Sequence();
                var pos = 0f;
                
                for (int i = 0; i < targets.Count; i++)
                {
                    var t = ApplyOptions(AnimAction(targets[i]), options);
                    Tweens.Add(t);
                    sequence.Insert(pos, t);
                    pos += timeOffsetPerTarget;
                }

                return sequence;
            }
            
            return ApplyOptions(AnimAction(target), options);
        }

        public void Reverse()
        {
            (startValue, endValue) = (endValue, startValue);
        }

        public virtual void OnBeforeSerialize() { }

        public virtual void OnAfterDeserialize()
        {
            if(World.IsEditMode) return;
            if (useTargetPath)
            {
                targets = new List<TTarget>();
                if (typeof(Component).IsAssignableFrom(typeof(T)))
                {
                    if (targetPath[0] == '$')
                    {
                        Bind(targetPath, t => target = t);
                    }
                    else
                    {
                        target = root.FindComponent<TTarget>(targetPath);
                    }

                    for (int i = 0; i < targetsPaths.Count; i++)
                    {
                        var path = targetsPaths[i];
                        if (path[0] == '$')
                        {
                            var index = i;
                            Bind(path, t => targets.Add(t));
                        }
                        else
                        {
                            targets.Add(root.FindComponent<TTarget>(path));
                        }
                    }
                }
                else
                {
                    Bind();
                }
            }
        }

        protected sealed override void Bind()
        {
            if (useTargetPath)
            {
                if (targetPath[0] == '$')
                {
                    Bind(targetPath, t => target = t);
                }

                for (int i = 0; i < targetsPaths.Count; i++)
                {
                    var path = targetsPaths[i];
                    if (path[0] == '$')
                    {
                        var index = i;
                        Bind(path, t => targets[index] = t);
                    }
                }
            }
        }
        
        private List<(string key, Action<TTarget> action)> binds;
        
        protected sealed override void UnbindAll()
        {
            foreach (var (key, action) in binds)
            {
                Binder<TTarget>.Unbind(key, action);
            }
            
            binds.Clear();
        }
        
        private void Bind(string key, Action<TTarget> action)
        {
            key = GetBindKey(key);
            binds ??= new List<(string, Action<TTarget>)>();
            binds.Add((key, action));
            Binder<TTarget>.Bind(key, action);
        }
    }
}