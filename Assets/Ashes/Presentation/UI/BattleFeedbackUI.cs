using System.Collections;
using TMPro;
using UnityEngine;

public class BattleFeedbackUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The TextMeshPro element that will dispaly the error message")]
    public TextMeshProUGUI FeedbackText;

    [Header("Animation Settings")]
    public float DisplayDuration = 1.0f;
    public float FadeDuration = 0.5f;

    private Coroutine currentAnimation;
    private BattleSimulation simulation;

    public void Initialize(BattleSimulation simulation)
    {
        this.simulation = simulation;

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
        FeedbackText.text = message;
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
        if (FeedbackText != null)
        {
            Color c = FeedbackText.color;
            c.a = alpha;
            FeedbackText.color = c;
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