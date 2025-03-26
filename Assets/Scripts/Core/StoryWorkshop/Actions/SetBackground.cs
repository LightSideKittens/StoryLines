using System;

namespace StoryWorkshop
{
    [Serializable]
    public class SetBackground : BaseSingleGameObjectSetter<SetBackground>
    {
        protected override SetBackground This => this;
        protected override string ConfigPropertyName => "prevBackgroundByActionId";
    }
}