using System.Collections.Generic;
using UnityEngine;

public class IodoformReactionTubeLiquid : ReactionTubeLiquid
{
    public enum IodoformReactionState
    {
        Empty,
        ClearIntermediate,
        BrownIntermediate,
        LightBrownIntermediate,
        YellowIntermediate,
        HeatedYellowIntermediate
    }

    public enum IodoformVisualState
    {
        Empty,
        Clear,
        Brown,
        LightBrown,
        YellowPrecipitate,
        HeatedYellowPrecipitate
    }

    [Header("Materials")]
    public Material clearMaterial;
    public Material brownMaterial;
    public Material lightBrownMaterial;
    public Material yellowPrecipitateMaterial;
    public Material heatedYellowPrecipitateMaterial;

    [Header("Heated Bubbles")]
    [SerializeField] private ParticleSystem liquidBubbles;

    [Header("Bubble Shape Multipliers")]
    [SerializeField] private float bubblePositionYMultiplier = 1.5f;
    [SerializeField] private float bubbleScaleXMultiplier = 0.7857f;
    [SerializeField] private float bubbleScaleYMultiplier = 2.6f;
    [SerializeField] private float bubbleScaleZMultiplier = 0.7857f;

    [Header("Procedure Tracker")]
    [SerializeField] private IodoformProcedureTracker procedureTracker;

    [Header("Validation Outline")]
    [SerializeField] private ValidationOutline validationOutline;

    public IodoformReactionState CurrentReactionState { get; private set; } = IodoformReactionState.Empty;
    public IodoformVisualState CurrentVisualState { get; private set; } = IodoformVisualState.Empty;

    public int AcetoneCount { get; private set; }
    public int DistilledWaterCount { get; private set; }
    public int NaOHCount { get; private set; }
    public int IodineCount { get; private set; }
    public int HeatingCount { get; private set; }

    public bool HasAcetone => AcetoneCount > 0;
    public bool HasDistilledWater => DistilledWaterCount > 0;
    public bool HasNaOH => NaOHCount > 0;
    public bool HasIodine => IodineCount > 0;

    private bool wasHeatedCorrectly = false;
    private bool hadIodoformSequence = false;

    private readonly List<ReagentType> reactionSequence = new List<ReagentType>();

    protected override void Awake()
    {
        base.Awake();

        if (procedureTracker == null)
            procedureTracker = GetComponent<IodoformProcedureTracker>();

        if (validationOutline == null)
            validationOutline = GetComponent<ValidationOutline>();

        ResetState();
        RefreshVisuals();
    }

    public override bool AddReagent(ReagentType _reagent)
    {
        if (_reagent == ReagentType.None)
            return false;

        if (IsFull())
            return false;

        if (!AddReagentToSequence(_reagent))
            return false;

        UpdateFillStep();
        RefreshVisuals();

        if (procedureTracker != null)
            procedureTracker.RegisterAction(GetActionFromReagent(_reagent), validationOutline);

        Debug.Log($"[IodoformReactionTubeLiquid] Reagent Added: {_reagent}, Reaction State: {CurrentReactionState}, Visual State: {CurrentVisualState}");
        return true;
    }

    public override void ApplyHeating()
    {
        if (CurrentReactionState == IodoformReactionState.YellowIntermediate)
            wasHeatedCorrectly = true;
        
        RefreshVisuals();

        if (procedureTracker != null)
            procedureTracker.RegisterAction(IodoformActionType.ApplyHeating, validationOutline);

        Debug.Log($"[IodoformReactionTubeLiquid] Heating Applied, Reaction State: {CurrentReactionState}, Visual State: {CurrentVisualState}");
    }

    private bool AddReagentToSequence(ReagentType _reagent)
    {
        reactionSequence.Add(_reagent);

        switch (_reagent)
        {
            case ReagentType.Acetone:
                AcetoneCount++;
                return true;

            case ReagentType.DistilledWater:
                DistilledWaterCount++;
                return true;

            case ReagentType.NaOH:
                NaOHCount++;
                return true;

            case ReagentType.Iodine:
                IodineCount++;
                return true;

            default:
                return false;
        }
    }

    private void UpdateFillStep()
    {
        CurrentFillStep++;
        FillReactionTube(CurrentFillStep);
    }

    private bool HasIodoformSequence()
    {
        return reactionSequence.Count == 4 &&
               reactionSequence[0] == ReagentType.Acetone &&
               reactionSequence[1] == ReagentType.DistilledWater &&
               reactionSequence[2] == ReagentType.NaOH &&
               reactionSequence[3] == ReagentType.Iodine;
    }

    private int IodoformCombinationKey()
    {
        int key = 0;

        if (HasAcetone)
            key |= 1;

        if (HasDistilledWater)
            key |= 2;

        if (HasNaOH)
            key |= 4;

        if (HasIodine)
            key |= 8;

        return key;
    }

    private void UpdateReactionState()
    {
        if (UpdateExtraReactionState())
            return;

        switch (IodoformCombinationKey())
        {
            case 0:
                CurrentReactionState = IodoformReactionState.Empty;
                return;

            case 1: // Acetone
            case 2: // Distilled Water
            case 3: // Acetone & Distilled Water
            case 4: // NaOH
            case 5: // Acetone & NaOH
            case 6: // Distilled Water & NaOH
            case 7: // Acetone & Distilled Water & NaOH
                CurrentReactionState = IodoformReactionState.ClearIntermediate;
                return;

            case 8:  // Iodine
            case 9:  // Acetone & Iodine
            case 10: // Distilled Water & Iodine
            case 11: // Acetone & Distilled Water & Iodine
                CurrentReactionState = IodoformReactionState.BrownIntermediate;
                return;

            case 12: // NaOH & Iodine
            case 13: // Acetone & NaOH & Iodine
            case 14: // Distilled Water & NaOH & Iodine
                CurrentReactionState = IodoformReactionState.LightBrownIntermediate;
                return;

            case 15: // Acetone & Distilled Water & NaOH & Iodine
                if (HasIodoformSequence())
                {
                    hadIodoformSequence = true;

                    CurrentReactionState = wasHeatedCorrectly
                        ? IodoformReactionState.HeatedYellowIntermediate
                        : IodoformReactionState.YellowIntermediate;
                }
                else
                {
                    CurrentReactionState = IodoformReactionState.LightBrownIntermediate;
                }
                return;

            default:
                CurrentReactionState = IodoformReactionState.Empty;
                return;
        }
    }

    private bool UpdateExtraReactionState()
    {
        if (!hadIodoformSequence || !wasHeatedCorrectly)
            return false;

        if (reactionSequence.Count <= 4)
            return false;

        ReagentType lastAddedReagent = reactionSequence[reactionSequence.Count - 1];

        if (lastAddedReagent == ReagentType.Iodine)
        {
            CurrentReactionState = IodoformReactionState.LightBrownIntermediate;
            return true;
        }

        if (lastAddedReagent == ReagentType.Acetone ||
            lastAddedReagent == ReagentType.NaOH ||
            lastAddedReagent == ReagentType.DistilledWater)
        {
            CurrentReactionState = IodoformReactionState.YellowIntermediate;
            return true;
        }

        return false;
    }

    private void UpdateVisualState()
    {
        switch (CurrentReactionState)
        {
            case IodoformReactionState.Empty:
                CurrentVisualState = IodoformVisualState.Empty;
                break;

            case IodoformReactionState.ClearIntermediate:
                CurrentVisualState = IodoformVisualState.Clear;
                break;

            case IodoformReactionState.BrownIntermediate:
                CurrentVisualState = IodoformVisualState.Brown;
                break;

            case IodoformReactionState.LightBrownIntermediate:
                CurrentVisualState = IodoformVisualState.LightBrown;
                break;

            case IodoformReactionState.YellowIntermediate:
                CurrentVisualState = IodoformVisualState.YellowPrecipitate;
                break;

            case IodoformReactionState.HeatedYellowIntermediate:
                CurrentVisualState = IodoformVisualState.HeatedYellowPrecipitate;
                break;
        }
    }

    private void UpdateMaterial()
    {
        if (liquidRenderer == null)
            return;

        switch (CurrentVisualState)
        {
            case IodoformVisualState.Clear:
                if (clearMaterial != null)
                    liquidRenderer.material = clearMaterial;
                break;

            case IodoformVisualState.Brown:
                if (brownMaterial != null)
                    liquidRenderer.material = brownMaterial;
                break;

            case IodoformVisualState.LightBrown:
                if (lightBrownMaterial != null)
                    liquidRenderer.material = lightBrownMaterial;
                break;

            case IodoformVisualState.YellowPrecipitate:
                if (yellowPrecipitateMaterial != null)
                    liquidRenderer.material = yellowPrecipitateMaterial;
                break;

            case IodoformVisualState.HeatedYellowPrecipitate:
                if (heatedYellowPrecipitateMaterial != null)
                    liquidRenderer.material = heatedYellowPrecipitateMaterial;
                break;

            case IodoformVisualState.Empty:
                break;
        }
    }

    private void UpdateBubbles()
    {
        if (liquidBubbles == null)
            return;

        bool showBubbles = CurrentReactionState == IodoformReactionState.HeatedYellowIntermediate;

        if (showBubbles)
        {
            UpdateBubbleShapeToLiquid();

            if (!liquidBubbles.isPlaying)
                liquidBubbles.Play();
        }
        else
        {
            if (liquidBubbles.isPlaying)
                liquidBubbles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void UpdateBubbleShapeToLiquid()
    {
        if (liquidBody == null || liquidBubbles == null)
            return;

        var shape = liquidBubbles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;

        Vector3 currentLiquidPosition = liquidBody.localPosition;
        Vector3 currentLiquidScale = liquidBody.localScale;

        shape.position = new Vector3(
            currentLiquidPosition.x,
            currentLiquidPosition.y * bubblePositionYMultiplier,
            currentLiquidPosition.z
        );

        shape.scale = new Vector3(
            currentLiquidScale.x * bubbleScaleXMultiplier,
            currentLiquidScale.y * bubbleScaleYMultiplier,
            currentLiquidScale.z * bubbleScaleZMultiplier
        );
    }

    protected override void UpdateReactionTubeVisual()
    {
        base.UpdateReactionTubeVisual();

        bool showLiquid = CurrentVisualState != IodoformVisualState.Empty;

        if (liquidRenderer != null)
            liquidRenderer.enabled = showLiquid && liquidAmount > 0.001f;
    }

    private void RefreshVisuals()
    {
        UpdateReactionState();
        UpdateVisualState();
        UpdateMaterial();
        UpdateReactionTubeVisual();
        UpdateBubbles();
    }

    private void ResetState()
    {
        CurrentReactionState = IodoformReactionState.Empty;
        CurrentVisualState = IodoformVisualState.Empty;

        AcetoneCount = 0;
        DistilledWaterCount = 0;
        NaOHCount = 0;
        IodineCount = 0;
        HeatingCount = 0;

        wasHeatedCorrectly = false;
        hadIodoformSequence = false;

        reactionSequence.Clear();

        if (liquidBubbles != null)
            liquidBubbles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private IodoformActionType GetActionFromReagent(ReagentType _reagent)
    {
        switch (_reagent)
        {
            case ReagentType.Acetone:
                return IodoformActionType.AddAcetone;

            case ReagentType.DistilledWater:
                return IodoformActionType.AddDistilledWater;

            case ReagentType.NaOH:
                return IodoformActionType.AddNaOH;

            case ReagentType.Iodine:
                return IodoformActionType.AddIodine;

            default:
                return IodoformActionType.AddAcetone;
        }
    }
}