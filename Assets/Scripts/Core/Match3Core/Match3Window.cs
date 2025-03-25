using LSCore;
using UnityEngine;

namespace Core.Match3Core
{
    public class Match3Window : BaseWindow<Match3Window>
    {
        public GameObject gameScreen;
        public GameObject winScreen;
        public LocalizationText stepsText;
        public LocalizationText pointsText;
        
        public static LocalizationText StepsText => Instance.stepsText;
        public static LocalizationText PointsText => Instance.pointsText;

        public new static void Show()
        {
            BaseWindow<Match3Window>.Show(ShowWindowOption.HideAllPrevious);
            Instance.gameScreen.SetActive(true);
            Instance.winScreen.SetActive(false);
        }

        public static void Win()
        {
            BaseWindow<Match3Window>.Show(ShowWindowOption.HideAllPrevious);
            Instance.gameScreen.SetActive(false);
            Instance.winScreen.SetActive(true);
        }
    }
}