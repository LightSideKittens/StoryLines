using System;
using System.Collections.Generic;
using LSCore;
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
        [SerializeReference] public List<LSAction> onSuccess = new();
        
        private SceneInstance sceneInstance;
        private static AsyncOperationHandle<SceneInstance> handler;
        private static SceneInstance currentScene;
        private static Dictionary<LSAssetReference, SceneInstance> scenes = new();
        private static LinkedList<(LSAssetReference, SceneInstance)> scenesList = new();
        
        
#if UNITY_EDITOR
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
                        scenesList.AddLast((sceneRef, currentScene));
                    }
                    else
                    {
                        scenesList.AddFirst((sceneRef, currentScene));
                    }
                }
                
                if (scenes.Count > 3)
                {
                    (LSAssetReference, SceneInstance) value;
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
                    
                    Addressables.UnloadSceneAsync(value.Item2);
                    scenes.Remove(value.Item1);
                }

                onSuccess?.Invoke();
            };
        }
    }
}