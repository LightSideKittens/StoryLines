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

        [NonSerialized] public string branchId;
        
        public static Dictionary<string, BaseStoryAction> actionById = new();
        private static HashSet<string> calledIds = new();

        protected virtual bool ShouldSaveAction => true;
        protected virtual bool ShouldDeleteActionOnExit => false;
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
            calledIds.Add(id);
            
            if (ShouldSaveAction)
            {
                StoryWorld.SetAction(Hash, id);
            }
            
            OnInvoke();
        }

        public virtual void Exit()
        {
            if (ShouldDeleteActionOnExit)
            {
                StoryWorld.RemoveAction(Hash);
            }
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
                    if (actionById.TryGetValue(actionId, out var action) && calledIds.Add(actionId))
                    {
                        action.Invoke();
                    }
                }
            }
        }

        public virtual void Unload()
        {
            actionById.Remove(id);
        }

        public virtual int Hash => GetType().FullName!.GetHashCode();
        
        void ISerializationCallbackReceiver.OnBeforeSerialize() => id ??= Guid.NewGuid().ToString("N");
        void ISerializationCallbackReceiver.OnAfterDeserialize() => actionById[id] = this;
    }
}