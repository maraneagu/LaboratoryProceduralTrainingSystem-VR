using System.Collections;
using UnityEngine;

public class BeakerLiquid : MonoBehaviour
{
    [Header("References")]
    public Transform liquidBody;
    public Renderer liquidRenderer;

    protected Vector3 liquidScale;
    protected Vector3 liquidPosition;

    [Header("Filling Settings")]
    public float fillPerStep = 0.25f;
    public float fillDurationAnimation = 0.35f;

    [Header("Emptying Settings")]
    public float removePerStep = 0.25f;
    public float removeDurationAnimation = 0.25f;

    protected Coroutine fillRoutine;
    protected Coroutine emptyRoutine;

    public int CurrentFillStep { get; protected set; } = 0;
    protected float liquidAmount = 0f;

    public bool HasLiquid => liquidAmount > 0.001f;

    protected virtual void Awake()
    {
        if (liquidBody != null)
        {
            liquidScale = liquidBody.localScale;
            liquidPosition = liquidBody.localPosition;
        }

        CurrentFillStep = 0;
        liquidAmount = 0f;

        UpdateBeakerVisual();
    }

    public bool IsEmpty()
    {
        return CurrentFillStep == 0;
    }

    public bool IsFull()
    {
        return liquidAmount >= 0.999f;
    }

    public float GetLiquidAmount()
    {
        return liquidAmount;
    }

    public virtual void SetLiquidAmount(float amount)
    {
        liquidAmount = Mathf.Clamp01(amount);

        if (fillPerStep > 0f)
            CurrentFillStep = Mathf.RoundToInt(liquidAmount / fillPerStep);
        else
            CurrentFillStep = 0;

        UpdateBeakerVisual();
    }

    public virtual bool AddReagent(ReagentType reagent)
    {
        Debug.Log("[BeakerLiquid] Reagent Add Not Implemented!");
        return false;
    }

    public virtual bool AddPowder(Powder powder)
    {
        Debug.Log("[BeakerLiquid] Powder Add Not Implemented!");
        return false;
    }

    public virtual bool AddSolution(BeakerLiquid solution)
    {
        Debug.Log("[BeakerLiquid] Solution Add Not Implemented!");
        return false;
    }

    public virtual void ApplyMixing()
    {
        Debug.Log("[BeakerLiquid] Mixing Not Implemented!");
    }

    public virtual void EmptyBeaker()
    {
        CurrentFillStep = 0;
        liquidAmount = 0f;

        if (fillRoutine != null)
            StopCoroutine(fillRoutine);

        if (emptyRoutine != null)
            StopCoroutine(emptyRoutine);

        UpdateBeakerVisual();
    }

    public void FillBeaker(int step)
    {
        CurrentFillStep = Mathf.Max(0, step);

        float targetFill = GetNormalizedStep(CurrentFillStep);

        if (emptyRoutine != null)
            StopCoroutine(emptyRoutine);

        if (fillRoutine != null)
            StopCoroutine(fillRoutine);

        fillRoutine = StartCoroutine(SmoothFillBeaker(targetFill, fillDurationAnimation));
    }

    private IEnumerator SmoothFillBeaker(float targetFill, float duration)
    {
        float startFill = liquidAmount;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            liquidAmount = Mathf.Lerp(startFill, targetFill, elapsed / duration);
            UpdateBeakerVisual();
            yield return null;
        }

        liquidAmount = targetFill;
        UpdateBeakerVisual();
        fillRoutine = null;
    }

    public virtual bool RemoveFromBeaker()
    {
        if (IsEmpty())
            return false;

        float targetFill = Mathf.Clamp01(liquidAmount - removePerStep);

        if (fillPerStep > 0f)
            CurrentFillStep = Mathf.RoundToInt(targetFill / fillPerStep);
        else
            CurrentFillStep = 0;

        if (fillRoutine != null)
            StopCoroutine(fillRoutine);

        if (emptyRoutine != null)
            StopCoroutine(emptyRoutine);

        emptyRoutine = StartCoroutine(SmoothRemoveFromBeaker(targetFill, removeDurationAnimation));
        return true;
    }

    private IEnumerator SmoothRemoveFromBeaker(float targetFill, float duration)
    {
        float startFill = liquidAmount;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            liquidAmount = Mathf.Lerp(startFill, targetFill, elapsed / duration);
            UpdateBeakerVisual();
            yield return null;
        }

        liquidAmount = targetFill;
        UpdateBeakerVisual();
        emptyRoutine = null;
    }

    protected virtual void UpdateBeakerVisual()
    {
        if (liquidBody == null)
            return;

        Vector3 scale = liquidScale;
        scale.y = liquidScale.y * liquidAmount;
        liquidBody.localScale = scale;

        liquidBody.localPosition = liquidPosition;

        if (liquidRenderer != null)
            liquidRenderer.enabled = liquidAmount > 0.001f;
    }

    protected float GetNormalizedStep(int step)
    {
        return Mathf.Clamp01(step * fillPerStep);
    }

    public virtual float GetFillPerStep(BeakerLiquid solution)
    {
        return fillPerStep;
    }
}