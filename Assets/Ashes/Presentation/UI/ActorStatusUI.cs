using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActorStatusUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI HPText;
    public TextMeshProUGUI MPText;
    public Slider ATBSlider;

    private ActorId trackedActorId;
    private BattleActor actor;

    // Set up by Boostrapper
    public void Initialize(BattleActor battleActor, BattleEventBus battleEvents)
    {
        actor = battleActor;
        trackedActorId = actor.Id;

        NameText.text = actor.Name;
        UpdateText();
        
        battleEvents.Subscribe<ResourceConsumedEvent>(OnResourceConsumed);
        battleEvents.Subscribe<DamageAppliedEvent>(OnDamageApplied);
        battleEvents.Subscribe<HealAppliedEvent>(OnHealApplied);
        battleEvents.Subscribe<ATBChangedEvent>(OnATBChanged);
    }

    private void UpdateText()
    {
        HPText.text = $"HP: {actor.CurrentHP} / {actor.Stats.MaxHP}";
        MPText.text = $"MP: {actor.CurrentMP} / {actor.Stats.MaxMP}";
    }

    private void OnResourceConsumed(ResourceConsumedEvent e)
    {
        if (e.ActorId.Value == trackedActorId.Value) UpdateText();
    }

    private void OnDamageApplied(DamageAppliedEvent e)
    {
        if (e.TargetId.Value == trackedActorId.Value) UpdateText();
    }

    private void OnHealApplied(HealAppliedEvent e)
    {
        if (e.TargetId.Value == trackedActorId.Value) UpdateText();
    }

    private void OnATBChanged(ATBChangedEvent e)
    {
        if (e.ActorId == trackedActorId)
        {
            ATBSlider.value = e.ActorATB;
        }
    }
}