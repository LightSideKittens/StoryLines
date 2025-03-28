﻿using System;
using System.Collections.Generic;
using LSCore;
using LSCore.Extensions;
using Newtonsoft.Json.Linq;
using StoryWorkshop;
using UnityEngine;

public class StoryBranch : MonoBehaviour
{
    [Serializable]
    public class Step
    {
        [SerializeReference] public List<BaseStoryAction> onEnter;
        [SerializeReference] public List<BaseStoryAction> onNext;
        [NonSerialized] public string branchId;
        
        public void OnNext()
        {
            onNext.Invoke();
        }

        public void Enter()
        {
            onEnter.Invoke();
        }

        public void Discard()
        {
            foreach (var action in onEnter)
            {
                action.Discard();
            }
        }

        public void Preload()
        {
            foreach (var action in onEnter)
            {
                action.branchId = branchId;
                action.Preload();
            }
            
            foreach (var action in onNext)
            {
                action.branchId = branchId;
                action.Preload();
            }
        }

        public void Unload()
        {
            foreach (var action in onEnter)
            {
                action.Unload();
            }
            
            foreach (var action in onNext)
            {
                action.Unload();
            }
        }

        public void Exit()
        {
            foreach (var action in onEnter)
            {
                action.Exit();
            }
        }
    }

    public string id;
    public List<Step> steps;
    private bool IsValidIndex => CurrentIndex >= 0 && CurrentIndex < steps.Count;
    private int currentIndex;

    private int CurrentIndex
    {
        get => currentIndex;
        set
        {
            currentIndex = value;
            SaveBranchData();
        }
    }
    
    private static Dictionary<string, StoryBranch> branchById = new();
    private static StoryBranch activeBranch;
    private SetBranchAction backAction;
    private bool isLoaded;
    
#if UNITY_EDITOR
    static StoryBranch()
    {
        World.Creating += () =>
        {
            branchById.Clear();
            activeBranch = default;
        };
    }
#endif
    
    public static string ActiveBranchId => activeBranch?.id;
    public static bool IsNext { get; private set; }

    public static bool TryLoadFromConfig()
    {
        var config = StoryWorld.Config;
        var id = config["activeBranchId"]?.ToString();

        if (id != null)
        {
            activeBranch = branchById[id];
            activeBranch.LoadBranchData();
            return true;
        }
        
        return false;
    }
    
    public static void SetActiveBranch(string id, SetBranchAction backAction)
    {
        if (string.IsNullOrEmpty(id))
        {
            return;
        }
        
        activeBranch = branchById[id];
        activeBranch.backAction = backAction;
        activeBranch.LoadBranchData();
        activeBranch.Enter();
        StoryWorld.Config["activeBranchId"] = id;
        
        activeBranch.SaveBranchData();
    }

    private void SaveBranchData()
    {
        var data = StoryWorld.GetJObject("branchesData");
        var entry = data.GetOrCreate<JObject>(id);
        entry["backAction"] = new JObject()
        {
            {"branchId", backAction.branchId},
            {"lastBranchId", backAction.lastBranchId},
        };
        entry["currentIndex"] = CurrentIndex;
    }

    private void LoadBranchData()
    {
        if(isLoaded) return;
        
        isLoaded = true;
        
        var data = StoryWorld.Config["branchesData"];

        var entry = data?[id];
        if (entry != null)
        {
            var backActionJToken = entry["backAction"];
            if (backActionJToken != null)
            {
                if (backAction == null)
                {
                    backAction = new SetBranchAction()
                    {
                        branchId = backActionJToken["branchId"]!.ToString(),
                        lastBranchId = backActionJToken["lastBranchId"]?.ToString(),
                    };
                    backAction.id = string.Empty;
                    backAction.Preload();
                }
            }
            
            CurrentIndex = entry["currentIndex"]?.ToInt() ?? 0;
        }
    }

    public static void SetActiveBranch(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return;
        }
        
        activeBranch = branchById[id];
        activeBranch.LoadBranchData();
        activeBranch.Enter();
        StoryWorld.Config["activeBranchId"] = id;
    }
    
    public static void Next() => activeBranch.Internal_Next();
    public static void Back() => activeBranch.Internal_Back();
    
    private void Awake()
    {
        branchById[id] = this;
        foreach (var step in steps)
        {
            step.branchId = id;
            step.Preload();
        }
    }

    private void OnDestroy()
    {
        branchById.Remove(id);
        foreach (var step in steps)
        {
            step.Unload();
        }
    }
    
    private void Enter()
    {
        steps[CurrentIndex].Enter();
    }
    
    private void Internal_Next()
    {
        IsNext = true;
        
        if(!IsValidIndex) return;
        
        steps[CurrentIndex].Exit();
        steps[CurrentIndex].OnNext();
        CurrentIndex++;
        
        if (!IsValidIndex)
        {
            CurrentIndex--;
            return;
        }

        Enter();
    }
    
    private void Internal_Back()
    {
        IsNext = false;

        steps[CurrentIndex].Discard();
        
        if (CurrentIndex < 1)
        {
            backAction?.Discard();
            return;
        }

        steps[CurrentIndex].Exit();
        CurrentIndex--;
        Enter();
    }

    public static bool IsBranchExist(string branchId) => branchById.ContainsKey(branchId);
}