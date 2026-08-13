using UnityEngine;
using TMPro;

public class FloatingTextManager : MonoBehaviour
{
    [Tooltip("A Prefab containing a TextMeshPro component. Add a simple script to it that moves it up and destroys it after 1.5s!")]
    public GameObject FloatingTextPrefab; 
    
    private BattleSimulation simulation;

    public void Initialize(BattleSimulation sim)
    {
        simulation = sim;
        
        // Subscribe using the Simulation's EventBus
        simulation.Events.Subscribe<DamageAppliedEvent>(OnDamageApplied);
        simulation.Events.Subscribe<HealAppliedEvent>(OnHealApplied);
    }

    private void OnDamageApplied(DamageAppliedEvent e)
    {
        SpawnText(e.TargetId, e.Amount.ToString(), Color.white);
    }

    private void OnHealApplied(HealAppliedEvent e)
    {
        SpawnText(e.TargetId, e.Amount.ToString(), Color.green);
    }

    private void SpawnText(ActorId targetId, string text, Color color)
    {
        if (FloatingTextPrefab == null || simulation == null) return;

        // 1. Look up the actor directly from the pure C# Engine (O(1) lookup speed!)
        var targetActor = simulation.Actors.GetActor(targetId);
        if (targetActor == null) return;

        // 2. Convert their pure simulation position into a Unity world position
        Vector3 spawnPos = new Vector3(targetActor.Position.x, targetActor.Position.y + 2.0f, targetActor.Position.z);
        
        // 3. Spawn the text
        GameObject floatingCombatText = Instantiate(FloatingTextPrefab, spawnPos, Quaternion.identity);
        
        var textMeshPro = floatingCombatText.GetComponent<TextMeshPro>();
        if (textMeshPro != null)
        {
            textMeshPro.text = text;
            textMeshPro.color = color;
        }

        // Failsafe destroy (You should add an animation/fade script to the Prefab itself for smoothness)
        Destroy(floatingCombatText, 1.5f);
    }

    private void OnDestroy()
    {
        // Always cleanly unsubscribe to prevent memory leaks when changing scenes
        if (simulation != null && simulation.Events != null)
        {
            simulation.Events.Unsubscribe<DamageAppliedEvent>(OnDamageApplied);
            simulation.Events.Unsubscribe<HealAppliedEvent>(OnHealApplied);
        }
    }
}