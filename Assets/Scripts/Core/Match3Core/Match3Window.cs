using System.Collections.Generic;
using LSCore;
using UnityEngine;

namespace Core.Match3Core
{
    public class Match3Window : BaseWindow<Match3Window>
    {
        public GameObject gameScreen;
        public GameObject winScreen;

        public LocalizationText stepsText;

        public static LocalizationText StepsText => Instance.stepsText;
        
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

        public static void Destroy()
        {
            Destroy(Instance.gameObject);
        }
    }
}