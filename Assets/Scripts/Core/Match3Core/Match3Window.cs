using LSCore;
using UnityEngine;

namespace Core.Match3Core
{
    public class Match3Window : BaseWindow<Match3Window>
    {
        public GameObject gameScreen;
        public GameObject winScreen;
        public GameObject loseScreen;

        public LocalizationText movesText;

        public static LocalizationText MovesText => Instance.movesText;
        
        public new static void Show()
        {
            BaseWindow<Match3Window>.Show(ShowWindowOption.HideAllPrevious);
            Instance.gameScreen.SetActive(true);
            Instance.winScreen.SetActive(false);
            Instance.loseScreen.SetActive(false);
        }

        public static void Win()
        {
            BaseWindow<Match3Window>.Show(ShowWindowOption.HideAllPrevious);
            Instance.gameScreen.SetActive(false);
            Instance.winScreen.SetActive(true);
            Instance.loseScreen.SetActive(false);
        }

        public static void Lose()
        {
            BaseWindow<Match3Window>.Show(ShowWindowOption.HideAllPrevious);
            Instance.gameScreen.SetActive(false);
            Instance.winScreen.SetActive(false);
            Instance.loseScreen.SetActive(true);
        }

        public static void Destroy()
        {
            Destroy(Instance.gameObject);
        }
    }
}