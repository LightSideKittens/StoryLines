using System;
using LSCore;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace StoryWorkshop
{
    [Serializable]
    public abstract class BaseSingleGameObjectSetter<T> : BaseStoryAction where T : BaseSingleGameObjectSetter<T>, new()
    {
        public GameObject target;
        private T prevCharacter;
        private static T lastCharacter;

        protected abstract T This { get; }
        protected abstract string ConfigPropertyName { get; }
        
        private T PrevCharacter
        {
            get 
            {
                if (TryGetPrevId(out var prev))
                {
                    var prevId = prev["id"]!.ToString();
                    
                    if (actionById.TryGetValue(prevId, out var action))
                    {
                        prevCharacter = (T)action;
                    }
                    else
                    {
                        prevCharacter = new T()
                        {
                            id = prevId,
                            branchId =  prev["branchId"]!.ToString()
                        };
                    }
                }
                
                return prevCharacter;
            }
            set
            {
                prevCharacter = value;
            }
        }

#if UNITY_EDITOR

        static BaseSingleGameObjectSetter()
        {
            World.Creating += () => lastCharacter = null;
        }
#endif

        public override void Preload()
        {
            target.SetActive(false);
            base.Preload();

            if (lastCharacter?.target == target)
            {
                target.SetActive(true);
            }
        }

        protected override void OnInvoke()
        {
            if(lastCharacter == this) return;
            
            if (lastCharacter != null)
            {
                prevCharacter = lastCharacter;
                if (prevCharacter.target != null)
                {
                    prevCharacter.target.SetActive(false);
                }
                SetPrevId(prevCharacter);
            }
            
            target.SetActive(true);
            lastCharacter = This;
        }

        public override void Discard()
        {
            base.Discard();
            var p = PrevCharacter;
            if (p != null)
            {
                if (!StoryBranch.IsBranchExist(p.branchId))
                {
                    if (StoryWorld.SceneByBranchId.TryGetValue(p.branchId, out var loadSceneAction))
                    {
                        loadSceneAction.onSuccess.Clear();
                        loadSceneAction.onSuccess.Add((DelegateLSAction)Set);
                        loadSceneAction.Invoke();
                    }
                }
                else
                {
                    Set();
                }

                void Set()
                {
                    p = PrevCharacter;
                    target.SetActive(false);
                    lastCharacter = p.PrevCharacter;
                    p.Invoke();
                }
            }
        }

        private void SetPrevId(T prevAction)
        {
            var ids = StoryWorld.GetJObject(ConfigPropertyName);
            ids[id] = new JObject()
            {
                {"id", prevAction.id},
                {"branchId", prevAction.branchId},
            };
        }

        private bool TryGetPrevId(out JToken prevAction)
        {
            var ids = StoryWorld.Config[ConfigPropertyName];
            prevAction = ids?[id];
            return prevAction != null;
        }
    }
}