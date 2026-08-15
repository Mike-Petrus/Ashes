using UnityEngine;

public class BattleUIManager : MonoBehaviour
{
    [Header("Global UI Theme")]
    [Tooltip("Drag your UIThemeSO here. All child panels will pull colors from this!")]
    public UIThemeSO CurrentTheme;

    [Header("UI Sub-Controllers")]
    public PartyUIController PartyUI;
    public CommandMenuUIController CommandMenuUI;
    public BattleFeedbackUIController FeedbackUI;
    public FloatingTextUIController FloatingTextUI;
    public StateIndicatorUIController StateIndicatorUI;
    public TargetInfoUIController TargetInfoUI;

    // Future Controllers:
    // public TargetInfoUIController TargetInfoUI;
    // public TimelineUIController TimelineUI;

    public void Initialize(BattleSimulation simulation, PlayerTurnController turnController)
    {
        if (CurrentTheme == null)
        {
            Debug.LogError("[BattleUIManager] No UI Theme assigned! UI will break.");
            return;
        }

        if (PartyUI != null) PartyUI.Initialize(simulation.Events, CurrentTheme);
        if (CommandMenuUI != null) CommandMenuUI.Initialize(turnController, CurrentTheme);
        if (FeedbackUI != null) FeedbackUI.Initialize(simulation);
        if (FloatingTextUI != null) FloatingTextUI.Initialize(simulation);
        if (StateIndicatorUI != null) StateIndicatorUI.Initialize(simulation.Events);
        if (TargetInfoUI != null) TargetInfoUI.Initialize(simulation, CurrentTheme);
    }
}