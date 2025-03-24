using System.Collections.Generic;
using LSCore;
using UnityEngine;

public class StoryWindow : BaseWindow<StoryWindow>
{
    public LSText text;
    public RectTransform contentParent;
    public LSButton buttonPrefab;
    public LSButton backButton;
    
    private List<LSButton> buttons = new();
    
    public static bool BlockNextByClick
    {
        get => Instance.blockNextByClick;
        set => Instance.blockNextByClick = value;
    }

    private bool blockNextByClick;

    private void Start()
    {
        backButton.Clicked += StoryBranch.Back;
    }
    
    public static void SetText(string text)
    {
        Instance.text.text = text;
    }

    public static void NextByClick()
    {
        if(BlockNextByClick) return;
        StoryBranch.Next();
    }
    
    public static void ClearButtons()
    {
        foreach (var button in Instance.buttons)
        {
            Destroy(button.gameObject);
        }
        Instance.buttons.Clear();
    }
    
    public static void CreateButton(string text, List<LSAction> clickActions)
    {
        var button = Instantiate(Instance.buttonPrefab, Instance.contentParent);
        button.Clicked = clickActions.Invoke;
        button.GetComponentInChildren<LSText>().text = text;
        Instance.buttons.Add(button);
    }
}