using System;
using UnityEngine;

namespace StoryWorkshop
{
    [Serializable]
    public class SetActiveGO : BaseStoryAction
    {
        public GameObject target;
        public bool active;
        
        protected override void OnInvoke()
        {
            target.SetActive(active);
        }

        public override void Discard()
        {
            base.Discard();
            target.SetActive(!active);
        }
    }
}