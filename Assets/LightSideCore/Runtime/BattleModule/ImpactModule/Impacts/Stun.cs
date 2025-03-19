﻿using System;
using LSCore.Async;
using LSCore.BattleModule;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public class Stun : Impact
    {
        public float time = 0.5f;
        
        public override void Apply(Transform initiator, Transform target)
        {
            var move = target.Get<BaseMoveComp>();

            if (!move.IsRunning) return;
            
            move.IsRunning = false;
            Wait.Delay(time, () =>
            {
                move.IsRunning = true;
            });
        }
    }
}