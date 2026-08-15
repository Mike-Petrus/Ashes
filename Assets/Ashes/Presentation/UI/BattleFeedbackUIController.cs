using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class BattleFeedbackUIController : MonoBehaviour
{
    [Header("Animation Settings")]
    public float DisplayDuration = 1.0f;
    public float FadeDuration = 0.5f;

    private TextMeshProUGUI feedbackText; 
    private Coroutine currentAnimation;
    private BattleSimulation simulation;

    public void Initialize(BattleSimulation simulation)
    {
        this.simulation = simulation;
        
        // Dynamically grab the component attached to this GameObject
        feedbackText = GetComponent<TextMeshProUGUI>();

        SetTextAlpha(0f);

        this.simulation.Events.Subscribe<PlayerFeedbackEvent>(OnPlayerFeedbackReceived);
    }

    private void OnPlayerFeedbackReceived(PlayerFeedbackEvent e)
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        // TODO: Play error sound via AudioManager

        currentAnimation = StartCoroutine(AnimateFeedback(e.FeedbackMessage));
    }

    private IEnumerator AnimateFeedback(string message)
    {
        if (feedbackText != null) feedbackText.text = message;
        SetTextAlpha(1f);

        // Hold for the display duration
        yield return new WaitForSeconds(DisplayDuration);

        // Fade out
        float timer = 0f;
        
        while (timer < FadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / FadeDuration);
            SetTextAlpha(alpha);
            yield return null;
        }

        SetTextAlpha(0f);
        currentAnimation = null;
    }

    private void SetTextAlpha(float alpha)
    {
        if (feedbackText != null)
        {
            Color c = feedbackText.color;
            c.a = alpha;
            feedbackText.color = c;
        }
    }

    private void OnDestroy()
    {
        if (simulation != null)
        {
            simulation.Events.Unsubscribe<PlayerFeedbackEvent>(OnPlayerFeedbackReceived);
        }
    }
}