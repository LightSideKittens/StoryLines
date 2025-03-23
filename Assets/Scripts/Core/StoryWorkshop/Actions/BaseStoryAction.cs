using System;
using System.Collections.Generic;
using LSCore;
using UnityEngine;

namespace StoryWorkshop
{
    [Serializable]
    public abstract class BaseStoryAction : LSAction, ISerializationCallbackReceiver
    {
        [HideInInspector] public string id;
        public static Dictionary<string, BaseStoryAction> actionById = new();
        private static HashSet<string> calledIds = new();

        protected virtual bool ShouldSaveAction => true;
#if UNITY_EDITOR
        static BaseStoryAction()
        {
            World.Destroyed += () =>
            {
                actionById.Clear();
                calledIds.Clear();
            };
        }
#endif
        
        public sealed override void Invoke()
        {
            if (ShouldSaveAction)
            {
                StoryWorld.SetAction(Hash, id);
            }
            OnInvoke();
        }

        protected abstract void OnInvoke();
        
        public virtual void Discard()
        {
            
        }
        
        public virtual void Preload()
        {
            if (ShouldSaveAction)
            {
                if (StoryWorld.TryGetAction(Hash, out var actionId))
                {
                    if (actionById.TryGetValue(actionId, out var action) && calledIds.Add(actionId) )
                    {
                        action.Invoke();
                    }
                }
            }
        }

        public virtual void Unload()
        {
            
        }

        public virtual int Hash => GetType().FullName!.GetHashCode();
        
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            id ??= Guid.NewGuid().ToString("N");
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            actionById[id] = this;
        }
    }
}