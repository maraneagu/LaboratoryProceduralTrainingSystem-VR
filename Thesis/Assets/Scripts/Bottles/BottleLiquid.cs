using System.Collections;
using UnityEngine;

public class BottleLiquid : MonoBehaviour
{
    [Header("References")]
    public Transform liquidBody;
    public Renderer liquidRenderer;
    public ReagentType reagentType = ReagentType.None;

    private Vector3 initialScale;
    private Vector3 initialPosition;

    [Header("Liquid Settings")]
    [SerializeField] private int fullAmount = 15;
    [SerializeField] private int liquidAmount = 15;

    [Header("Removing Settings")]
    private float removeDurationAnimation = 0.25f;
    private Coroutine removeRoutine;

    public int LiquidAmount => liquidAmount;
    public int FullAmount => fullAmount;

    private void Awake()
    {
        if (liquidBody != null)
        {
            initialScale = liquidBody.localScale;
            initialPosition = liquidBody.localPosition;
        }

        liquidAmount = Mathf.Clamp(liquidAmount, 0, fullAmount);
        UpdateBottleVisual();
    }

    public bool IsEmpty()
    {
        return liquidAmount <= 0;
    }

    public bool RemoveOneStep()
    {
        if (IsEmpty())
            return false;

        liquidAmount--;

        if (removeRoutine != null)
            StopCoroutine(removeRoutine);

        removeRoutine = StartCoroutine(SmoothRemoveOneStep());
        return true;
    }

    public void EmptyBottle()
    {
        liquidAmount = 0;
        UpdateBottleVisual();
    }

    private IEnumerator SmoothRemoveOneStep()
    {
        if (liquidBody == null)
            yield break;

        Vector3 startScale = liquidBody.localScale;
        Vector3 startPosition = liquidBody.localPosition;

        float fill = (float)liquidAmount / fullAmount;

        Vector3 targetScale = initialScale;
        targetScale.y = initialScale.y * fill;

        Vector3 targetPosition = initialPosition;

        float elapsed = 0f;

        while (elapsed < removeDurationAnimation)
        {
            elapsed += Time.deltaTime;
            float time = elapsed / removeDurationAnimation;

            liquidBody.localScale = Vector3.Lerp(startScale, targetScale, time);
            liquidBody.localPosition = Vector3.Lerp(startPosition, targetPosition, time);

            yield return null;
        }

        liquidBody.localScale = targetScale;
        liquidBody.localPosition = targetPosition;

        if (liquidRenderer != null)
            liquidRenderer.enabled = liquidAmount > 0;

        removeRoutine = null;
    }

    private void UpdateBottleVisual()
    {
        if (liquidBody == null)
            return;

        float fill = (float)liquidAmount / fullAmount;

        Vector3 scale = initialScale;
        scale.y = initialScale.y * fill;

        Vector3 position = initialPosition;

        liquidBody.localScale = scale;
        liquidBody.localPosition = position;

        if (liquidRenderer != null)
            liquidRenderer.enabled = liquidAmount > 0;
    }
}