using System.Collections.Generic;
using LSCore;
using UnityEngine;

namespace Core.Match3Core
{
    public class GoalView : MonoBehaviour
    {
        public LSText text;
        public LSImage goalImage;
        public GameObject checkMark;
        public int targetCount;
        
        private int count;

        public int Count
        {
            get => count;
            set
            {
                count = value;
                if (IsReached)
                {
                    text.gameObject.SetActive(false);
                    checkMark.SetActive(true);
                }
                else
                {
                    text.gameObject.SetActive(true);
                    checkMark.SetActive(false);
                    text.text = $"{targetCount - count}";
                }
            }
        }
        
        public bool IsReached => count >= targetCount;

        public static Dictionary<Sprite, GoalView> Goals { get; private set; } = new();

        private void Awake()
        {
            Goals[goalImage.sprite] = this;
            Count = 0;
        }

        private void OnDestroy()
        {
            Goals.Remove(goalImage.sprite);
        }
    }
}