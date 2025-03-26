using System;

namespace StoryWorkshop
{
    [Serializable]
    public class SetMainCharacter : BaseSingleGameObjectSetter<SetMainCharacter>
    {
        protected override SetMainCharacter This => this;
        protected override string ConfigPropertyName => "prevCharacterByActionId";
    }
}