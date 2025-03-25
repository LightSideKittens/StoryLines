using System;
using LSCore;
using LSCore.Attributes;
using LSCore.ConfigModule;
using LSCore.DataStructs;
using LSCore.Extensions;
using Newtonsoft.Json.Linq;
using StoryWorkshop;

public class StoryWorld : ServiceManager
{
    [Unwrap]
    [Serializable]
    public class LoadScene : LoadSceneAction
    {
#if UNITY_EDITOR
        protected override bool HideOnSuccessActions => true;
#endif

        public override int Hash => typeof(LoadSceneAction).FullName!.GetHashCode() ^ sceneRef.GetHashCode();
    }
    
    
    public string configName;

    public static JToken Config => JTokenGameConfig.Get(Instance.configName);
    
    public SetBranchAction startBranchAction;
    public UniDict<string, LoadScene> sceneByBranchId;
    public static UniDict<string, LoadScene> SceneByBranchId => Instance.sceneByBranchId;
    private static StoryWorld Instance { get; set; }


    protected override void Awake()
    {
        base.Awake();
        Instance = this;
    }

    private void Start()
    {
        StoryWindow.Show();
        LoadScene lastScene = null;
        foreach (var loadScene in sceneByBranchId.Values)
        {
            loadScene.Preload();
            if (TryGetAction(loadScene.Hash, out _))
            {
                if (lastScene == null || lastScene.sceneRef != loadScene.sceneRef)
                {
                    lastScene = loadScene;
                }
            }
        }
        
        if (lastScene != null)
        {
            lastScene.onSuccess.Add((DelegateLSAction)SetBranch);
        }
        else
        {
            SetBranch();
        }


        void SetBranch()
        {
            if (!StoryBranch.TryLoadFromConfig())
            {
                startBranchAction?.Invoke();
            }
        }
    }

    public static JObject GetJObject(string propertyName) => Config.GetOrCreate<JObject>(propertyName);
    
    public static void SetAction(int actionHash, string actionId)
    {
        var actions = GetJObject("actions");
        actions[actionHash.ToString()] = actionId;
    }
    
    public static void RemoveAction(int actionHash)
    {
        var actions = GetJObject("actions");
        actions.Remove(actionHash.ToString());
    }

    public static bool TryGetAction(int actionHash, out string actionId)
    {
        var actions = GetJObject("actions");
        actionId = actions?[actionHash.ToString()]?.ToString();
        return actionId != null;
    }

    public static void SetLastBranchId(string actionId, string lastBranchId)
    {
        var ids = GetJObject("lastBranchIds");
        ids[actionId] = lastBranchId;
    }

    public static bool TryGetLastBranchId(string actionId, out string lastBranchId)
    {
        var ids = GetJObject("lastBranchIds");
        lastBranchId = ids?[actionId]?.ToString();
        return lastBranchId != null;
    }
}