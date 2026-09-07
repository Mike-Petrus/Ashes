using UnityEngine;
using UnityEngine.UI;

public class UnitStatusPanelController : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject VisualRoot;
    public Slider HPSlider;
    public Image ATBRing;
    public Image IconImage;

    [Header("Settings")]
    public float HeightOffset = 2.0f; // How high above the actor's origin the panel hovers
    public float FlashSpeed = 6.0f;

    private BattleActor actor;
    private UIThemeSO theme;
    private Camera mainCamera;

    // External Visibility Overrides
    private bool isTargeted = false;
    private bool isInfoToggled = false; // L2 Button

    public void Initialize(BattleActor actor, UIThemeSO globalTheme, Camera camera)
    {
        this.actor = actor;
        theme = globalTheme;
        mainCamera = camera;

        // Initialize HP Slider Max Value
        if (HPSlider != null)
        {
            HPSlider.maxValue = actor.Stats.MaxHP;
            HPSlider.value = actor.Stats.CurrentHP;
        }
    }

    public void SetTargetedState(bool targeted)
    {
        isTargeted = targeted;
    }

    public void SetInfoToggleState(bool toggled)
    {
        isInfoToggled = toggled;
    }

    private void LateUpdate()
    {
        if (actor == null || mainCamera == null) return;

        // 1. Sync Position
        Vector3 targetPos = new Vector3(actor.Position.x, actor.Position.y, actor.Position.z);
        transform.position = targetPos + new Vector3(0, HeightOffset, 0);
        transform.rotation = mainCamera.transform.rotation;

        // 2. Process Visibility & Visuals
        UpdatePanelState();
    }

    private void UpdatePanelState()
    {
        float atbPercent = (float)actor.ATB / (float)actor.MaxATB;
        bool isParty = actor.Faction == ActorFaction.Party;

        // --- VISIBILITY LOGIC ---
        bool shouldShow = false;

        if (isParty)
        {
            shouldShow = isTargeted || isInfoToggled;
        }
        else // Is Enemy
        {
            shouldShow = isTargeted || isInfoToggled || (atbPercent >= 0.5f);
        }

        // Apply Visibility
        if (VisualRoot != null)
        {
            VisualRoot.SetActive(shouldShow);
        }

        // If hidden, skip the rest of the math!
        if (!shouldShow) return; 

        // --- VISUAL UPDATES ---
        if (HPSlider != null)
        {
            HPSlider.value = actor.Stats.CurrentHP;
        }

        if (ATBRing != null)
        {
            ATBRing.fillAmount = atbPercent * 0.75f;

            if (atbPercent < 0.75f)
            {
                // 50-74%: Standard Yellow
                ATBRing.color = theme.ATBNormal; 
            }
            else if (atbPercent < 1.0f)
            {
                // 75-99%: Flashing Orange (PingPongs between Yellow and Orange based on time)
                float flashPhase = Mathf.PingPong(Time.time * FlashSpeed, 1f);
                ATBRing.color = Color.Lerp(theme.ATBNormal, theme.ATBWarning, flashPhase);
            }
            else
            {
                // 100%: Solid Red
                ATBRing.color = theme.ATBFilling; 
            }
        }
    }
}