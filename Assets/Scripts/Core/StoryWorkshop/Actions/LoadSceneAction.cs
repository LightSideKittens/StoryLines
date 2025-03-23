using System;
using System.Collections.Generic;
using LSCore;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace StoryWorkshop
{
    [Serializable]
    public class LoadSceneAction : BaseStoryAction
    {
        public LSAssetReference sceneRef;
        [HideIf("HideOnSuccessActions")] [SerializeReference] public List<LSAction> onSuccess = new();
        
        private SceneInstance sceneInstance;
        private static AsyncOperationHandle<SceneInstance> handler;
        private static SceneInstance currentScene;
        private static Dictionary<LSAssetReference, SceneInstance> scenes = new();
        private static LinkedList<LoadSceneAction> scenesList = new();
        
        
#if UNITY_EDITOR

        protected virtual bool HideOnSuccessActions => false;
        static LoadSceneAction()
        {
            World.Creating += () =>
            {
                scenes.Clear();
                scenesList.Clear();
                currentScene = default;
                handler = default;
            };
        }
#endif
        public override void Preload()
        {
            if (handler.IsValid())
            {
                handler.OnComplete(base.Preload);
            }
            else
            {
                base.Preload();
            }
            
            Addressables.DownloadDependenciesAsync(sceneRef, true);
        }

        protected override void OnInvoke()
        {
            if (scenes.ContainsKey(sceneRef))
            {
                onSuccess?.Invoke();
                return;
            }
            
            LoadingScreen.ShowLoop();
            handler = Addressables.LoadSceneAsync(sceneRef, LoadSceneMode.Additive);
            handler.Completed += x =>
            {
                handler = default;
                LoadingScreen.Hide();
                
                if (x.Status == AsyncOperationStatus.Failed)
                {
                    ErrorScreen.Show(Invoke);
                    return;
                }

                sceneInstance = x.Result;
                currentScene = sceneInstance;
                
                if (scenes.TryAdd(sceneRef, currentScene))
                {
                    if (StoryBranch.IsNext)
                    {
                        scenesList.AddLast(this);
                    }
                    else
                    {
                        scenesList.AddFirst(this);
                    }
                }
                
                if (scenes.Count > 3)
                {
                    LoadSceneAction value;
                    if (StoryBranch.IsNext)
                    {
                        value = scenesList.First.Value;
                        scenesList.RemoveFirst();
                    }
                    else
                    {
                        value = scenesList.Last.Value;
                        scenesList.RemoveLast();
                    }

                    StoryWorld.RemoveAction(value.Hash);
                    Addressables.UnloadSceneAsync(value.sceneInstance);
                    scenes.Remove(value.sceneRef);
                }

                onSuccess?.Invoke();
            };
        }
    }
}