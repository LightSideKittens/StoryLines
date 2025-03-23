using System;
using UnityEngine;

namespace StoryWorkshop
{
    [Serializable]
    public class TextAction : BaseStoryAction
    {
        [TextArea(1, 5)]
        public string text;
        
        protected override void OnInvoke()
        {
            StoryWindow.SetText(text);
        }
    }
}