using System;
using LSCore;
using LSCore.Attributes;
using LSCore.ConfigModule;
using LSCore.DataStructs;
using Newtonsoft.Json.Linq;
using StoryWorkshop;
using UnityEngine;

public class StoryWorld : ServiceManager
{
    [Unwrap]
    [Serializable]
    public class LoadScene : LoadSceneAction
    {
#if UNITY_EDITOR
        protected override bool HideOnSuccessActions => true;
#endif

        public override int Hash => HashCode.Combine(typeof(LoadSceneAction).FullName!.GetHashCode(), sceneRef.GetHashCode());
    }
    
    public string configName;

    public static JToken Config => JTokenGameConfig.Get(Instance.configName);
    
    public UniDict<string, LoadScene> sceneByBranchId;
    public static UniDict<string, LoadScene> SceneByBranchId => Instance.sceneByBranchId;
    private static StoryWorld Instance { get; set; }

    [SerializeField] private GameObject branches;

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
        branches.SetActive(true);
    }

    private void Start()
    {
        StoryWindow.Show();
        foreach (var loadScene in sceneByBranchId.Values)
        {
            loadScene.Preload();
        }
    }

    public static void SetAction(int actionHash, string actionId)
    {
        var actions = (JObject)(Config["actions"] ??= new JObject());
        actions[actionHash.ToString()] = actionId;
    }
    
    public static void RemoveAction(int actionHash)
    {
        var actions = (JObject)(Config["actions"] ??= new JObject());
        actions.Remove(actionHash.ToString());
    }

    public static bool TryGetAction(int actionHash, out string actionId)
    {
        var actions = Config["actions"];
        actionId = actions?[actionHash.ToString()]?.ToString();
        return actionId != null;
    }

    public static void SetLastBranchId(string actionId, string lastBranchId)
    {
        var ids = (JObject)(Config["lastBranchIds"] ??= new JObject());
        ids[actionId] = lastBranchId;
    }

    public static bool TryGetLastBranchId(string actionId, out string lastBranchId)
    {
        var ids = Config["lastBranchIds"];
        lastBranchId = ids?[actionId]?.ToString();
        return lastBranchId != null;
    }
}