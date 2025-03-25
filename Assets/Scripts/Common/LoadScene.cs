using System;
using LSCore;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Common
{
    [Serializable]
    public class LoadScene : LSAction
    {
        public static LoadScene metaLoadScene;
        
        public LSAssetReference sceneRef;
        private AsyncOperationHandle<SceneInstance> sceneHandle;
        
        public override void Invoke()
        {
            LoadingScreen.ShowLoop();
            sceneHandle = Addressables.LoadSceneAsync(sceneRef).OnComplete(x =>
            {
                LoadingScreen.Hide();
                
                if (!x.Scene.IsValid())
                {
                    ErrorScreen.Show(Invoke);
                }
            });
        }
    }
}