using System.Collections;
using UnityEngine;

public class PipetteLiquid : MonoBehaviour
{
    [Header("References")]
    public Transform liquidBody;
    public Renderer liquidRenderer;

    private Vector3 liquidScale;
    private Vector3 liquidPosition;

    [Header("Filling Settings")]
    public float fillDurationAnimation = 0.35f;

    [Header("Emptying Settings")]
    public float emptyDurationAnimation = 0.7f;

    public bool HasLiquid { get; private set; } = false;

    public Bottle SourceBottle { get; private set; } = null;
    public ReagentType CurrentReagent { get; private set; } = ReagentType.None;

    public Beaker SourceBeaker { get; private set; } = null;
    public BeakerLiquid CurrentSolution { get; private set; } = null;

    private float liquidAmount = 0f;
    private Coroutine fillRoutine;

    private void Awake()
    {
        if (liquidBody != null)
        {
            liquidScale = liquidBody.localScale;
            liquidPosition = liquidBody.localPosition;
        }

        HasLiquid = false;
        liquidAmount = 0f;

        SourceBottle = null;
        CurrentReagent = ReagentType.None;

        SourceBeaker = null;
        CurrentSolution = null;

        UpdatePipetteVisual();
    }

    public void EmptyPipette()
    {
        FillPipette(false);

        SourceBottle = null;
        CurrentReagent = ReagentType.None;

        SourceBeaker = null;
        CurrentSolution = null;
    }

    public void FillPipette(bool filled)
    {
        HasLiquid = filled;

        if (!filled)
            CurrentReagent = ReagentType.None;

        float targetFill = filled ? 1f : 0f;
        float duration = filled ? fillDurationAnimation : emptyDurationAnimation;

        if (fillRoutine != null)
            StopCoroutine(fillRoutine);

        fillRoutine = StartCoroutine(SmoothFillPipette(targetFill, duration));
    }

    private IEnumerator SmoothFillPipette(float fillAmount, float duration)
    {
        float startFill = liquidAmount;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            liquidAmount = Mathf.Lerp(startFill, fillAmount, elapsed / duration);
            UpdatePipetteVisual();
            yield return null;
        }

        liquidAmount = fillAmount;
        UpdatePipetteVisual();
        fillRoutine = null;
    }

    public void FillFromBottle(Bottle bottle)
    {
        if (bottle == null || bottle.bottleLiquid == null)
        {
            Debug.Log("[PipetteLiquid] Can't Find Bottle Or Bottle Liquid!");
            return;
        }

        if (HasLiquid)
        {
            Debug.Log("[PipetteLiquid] Pipette Is Already Filled!");
            return;
        }

        if (bottle.bottleLiquid.reagentType == ReagentType.None)
        {
            Debug.Log("[PipetteLiquid] Bottle Has No Reagent Assigned!");
            return;
        }

        SourceBottle = bottle;
        CurrentReagent = bottle.bottleLiquid.reagentType;
        
        SourceBeaker = null;
        CurrentSolution = null;

        CopyColorFromRenderer(bottle.bottleLiquid.liquidRenderer, "Bottle");

        FillPipette(true);
        Debug.Log($"[PipetteLiquid] Pipette Filled With {CurrentReagent}!");
    }

    public void FillFromBeaker(Beaker beaker)
    {
        if (beaker == null || beaker.beakerLiquid == null)
        {
            Debug.Log("[PipetteLiquid] Can't Find Beaker Or Beaker Liquid!");
            return;
        }

        if (HasLiquid)
        {
            Debug.Log("[PipetteLiquid] Pipette Is Already Filled!");
            return;
        }

        if (!beaker.beakerLiquid.HasLiquid)
        {
            Debug.Log("[PipetteLiquid] Beaker Has No Liquid!");
            return;
        }

        SourceBeaker = beaker;
        CurrentSolution = beaker.beakerLiquid;

        SourceBottle = null;
        CurrentReagent = ReagentType.None;

        CopyColorFromRenderer(beaker.beakerLiquid.liquidRenderer, "Beaker");

        FillPipette(true);
        Debug.Log("[PipetteLiquid] Pipette Filled From Beaker!");
    }

    private void CopyColorFromRenderer(Renderer sourceRenderer, string sourceName)
    {
        if (liquidRenderer == null || sourceRenderer == null)
        {
            Debug.Log($"[PipetteLiquid] Missing LiquidRenderer Or {sourceName}LiquidRenderer!");
            return;
        }

        Material sourceMaterial = sourceRenderer.material;
        Material pipetteMaterial = liquidRenderer.material;

        if (sourceMaterial.HasProperty("_Color") && pipetteMaterial.HasProperty("_Color"))
        {
            Color sourceColor = sourceMaterial.GetColor("_Color");
            Color pipetteColor = pipetteMaterial.GetColor("_Color");

            pipetteColor.r = sourceColor.r;
            pipetteColor.g = sourceColor.g;
            pipetteColor.b = sourceColor.b;

            pipetteMaterial.SetColor("_Color", pipetteColor);
            Debug.Log($"[PipetteLiquid] Liquid Color Copied From {sourceName}!");
        }
        else
        {
            Debug.Log("[PipetteLiquid] No _Color Property On Material!");
        }
    }

    private void UpdatePipetteVisual()
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
}