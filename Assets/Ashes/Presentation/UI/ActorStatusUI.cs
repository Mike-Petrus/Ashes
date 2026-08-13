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

    [Header("Visual Feedback")]
    public Image BackgroundImage;
    public Color NormalColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
    public Color HoverColor = new Color(0.55f, 0.55f, 0.55f, 0.9f);
    public Color ActiveCommandColor = new Color(0.1f, 0.3f, 0.6f, 0.9f);

    [Header("Animation Settings")]
    [Tooltip("The inner container that holds the background and text. (Needs to be a child of this object)")]
    public RectTransform VisualRoot; 
    public float HoverPopOutX = 20f;
    public float ActivePopOutX = 35f; // Slide out even further when confirming commands!
    public float SlideSpeed = 15f;

    [Header("ATB Visuals")]
    public Image ATBFillImage;
    public Color ATBNormalColor = Color.red;
    public Color ATBReadyColor = Color.green;

    private ActorId trackedActorId;
    private BattleActor actor;
    private float targetX = 0f;

    // Set up by Boostrapper
    public void Initialize(BattleActor battleActor, BattleEventBus battleEvents)
    {
        actor = battleActor;
        trackedActorId = actor.Id;

        NameText.text = actor.Name;
        UpdateText();
        
        if (BackgroundImage != null) BackgroundImage.color = NormalColor;
        if (ATBFillImage != null) ATBFillImage.color = ATBNormalColor;
        
        battleEvents.Subscribe<ResourceConsumedEvent>(OnResourceConsumed);
        battleEvents.Subscribe<DamageAppliedEvent>(OnDamageApplied);
        battleEvents.Subscribe<HealAppliedEvent>(OnHealApplied);
        battleEvents.Subscribe<ATBChangedEvent>(OnATBChanged);

        battleEvents.Subscribe<PartyMemberHoveredEvent>(OnPartyHovered);
        battleEvents.Subscribe<PlayerCommandStartedEvent>(OnCommandStarted);
        battleEvents.Subscribe<PlayerCommandEndedEvent>(OnCommandEnded);
        battleEvents.Subscribe<ActorReadyEvent>(OnActorReady);
    }

    private void Update()
    {
        // Smoothly slide the VisualRoot towards the targetX position every frame
        if (VisualRoot != null)
        {
            float currentX = VisualRoot.anchoredPosition.x;
            if (Mathf.Abs(currentX - targetX) > 0.1f)
            {
                float newX = Mathf.Lerp(currentX, targetX, Time.deltaTime * SlideSpeed);
                VisualRoot.anchoredPosition = new Vector2(newX, VisualRoot.anchoredPosition.y);
            }
        }
    }

    private void UpdateText()
    {
        HPText.text = $"HP: {actor.CurrentHP} / {actor.Stats.MaxHP}";
        MPText.text = $"MP: {actor.CurrentMP} / {actor.Stats.MaxMP}";
    }

    private void OnPartyHovered(PartyMemberHoveredEvent e)
    {
        if (BackgroundImage == null) return;
        
        // Don't override the active command blue color/position if they are currently taking their turn
        if (BackgroundImage.color == ActiveCommandColor) return;

        if (e.ActorId == trackedActorId)
        {
            BackgroundImage.color = HoverColor;
            targetX = HoverPopOutX; // Slide out slightly
        }
        else
        {
            BackgroundImage.color = NormalColor;
            targetX = 0f; // Slide back to origin
        }
    }

    private void OnCommandStarted(PlayerCommandStartedEvent e)
    {
        if (BackgroundImage == null) return;
        
        if (e.ActorId == trackedActorId)
        {
            BackgroundImage.color = ActiveCommandColor;
            targetX = ActivePopOutX; // Slide out even further!
        }
    }

    private void OnCommandEnded(PlayerCommandEndedEvent e)
    {
        if (e.ActorId == trackedActorId)
        {
            BackgroundImage.color = NormalColor;
            targetX = 0f; // Slide back to origin
        }
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

            // If ATB was spent, revert the color back to normal
            if (e.ActorATB < actor.MaxATB && ATBFillImage != null)
            {
                ATBFillImage.color = ATBNormalColor;
            }
        }
    }

    private void OnActorReady(ActorReadyEvent e)
    {
        if (e.ActorId == trackedActorId && ATBFillImage != null)
        {
            ATBFillImage.color = ATBReadyColor; // Turn green when ready!
        }
    }
}