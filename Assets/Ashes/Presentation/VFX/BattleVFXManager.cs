using UnityEngine;

public class BattleVFXManager : MonoBehaviour
{
    [Header("Global UI Theme")]
    [Tooltip("Drag your UIThemeSO here. All child panels will pull colors from this!")]
    public UIThemeSO CurrentTheme;

    [Header("VFX Sub-Controllers")]
    public TargetHighlightVFXController TargetHighlightVFX;

    public void Initialize(BattleSimulation simulation, PlayerTurnController turnController)
    {
        if (CurrentTheme == null)
        {
            Debug.LogError("[BattleVFXManager] No UI Theme assigned! UI will break.");
            return;
        }

        if (TargetHighlightVFX != null) TargetHighlightVFX.Initialize(simulation, CurrentTheme);
    }
}