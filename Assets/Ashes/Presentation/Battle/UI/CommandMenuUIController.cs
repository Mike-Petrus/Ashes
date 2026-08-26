using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommandMenuUIController : MonoBehaviour
{
    [Header("UI Containers")]
    [Tooltip("The panel containing the vertical layout group for menu text.")]
    public GameObject MenuPanel;
    [Tooltip("The panel used to show descriptions (can be empty/inactive for now).")]
    public GameObject DescriptionPanel;

    [Header("Description Containers")]
    public Image AbilityIcon;
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI CostText;

    public TextMeshProUGUI DescriptionText;
    public TextMeshProUGUI CategoryText;
    public TextMeshProUGUI ElementText;
    public TextMeshProUGUI TargetingText;

    [Header("Prefabs")]
    public TextMeshProUGUI OptionTextPrefab;

    [Header("Pool Settings")]
    public int PoolSize = 15;

    private BattleSimulation simulation;
    private PlayerTurnController controller;
    private IAbilityAssetProvider assetProvider;
    private UIThemeSO theme;
    private List<TextMeshProUGUI> optionTexts = new();

    public void Initialize(BattleSimulation sim, PlayerTurnController controller, IAbilityAssetProvider abilityDB, UIThemeSO globalTheme)
    {
        this.simulation = sim;
        this.controller = controller;
        assetProvider = abilityDB;
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

        simulation.Events.Subscribe<MenuOptionHoveredEvent>(OnMenuOptionHovered);
        simulation.Events.Subscribe<MenuSelectionClosedEvent>(OnMenuSelectionClosed);
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

    private void OnMenuOptionHovered(MenuOptionHoveredEvent e)
    {
        if (DescriptionPanel != null) DescriptionPanel.SetActive(true);

        if (!string.IsNullOrEmpty(e.AbilityId) && assetProvider != null)
        {
            AbilityTemplateSO abilitySO = assetProvider.GetAbilitySO(e.AbilityId, e.Category); 

            if (abilitySO != null)
            {
                PopulateDescription(abilitySO);
                return;
            }
        }

        // Fallback if ID is null (Future proofing for Items)
        // IDK WTF this is for but I'll probably need it at some point
        // ClearDescription();
        // NameText.SetText(e.DisplayText);
        // DescriptionText.SetText(e.Description);
    }

    private void OnMenuSelectionClosed(MenuSelectionClosedEvent e)
    {
        ClearDescription();
        DescriptionPanel.SetActive(false);
    }

    private void PopulateDescription(AbilityTemplateSO abilitySO)
    {
        string cost = "";

        if (abilitySO.Icon != null)
        {
            AbilityIcon.gameObject.SetActive(true);
            AbilityIcon.sprite = abilitySO.Icon;
        }
        else
        {
            AbilityIcon.gameObject.SetActive(false);
        }

        if (abilitySO.Requirements.Count == 0)
        {
            cost = "No Requirements";
        }
        else
        {
            cost += $"{abilitySO.Requirements[0].Type}: {abilitySO.Requirements[0].Amount}";

            for (int i = 1; i < abilitySO.Requirements.Count; i++)
            {
                cost += $", {abilitySO.Requirements[i].Type}: {abilitySO.Requirements[i].Amount}";
            }
        }


        NameText.SetText(abilitySO.Name);
        CostText.SetText(cost);
        DescriptionText.SetText(abilitySO.Description);
        CategoryText.SetText(abilitySO.Category);
        ElementText.SetText(abilitySO.ElementType.ToString());
        TargetingText.SetText(abilitySO.Mode.ToString());
    } 

    private void ClearDescription()
    {
        AbilityIcon.gameObject.SetActive(false);
        NameText.SetText("");
        CostText.SetText("");

        DescriptionText.SetText("");
        CategoryText.SetText("");
        ElementText.SetText("");
        TargetingText.SetText("");
    }

    private void OnDestroy()
    {
        if (simulation != null && simulation.Events != null)
        {
            simulation.Events.Unsubscribe<MenuOptionHoveredEvent>(OnMenuOptionHovered);
        }
    }
}