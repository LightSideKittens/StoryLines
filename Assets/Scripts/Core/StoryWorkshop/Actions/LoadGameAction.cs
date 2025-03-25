using System;
using LSCore;
using Sirenix.OdinInspector;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace StoryWorkshop
{
    [Serializable]
    public class LoadGameAction : BaseStoryAction
    {
        public bool unloadCurrentScene;
        
        [HideIf("unloadCurrentScene")]
        public LSAssetReference sceneRef;
        private AsyncOperationHandle<SceneInstance> sceneHandle;
        private static LoadGameAction current;

        protected override bool ShouldDeleteActionOnExit => true;

        public override void Preload()
        {
            base.Preload();
            Addressables.DownloadDependenciesAsync(sceneRef, true);
        }

        protected override void OnInvoke()
        {
            if (unloadCurrentScene)
            {
                current.Discard();
            }
            else
            {
                LoadingScreen.ShowLoop();
                sceneHandle = Addressables.LoadSceneAsync(sceneRef, LoadSceneMode.Additive).OnComplete(x =>
                {
                    LoadingScreen.Hide();
                
                    if (!x.Scene.IsValid())
                    {
                        ErrorScreen.Show(Invoke);
                    }
                });
                current = this;
            }
        }

        public override void Discard()
        {
            base.Discard();
            Addressables.UnloadSceneAsync(sceneHandle);
        }
    }
}