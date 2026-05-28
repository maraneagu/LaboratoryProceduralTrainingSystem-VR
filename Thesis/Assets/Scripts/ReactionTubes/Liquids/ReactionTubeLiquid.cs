using System.Collections;
using UnityEngine;

public class ReactionTubeLiquid : MonoBehaviour
{
    [Header("References")]
    public Transform liquidBody;
    public Renderer liquidRenderer;

    private Vector3 liquidScale;
    private Vector3 liquidPosition;

    [Header("Filling Settings")]
    public int maxFillSteps = 4;
    public float fillDurationAnimation = 0.35f;
    private Coroutine fillRoutine;

    public int CurrentFillStep { get; protected set; } = 0;
    protected float liquidAmount = 0f;

    protected virtual void Awake()
    {
        if (liquidBody != null)
        {
            liquidScale = liquidBody.localScale;
            liquidPosition = liquidBody.localPosition;
        }

        CurrentFillStep = 0;
        liquidAmount = 0f;
        UpdateReactionTubeVisual();
    }

    public bool IsEmpty()
    {
        return CurrentFillStep == 0;
    }

    public bool IsFull()
    {
        return CurrentFillStep >= maxFillSteps;
    }

    public virtual bool AddReagent(ReagentType reagent)
    {
        return false;
    }

    public virtual bool AddSolution(BeakerLiquid solution)
    {
        return false;
    }

    public virtual void ApplyHeating() { }

    public void FillReactionTube(int step)
    {
        int clampedStep = Mathf.Clamp(step, 0, maxFillSteps);
        CurrentFillStep = clampedStep;

        float targetFill = GetNormalizedStep(clampedStep);

        if (fillRoutine != null)
            StopCoroutine(fillRoutine);

        fillRoutine = StartCoroutine(SmoothFillReactionTube(targetFill, fillDurationAnimation));
    }

    private IEnumerator SmoothFillReactionTube(float targetFill, float duration)
    {
        float startFill = liquidAmount;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            liquidAmount = Mathf.Lerp(startFill, targetFill, elapsed / duration);
            UpdateReactionTubeVisual();
            yield return null;
        }

        liquidAmount = targetFill;
        UpdateReactionTubeVisual();
        fillRoutine = null;
    }

    protected virtual void UpdateReactionTubeVisual()
    {
        if (liquidBody == null)
            return;

        Vector3 scale = liquidScale;
        scale.y = liquidScale.y * liquidAmount;
        liquidBody.localScale = scale;

        Vector3 position = liquidPosition;
        position.y = liquidPosition.y * liquidAmount;
        liquidBody.localPosition = position;

        liquidBody.gameObject.SetActive(liquidAmount > 0.001f);
    }

    private float GetNormalizedStep(int step)
    {
        if (maxFillSteps <= 0)
            return 0f;

        return (float)step / maxFillSteps;
    }
}