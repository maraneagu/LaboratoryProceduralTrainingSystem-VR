using System.Collections;
using UnityEngine;

public class ValidationOutline : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Outline validationOutline;

    [Header("Color Settings")]
    [SerializeField] private Color correctColor = new Color(0f, 1.5f, 0f, 1f);
    [SerializeField] private Color incorrectColor = new Color(1.5f, 0f, 0f, 1f);

    [Header("Width Settings")]
    [SerializeField] private float startValidationWidth = 5f;
    [SerializeField] private float peakValidationWidth = 8f;

    [Header("Timing Settings")]
    [SerializeField] private float startDelay = 0.7f;
    [SerializeField] private float fadeInDuration = 0.4f;
    [SerializeField] private float holdDuration = 0.6f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    [Header("Audio")]
    [SerializeField] private AudioClip correctValidationAudio;
    [SerializeField] private AudioClip incorrectValidationAudio;
    [SerializeField] private float validationAudioVolume = 0.2f;

    private Coroutine validationRoutine;

    private void Awake()
    {
        if (validationOutline == null)
            validationOutline = GetComponent<Outline>();

        if (validationOutline != null)
            validationOutline.enabled = false;
    }

    public void ShowValidation(bool wasCorrect)
    {
        if (validationOutline == null)
            return;

        if (validationRoutine != null)
            return;

        validationRoutine = StartCoroutine(PlayValidation(wasCorrect));
    }

    private IEnumerator PlayValidation(bool wasCorrect)
    {
        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        AudioClip validationAudio = wasCorrect ? correctValidationAudio : incorrectValidationAudio;
        AudioManager.Instance?.PlayAudio(validationAudio, validationAudioVolume);

        validationOutline.enabled = true;
        validationOutline.OutlineColor = wasCorrect ? correctColor : incorrectColor;
        validationOutline.OutlineWidth = startValidationWidth;

        yield return AnimateWidth(startValidationWidth, peakValidationWidth, fadeInDuration);

        if (holdDuration > 0f)
            yield return new WaitForSeconds(holdDuration);

        yield return AnimateWidth(peakValidationWidth, startValidationWidth, fadeOutDuration);

        validationOutline.enabled = false;
        validationRoutine = null;
    }

    private IEnumerator AnimateWidth(float startWidth, float endWidth, float duration)
    {
        if (duration <= 0f)
        {
            validationOutline.OutlineWidth = endWidth;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float time = Mathf.Clamp01(elapsed / duration);
            time = Mathf.SmoothStep(0f, 1f, time);

            validationOutline.OutlineWidth = Mathf.Lerp(startWidth, endWidth, time);
            yield return null;
        }

        validationOutline.OutlineWidth = endWidth;
    }

    public float GetValidationDuration()
    {
        return startDelay + fadeInDuration + holdDuration + fadeOutDuration;
    }
}