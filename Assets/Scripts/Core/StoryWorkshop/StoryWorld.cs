using LSCore;
using LSCore.ConfigModule;
using LSCore.DataStructs;
using Newtonsoft.Json.Linq;

public class StoryWorld : ServiceManager
{
    public string configName;

    public static JToken Config => JTokenGameConfig.Get(Instance.configName);
    
    public UniDict<string, LSAssetReference> sceneByBranchId;
    public static UniDict<string, LSAssetReference> SceneByBranchId => Instance.sceneByBranchId;
    private static StoryWorld Instance { get; set; }

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
    }

    private void Start()
    {
        StoryWindow.Show();
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