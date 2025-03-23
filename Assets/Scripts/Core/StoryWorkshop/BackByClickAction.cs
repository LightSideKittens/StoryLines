namespace Core.StoryWorkshop
{
    public class BackByClickAction : LSAction
    {
        public override void Invoke()
        {
            StoryBranch.Back();
        }
    }
}