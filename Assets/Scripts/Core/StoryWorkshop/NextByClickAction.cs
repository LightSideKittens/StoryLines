namespace Core.StoryWorkshop
{
    public class NextByClickAction : LSAction
    {
        public override void Invoke()
        {
            StoryWindow.NextByClick();
        }
    }
}