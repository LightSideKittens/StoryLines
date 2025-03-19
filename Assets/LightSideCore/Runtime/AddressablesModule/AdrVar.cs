﻿using LSCore;
using UnityEngine.Scripting;

public static class AdrVar
{
#if DEBUG
    [Preserve] public static string BuildEnvironment => LSDebugData.Data.Environment;
#endif

    private static string serverName;

    [Preserve]
    public static string ServerName
    {
        get
        {
            return serverName
#if DEBUG
                + $"/{LSDebugData.Data.Environment}"
#else
                + $"/{LSConsts.Env.Prod}"
#endif
                ;
        }
        set => serverName = value;
    }
}