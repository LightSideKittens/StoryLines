﻿using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace LSCore
{
    public static partial class LSAddressables
    {
        public static T Load<T>(this AssetRef<T> reference) where T : Object
        {
            return reference.IsValid() 
                ? reference
                : reference.LoadAssetAsync().WaitForCompletion();
        }
        
        public static T Load<T>(object key) where T : Object => Addressables.LoadAssetAsync<T>(key).WaitForCompletion();
    }
}