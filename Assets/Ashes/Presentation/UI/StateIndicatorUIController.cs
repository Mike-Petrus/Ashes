using UnityEngine;
using UnityEngine.UI;

public class StateIndicatorUIController : MonoBehaviour
{
    [Header("Pursuit Mode")]
    public Image PursuitImage;
    public Sprite PursuitOnSprite;
    public Sprite PursuitOffSprite;

    [Header("Free-Aim Mode")]
    public Image FreeAimImage;
    public Sprite FreeAimOnSprite;
    public Sprite FreeAimOffSprite;

    private BattleEventBus events;

    public void Initialize(BattleEventBus eventBus)
    {
        events = eventBus;
        
        // Initialize default visuals
        if (PursuitImage != null && PursuitOffSprite != null) PursuitImage.sprite = PursuitOffSprite;
        if (FreeAimImage != null && FreeAimOffSprite != null) FreeAimImage.sprite = FreeAimOffSprite;

        events.Subscribe<PursuitToggledEvent>(OnPursuitToggled);
        events.Subscribe<FreeAimToggledEvent>(OnFreeAimToggled);
    }

    private void OnPursuitToggled(PursuitToggledEvent e)
    {
        if (PursuitImage != null)
        {
            PursuitImage.sprite = e.IsEnabled ? PursuitOnSprite : PursuitOffSprite;
        }
    }

    private void OnFreeAimToggled(FreeAimToggledEvent e)
    {
        if (FreeAimImage != null)
        {
            FreeAimImage.sprite = e.IsEnabled ? FreeAimOnSprite : FreeAimOffSprite;
        }
    }

    private void OnDestroy()
    {
        if (events != null)
        {
            events.Unsubscribe<PursuitToggledEvent>(OnPursuitToggled);
            events.Unsubscribe<FreeAimToggledEvent>(OnFreeAimToggled);
        }
    }
}