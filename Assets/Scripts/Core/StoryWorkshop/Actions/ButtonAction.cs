using System;
using System.Collections.Generic;
using Core.StoryWorkshop;
using UnityEngine;

namespace StoryWorkshop
{
    [Serializable]
    public class ButtonsAction : BaseStoryAction
    {
        [Serializable]
        public class Action
        {
            public string text;
            public bool stepNext = true;
            [SerializeReference] public List<LSAction> clickActions = new();

            public void Invoke(System.Action onClick)
            {
                StoryWindow.BlockNextByClick = true;
                var actions = new List<LSAction>(clickActions);
                actions.Add((DelegateLSAction)onClick);
                if (stepNext)
                {
                    actions.Add(new NextByClickAction());
                }
                StoryWindow.CreateButton(text, actions);
            }
        }
        
        public List<Action> actions;
        
        protected override void OnInvoke()
        {
            foreach (var buttonAction in actions)
            {
                buttonAction.Invoke(Discard);
            }
        }

        public override void Discard()
        {
            base.Discard();
            StoryWorld.RemoveAction(Hash);
            StoryWindow.ClearButtons();
            StoryWindow.BlockNextByClick = false;
        }
    }
}