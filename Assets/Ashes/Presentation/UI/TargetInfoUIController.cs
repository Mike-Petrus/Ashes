using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TargetInfoUIController : MonoBehaviour
{
    [Header("UI Containers")]
    public GameObject TargetInfoPanel;
    public Image TargetImage; // The portrait/sprite

    [Header("Text & Bars")]
    public TextMeshProUGUI NameText;
    public Slider HPSlider;
    public Image HPFillImage;
    public Slider ATBSlider;
    public Image ATBFillImage;

    [Header("Status Icons")]
    public Transform StatusesContainer;
    public GameObject StatusIconPrefab; // A simple UI Image Prefab
    public int StatusPoolSize = 6;

    private BattleSimulation simulation;
    private UIThemeSO theme;
    private ActorId? currentFocusedActorId;
    
    private List<Image> statusIconPool = new List<Image>();

    public void Initialize(BattleSimulation sim, UIThemeSO globalTheme)
    {
        simulation = sim;
        theme = globalTheme;

        // Hide initially
        if (TargetInfoPanel != null) TargetInfoPanel.SetActive(false);

        // Initialize Status Icon Pool
        if (StatusIconPrefab != null && StatusesContainer != null)
        {
            for (int i = 0; i < StatusPoolSize; i++)
            {
                GameObject iconObj = Instantiate(StatusIconPrefab, StatusesContainer);
                iconObj.SetActive(false);
                statusIconPool.Add(iconObj.GetComponent<Image>());
            }
        }

        // Subscribe to Focus Changes
        simulation.Events.Subscribe<TargetingFocusChangedEvent>(OnFocusChanged);
        
        // Subscribe to Live Data Changes
        simulation.Events.Subscribe<DamageAppliedEvent>(OnDamageApplied);
        simulation.Events.Subscribe<HealAppliedEvent>(OnHealApplied);
        simulation.Events.Subscribe<ATBChangedEvent>(OnATBChanged);
        
        // Future Proofing: Listen for status additions/removals
        simulation.Events.Subscribe<StatusAppliedEvent>(OnStatusChanged);
        simulation.Events.Subscribe<StatusExpiredEvent>(OnStatusChanged);
    }

    private void OnFocusChanged(TargetingFocusChangedEvent e)
    {
        currentFocusedActorId = e.FocusedTargetId;

        if (!currentFocusedActorId.HasValue)
        {
            if (TargetInfoPanel != null) TargetInfoPanel.SetActive(false);
            return;
        }

        var actor = simulation.Actors.GetActor(currentFocusedActorId.Value);
        if (actor == null)
        {
            if (TargetInfoPanel != null) TargetInfoPanel.SetActive(false);
            return;
        }

        if (TargetInfoPanel != null) TargetInfoPanel.SetActive(true);
        
        UpdateAllInfo(actor);
    }

    private void UpdateAllInfo(BattleActor actor)
    {
        // 1. Update Identity
        if (NameText != null)
        {
            NameText.text = actor.Name;
            NameText.color = theme != null ? theme.TextNormal : Color.white;
        }

        // Tint portrait placeholder based on faction
        if (TargetImage != null)
        {
            TargetImage.color = actor.Faction == ActorFaction.Party ? Color.blue : Color.red;
        }

        // 2. Update HP
        if (HPSlider != null)
        {
            HPSlider.maxValue = actor.Stats.MaxHP;
            HPSlider.value = actor.CurrentHP;
            
            if (HPFillImage != null)
            {
                // Dynamic Color: Red to Green based on health percentage
                HPFillImage.color = Color.Lerp(Color.red, Color.green, (float)actor.CurrentHP / actor.Stats.MaxHP);
            }
        }

        // 3. Update ATB
        if (ATBSlider != null)
        {
            ATBSlider.maxValue = actor.MaxATB;
            ATBSlider.value = actor.ATB;
            
            if (ATBFillImage != null && theme != null)
            {
                ATBFillImage.color = actor.IsReady ? theme.ATBReady : theme.ATBFilling;
            }
        }

        // 4. Update Statuses
        UpdateStatuses(actor);
    }

    private void UpdateStatuses(BattleActor actor)
    {
        if (statusIconPool.Count == 0) return;

        int activeCount = 0;
        foreach (var status in actor.ActiveStatuses)
        {
            if (activeCount >= statusIconPool.Count) break;

            // Enable the icon
            statusIconPool[activeCount].gameObject.SetActive(true);
            
            // TODO: In the future, grab `StatusBlueprintSO.IconSprite` and assign it here
            // statusIconPool[activeCount].sprite = statusBlueprint.Icon;
            
            // Placeholder: Tint the square so we know it's working
            statusIconPool[activeCount].color = Color.magenta; 

            activeCount++;
        }

        // Hide unused pool slots
        for (int i = activeCount; i < statusIconPool.Count; i++)
        {
            statusIconPool[i].gameObject.SetActive(false);
        }
    }

    // --- Live Event Handlers ---

    private void OnDamageApplied(DamageAppliedEvent e)
    {
        if (currentFocusedActorId.HasValue && e.TargetId == currentFocusedActorId.Value)
            RefreshFocusedActor();
    }

    private void OnHealApplied(HealAppliedEvent e)
    {
        if (currentFocusedActorId.HasValue && e.TargetId == currentFocusedActorId.Value)
            RefreshFocusedActor();
    }

    private void OnATBChanged(ATBChangedEvent e)
    {
        if (currentFocusedActorId.HasValue && e.ActorId == currentFocusedActorId.Value)
            RefreshFocusedActor();
    }

    private void OnStatusChanged(IBattleEvent e)
    {
        // StatusAppliedEvent and StatusExpiredEvent both have TargetId, 
        // but since they are different classes, we handle the extraction loosely or refresh.
        // For simplicity in the MVP, we just refresh the whole actor if focus is active.
        if (currentFocusedActorId.HasValue)
            RefreshFocusedActor();
    }

    private void RefreshFocusedActor()
    {
        var actor = simulation.Actors.GetActor(currentFocusedActorId.Value);
        if (actor != null) UpdateAllInfo(actor);
    }

    private void OnDestroy()
    {
        if (simulation != null && simulation.Events != null)
        {
            simulation.Events.Unsubscribe<TargetingFocusChangedEvent>(OnFocusChanged);
            simulation.Events.Unsubscribe<DamageAppliedEvent>(OnDamageApplied);
            simulation.Events.Unsubscribe<HealAppliedEvent>(OnHealApplied);
            simulation.Events.Unsubscribe<ATBChangedEvent>(OnATBChanged);
            simulation.Events.Unsubscribe<StatusAppliedEvent>(OnStatusChanged);
            simulation.Events.Unsubscribe<StatusExpiredEvent>(OnStatusChanged);
        }
    }
}