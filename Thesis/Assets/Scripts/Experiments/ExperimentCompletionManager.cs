using System.Collections;
using UnityEngine;

public class ExperimentCompletionManager : MonoBehaviour
{
    [Header("Canvas Group")]
    [SerializeField] private CanvasGroup completionGroup;

    [Header("Fade Settings")]
    [SerializeField] private float completionDuration = 0.4f;
    [SerializeField] private float completionAlpha = 1f;
    [SerializeField] private float completionDelay = 8f;

    [Header("Audio")]
    [SerializeField] private AudioClip transitionAudio;
    [SerializeField] private float completionVolume = 0.3f;

    private bool hasCompleted;
    private Coroutine completionRoutine;

    private void Awake()
    {
        if (completionGroup != null)
            completionGroup.alpha = 0f;
    }

    public void CompleteExperiment()
    {
        if (hasCompleted)
            return;

        hasCompleted = true;

        if (completionRoutine != null)
            StopCoroutine(completionRoutine);

        completionRoutine = StartCoroutine(CompletionRoutine());
    }

    private IEnumerator CompletionRoutine()
    {
        yield return new WaitForSeconds(completionDelay);

        AudioManager.Instance?.PlayAudio(transitionAudio, completionVolume);

        yield return Fade();
    }

    private IEnumerator Fade()
    {
        float elapsed = 0f;
        float startAlpha = completionGroup.alpha;

        while (elapsed < completionDuration)
        {
            elapsed += Time.deltaTime;

            float time = Mathf.Clamp01(elapsed / completionDuration);
            time = Mathf.SmoothStep(0f, 1f, time);

            completionGroup.alpha = Mathf.Lerp(startAlpha, completionAlpha, time);

            yield return null;
        }

        completionGroup.alpha = completionAlpha;
    }
}