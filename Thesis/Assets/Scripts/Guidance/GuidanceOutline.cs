using System.Collections;
using UnityEngine;

public class GuidanceOutline : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Outline guidanceOutline;

    [Header("Pulse Width")]
    [SerializeField] private float minOutlineWidth = 6f;
    [SerializeField] private float maxOutlineWidth = 10f;

    [Header("Gradient Color")]
    [SerializeField] private Color startColor = new Color32(255, 0, 255, 255);
    [SerializeField] private Color endColor = new Color32(255, 0, 206, 255);

    [Header("Pulse Timing")]
    [SerializeField] private float pulseDuration = 1.2f;

    private Coroutine pulseRoutine;
    private bool isActive;

    private void Awake()
    {
        if (guidanceOutline == null)
            guidanceOutline = GetComponent<Outline>();

        if (guidanceOutline != null)
        {
            guidanceOutline.enabled = false;
            guidanceOutline.OutlineColor = startColor;
            guidanceOutline.OutlineWidth = minOutlineWidth;
        }
    }

    public void ShowGuidance()
    {
        if (guidanceOutline == null || isActive)
            return;

        isActive = true;
        guidanceOutline.enabled = true;
        guidanceOutline.OutlineColor = startColor;
        guidanceOutline.OutlineWidth = minOutlineWidth;

        pulseRoutine = StartCoroutine(PulseRoutine());
    }

    public void HideGuidance()
    {
        if (guidanceOutline == null)
            return;

        isActive = false;

        if (pulseRoutine != null)
        {
            StopCoroutine(pulseRoutine);
            pulseRoutine = null;
        }

        guidanceOutline.enabled = false;
    }

    private IEnumerator PulseRoutine()
    {
        while (true)
        {
            yield return AnimatePulse(minOutlineWidth, maxOutlineWidth, startColor, endColor, pulseDuration * 0.5f);
            yield return AnimatePulse(maxOutlineWidth, minOutlineWidth, endColor, startColor, pulseDuration * 0.5f);
        }
    }

    private IEnumerator AnimatePulse(
        float startWidth,
        float endWidth,
        Color fromColor,
        Color toColor,
        float duration)
    {
        if (duration <= 0f)
        {
            guidanceOutline.OutlineWidth = endWidth;
            guidanceOutline.OutlineColor = toColor;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            guidanceOutline.OutlineWidth = Mathf.Lerp(startWidth, endWidth, t);
            guidanceOutline.OutlineColor = Color.Lerp(fromColor, toColor, t);

            yield return null;
        }

        guidanceOutline.OutlineWidth = endWidth;
        guidanceOutline.OutlineColor = toColor;
    }
}