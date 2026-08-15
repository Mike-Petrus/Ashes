using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CommandMenuUIController : MonoBehaviour
{
    [Header("UI Containers")]
    [Tooltip("The panel containing the vertical layout group for menu text.")]
    public GameObject MenuPanel;
    [Tooltip("The panel used to show descriptions (can be empty/inactive for now).")]
    public GameObject DescriptionPanel;

    [Header("UI References")]
    public TextMeshProUGUI OptionTextPrefab;

    [Header("Pool Settings")]
    public int PoolSize = 15;

    private PlayerTurnController controller;
    private UIThemeSO theme;
    private List<TextMeshProUGUI> optionTexts = new();

    public void Initialize(PlayerTurnController controller, UIThemeSO globalTheme)
    {
        this.controller = controller;
        this.theme = globalTheme;
        
        // Hide containers initially
        if (MenuPanel != null) MenuPanel.SetActive(false);
        if (DescriptionPanel != null) DescriptionPanel.SetActive(false);

        for (int i = 0; i < PoolSize; i++)
        {
            // Instantiate directly under the MenuPanel's transform
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
            if (MenuPanel != null) MenuPanel.SetActive(true);
            
            // TODO: In the future, we will toggle the DescriptionPanel here and 
            // populate it based on the currently hovered ability/item!

            DrawMenuOptions(menuState.MenuOptions, menuState.CurrentIndex);
        }
        else
        {
            if (MenuPanel != null) MenuPanel.SetActive(false);
            if (DescriptionPanel != null) DescriptionPanel.SetActive(false);
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
                
                // Use the global theme for text highlighting
                if (theme != null)
                {
                    optionTexts[i].color = (i == currentIndex) ? theme.TextHighlight : theme.TextNormal;
                }
            }
            else
            {
                optionTexts[i].gameObject.SetActive(false);
            }
        }
    }
}