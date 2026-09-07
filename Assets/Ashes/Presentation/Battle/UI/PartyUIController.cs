using UnityEngine;

public class PartyUIController : MonoBehaviour
{
    [Header("Prefabs & Containers")]
    [Tooltip("Drag the Party Member Panel prefab here.")]
    public GameObject PartyMemberUIPrefab;

    private BattleEventBus events;
    private UIThemeSO theme;

    public void Initialize(BattleEventBus eventBus, UIThemeSO globalTheme)
    {
        events = eventBus;
        theme = globalTheme;
        
        // Listen for characters entering the battle
        events.Subscribe<ActorRegisteredEvent>(OnActorRegistered);
    }

    private void OnActorRegistered(ActorRegisteredEvent e)
    {
        if (e.Actor.Faction == ActorFaction.Party)
        {
            if (PartyMemberUIPrefab != null)
            {
                // Instantiate in local space (false) so the Layout Group takes control
                var uiObj = Instantiate(PartyMemberUIPrefab, transform, false);
                var memberUI = uiObj.GetComponent<PartyMemberUIController>();
                
                if (memberUI != null)
                {
                    memberUI.Initialize(e.Actor, events, theme);
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (events != null)
        {
            events.Unsubscribe<ActorRegisteredEvent>(OnActorRegistered);
        }
    }
}