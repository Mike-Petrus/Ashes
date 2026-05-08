using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleMenuUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject MenuPanel;
    public TextMeshProUGUI OptionTextPrefab;

    [Header("Pool Settings")]
    public int PoolSize = 15;

    [Header("Cursor Colors")]
    public Color NormalColor = Color.white;
    public Color HighlightedColor = Color.yellow;
    public Color DisabledColor = Color.gray;

    private PlayerTurnController controller;
    private List<TextMeshProUGUI> optionTexts = new();

    public void Initialize(PlayerTurnController controller)
    {
        this.controller = controller;
        MenuPanel.SetActive(false);

        for (int i = 0; i < PoolSize; i++)
        {
            TextMeshProUGUI newText = Instantiate(OptionTextPrefab, MenuPanel.transform);
            newText.gameObject.SetActive(false);
            optionTexts.Add(newText);
        }
    }

    void Update()
    {
        if (controller == null || controller.CurrentState == null)
        {
            return;
        }

        if (controller.CurrentState is IMenuState menuState)
        {
            MenuPanel.SetActive(true);
            DrawMenuOptions(menuState.MenuOptions, menuState.CurrentIndex);
        }
        else
        {
            MenuPanel.SetActive(false);
        }
    }

    private void DrawMenuOptions(IReadOnlyList<string> options, int currentIndex)
    {
        for (int i = 0; i < optionTexts.Count; i++)
        {
            if (i < options.Count)
            {
                optionTexts[i].gameObject.SetActive(true);
                optionTexts[i].text = options[i];
                optionTexts[i].color = (i == currentIndex) ? HighlightedColor : NormalColor;
            }
            else
            {
                optionTexts[i].gameObject.SetActive(false);
            }
        }
    }
}