using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoFourDNPHReactionTubeLiquid : ReactionTubeLiquid
{
    public enum TwoFourDNPHState
    {
        Empty,

        AcidOnly,
        EthanolOnly,
        AldehydeOnly,

        AcidAndEthanol,
        AcidAndAldehyde,
        EthanolAndAldehyde,
        AcidAndEthanolAndAldehyde,

        PaleAmberIntermediate,
        LightOrangeIntermediate,

        TwoFourDNPHSolution,
        TwoFourDNPHTest
    }

    public enum TwoFourDNPHVisualState
    {
        Empty,
        Clear,
        PaleYellow,
        PaleAmber,
        LightOrange,
        Amber,
        RedPrecipitate
    }

    private enum TwoFourDNPHAction
    {
        Acid,
        Ethanol,
        Aldehyde,
        PaleAmberIntermediate,
        LightOrangeIntermediate,
        TwoFourDNPHSolution
    }

    [Header("Materials")]
    public Material clearMaterial;
    public Material paleYellowMaterial;
    public Material paleAmberMaterial;
    public Material lightOrangeMaterial;
    public Material amberMaterial;
    public Material redPrecipitateMaterial;

    [Header("Bubbles")]
    [SerializeField] private ParticleSystem liquidBubbles;

    [Header("Bubble Shape Multipliers")]
    [SerializeField] private float bubblePositionYMultiplier = 1.5f;
    [SerializeField] private float bubbleScaleXMultiplier = 0.7857f;
    [SerializeField] private float bubbleScaleYMultiplier = 2.6f;
    [SerializeField] private float bubbleScaleZMultiplier = 0.7857f;

    [Header("Procedure Tracker")]
    [SerializeField] private TwoFourDNPHProcedureTracker procedureTracker;

    [Header("Validation Outline")]
    [SerializeField] private ValidationOutline validationOutline;

    private Coroutine bubbleRoutine;

    public TwoFourDNPHState CurrentState { get; private set; } = TwoFourDNPHState.Empty;
    public TwoFourDNPHVisualState CurrentVisualState { get; private set; } = TwoFourDNPHVisualState.Empty;

    public int AcidCount { get; private set; }
    public int EthanolCount { get; private set; }
    public int AldehydeCount { get; private set; }
    public int PaleAmberIntermediateCount { get; private set; }
    public int LightOrangeIntermediateCount { get; private set; }
    public int TwoFourDNPHSolutionCount { get; private set; }

    public bool HasAcid => AcidCount > 0;
    public bool HasEthanol => EthanolCount > 0;
    public bool HasAldehyde => AldehydeCount > 0;
    public bool HasPaleAmberIntermediate => PaleAmberIntermediateCount > 0;
    public bool HasLightOrangeIntermediate => LightOrangeIntermediateCount > 0;
    public bool HasTwoFourDNPHSolution => TwoFourDNPHSolutionCount > 0;

    private readonly List<TwoFourDNPHAction> reactionSequence = new List<TwoFourDNPHAction>();

    protected override void Awake()
    {
        base.Awake();

        if (procedureTracker == null)
            procedureTracker = GetComponent<TwoFourDNPHProcedureTracker>();

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
        AddReagentToProcedureTracker(_reagent);

        Debug.Log($"[TwoFourDNPHReactionTubeLiquid] Reagent Added: {_reagent}, Reaction State: {CurrentState}, Visual State: {CurrentVisualState}");
        return true;
    }

    public override bool AddSolution(BeakerLiquid _solution)
    {
        if (_solution == null)
            return false;

        if (IsFull())
            return false;

        TwoFourDNPHBeakerLiquid twoFourDNPHSolution = _solution as TwoFourDNPHBeakerLiquid;
        if (twoFourDNPHSolution == null)
            return false;

        if (!AddSolutionToSequence(twoFourDNPHSolution))
            return false;

        UpdateFillStep();
        RefreshVisuals();
        AddSolutionToProcedureTracker();

        Debug.Log($"[TwoFourDNPHReactionTubeLiquid] Solution Added: {_solution.name}, Reaction State: {CurrentState}, Visual State: {CurrentVisualState}");
        return true;
    }

    private bool AddReagentToSequence(ReagentType _reagent)
    {
        switch (_reagent)
        {
            case ReagentType.PhosphoricAcid:
                AcidCount++;
                reactionSequence.Add(TwoFourDNPHAction.Acid);
                return true;

            case ReagentType.Ethanol:
                EthanolCount++;
                reactionSequence.Add(TwoFourDNPHAction.Ethanol);
                return true;

            case ReagentType.PMethoxybenzaldehyde:
                AldehydeCount++;
                reactionSequence.Add(TwoFourDNPHAction.Aldehyde);
                return true;

            default:
                return false;
        }
    }

    private bool AddSolutionToSequence(TwoFourDNPHBeakerLiquid _solution)
    {
        switch (_solution.CurrentState)
        {
            case TwoFourDNPHBeakerLiquid.TwoFourDNPHState.AcidOnly:
            case TwoFourDNPHBeakerLiquid.TwoFourDNPHState.PowderAndAcid:
                AcidCount++;
                reactionSequence.Add(TwoFourDNPHAction.Acid);
                return true;

            case TwoFourDNPHBeakerLiquid.TwoFourDNPHState.EthanolOnly:
            case TwoFourDNPHBeakerLiquid.TwoFourDNPHState.PowderAndEthanol:
                EthanolCount++;
                reactionSequence.Add(TwoFourDNPHAction.Ethanol);
                return true;

            case TwoFourDNPHBeakerLiquid.TwoFourDNPHState.AldehydeOnly:
            case TwoFourDNPHBeakerLiquid.TwoFourDNPHState.PowderAndAldehyde:
                AldehydeCount++;
                reactionSequence.Add(TwoFourDNPHAction.Aldehyde);
                return true;

            case TwoFourDNPHBeakerLiquid.TwoFourDNPHState.AcidAndEthanol:
            case TwoFourDNPHBeakerLiquid.TwoFourDNPHState.PowderAndAcidAndEthanol:
                AcidCount++;
                EthanolCount++;
                reactionSequence.Add(TwoFourDNPHAction.Acid);
                reactionSequence.Add(TwoFourDNPHAction.Ethanol);
                return true;

            case TwoFourDNPHBeakerLiquid.TwoFourDNPHState.AcidAndAldehyde:
            case TwoFourDNPHBeakerLiquid.TwoFourDNPHState.PowderAndAcidAndAldehyde:
                AcidCount++;
                AldehydeCount++;
                reactionSequence.Add(TwoFourDNPHAction.Acid);
                reactionSequence.Add(TwoFourDNPHAction.Aldehyde);
                return true;

            case TwoFourDNPHBeakerLiquid.TwoFourDNPHState.EthanolAndAldehyde:
            case TwoFourDNPHBeakerLiquid.TwoFourDNPHState.PowderAndEthanolAndAldehyde:
                EthanolCount++;
                AldehydeCount++;
                reactionSequence.Add(TwoFourDNPHAction.Ethanol);
                reactionSequence.Add(TwoFourDNPHAction.Aldehyde);
                return true;

            case TwoFourDNPHBeakerLiquid.TwoFourDNPHState.AcidAndEthanolAndAldehyde:
            case TwoFourDNPHBeakerLiquid.TwoFourDNPHState.PowderAndAcidAndEthanolAndAldehyde:
                AcidCount++;
                EthanolCount++;
                AldehydeCount++;
                reactionSequence.Add(TwoFourDNPHAction.Acid);
                reactionSequence.Add(TwoFourDNPHAction.Ethanol);
                reactionSequence.Add(TwoFourDNPHAction.Aldehyde);
                return true;

            case TwoFourDNPHBeakerLiquid.TwoFourDNPHState.PowderAndAcidMixed:
            case TwoFourDNPHBeakerLiquid.TwoFourDNPHState.PowderAndEthanolMixed:
            case TwoFourDNPHBeakerLiquid.TwoFourDNPHState.PowderAndAcidAndEthanolMixed:
            case TwoFourDNPHBeakerLiquid.TwoFourDNPHState.TwoFourDNPHTest:
                PaleAmberIntermediateCount++;
                reactionSequence.Add(TwoFourDNPHAction.PaleAmberIntermediate);
                return true;

            case TwoFourDNPHBeakerLiquid.TwoFourDNPHState.PowderAndAldehydeMixed:
            case TwoFourDNPHBeakerLiquid.TwoFourDNPHState.PowderAndAcidAndAldehydeMixed:
            case TwoFourDNPHBeakerLiquid.TwoFourDNPHState.PowderAndEthanolAndAldehydeMixed:
            case TwoFourDNPHBeakerLiquid.TwoFourDNPHState.PowderAndAcidAndEthanolAndAldehydeMixed:
                LightOrangeIntermediateCount++;
                reactionSequence.Add(TwoFourDNPHAction.LightOrangeIntermediate);
                return true;

            case TwoFourDNPHBeakerLiquid.TwoFourDNPHState.TwoFourDNPHSolution:
                TwoFourDNPHSolutionCount++;
                reactionSequence.Add(TwoFourDNPHAction.TwoFourDNPHSolution);
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

    private int TwoFourDNPHCombinationKey()
    {
        int key = 0;

        if (HasAcid)
            key |= 1;

        if (HasEthanol)
            key |= 2;

        if (HasAldehyde)
            key |= 4;

        return key;
    }

    private bool HasTwoFourDNPHSolutionSequence()
    {
        return reactionSequence.Count == 1 &&
               reactionSequence[0] == TwoFourDNPHAction.TwoFourDNPHSolution;
    }

    private bool HasTwoFourDNPHTestSequence()
    {
        return reactionSequence.Count == 2 &&
               reactionSequence[0] == TwoFourDNPHAction.TwoFourDNPHSolution &&
               reactionSequence[1] == TwoFourDNPHAction.Aldehyde;
    }

    private void UpdateState()
    {
        if (HasTwoFourDNPHTestSequence())
        {
            CurrentState = TwoFourDNPHState.TwoFourDNPHTest;
            return;
        }

        if (HasTwoFourDNPHSolutionSequence())
        {
            CurrentState = TwoFourDNPHState.TwoFourDNPHSolution;
            return;
        }

        UpdateReactionState();
    }

    private void UpdateReactionState()
    {
        if (HasTwoFourDNPHSolution)
        {
            UpdateSolutionCompositionState();
            return;
        }

        if (HasLightOrangeIntermediate)
        {
            CurrentState = TwoFourDNPHState.LightOrangeIntermediate;
            return;
        }

        if (HasPaleAmberIntermediate)
        {
            CurrentState = HasAldehyde
                ? TwoFourDNPHState.LightOrangeIntermediate
                : TwoFourDNPHState.PaleAmberIntermediate;
            return;
        }

        UpdateReagentCompositionState();
    }

    private void UpdateSolutionCompositionState()
    {
        if (HasAldehyde || HasLightOrangeIntermediate)
        {
            CurrentState = TwoFourDNPHState.LightOrangeIntermediate;
            return;
        }

        if (HasAcid || HasEthanol || HasPaleAmberIntermediate)
        {
            CurrentState = TwoFourDNPHState.PaleAmberIntermediate;
            return;
        }

        CurrentState = TwoFourDNPHState.TwoFourDNPHSolution;
    }

    private void UpdateReagentCompositionState()
    {
        switch (TwoFourDNPHCombinationKey())
        {
            case 0:
                CurrentState = TwoFourDNPHState.Empty;
                return;

            case 1:
                CurrentState = TwoFourDNPHState.AcidOnly;
                return;

            case 2:
                CurrentState = TwoFourDNPHState.EthanolOnly;
                return;

            case 4:
                CurrentState = TwoFourDNPHState.AldehydeOnly;
                return;

            case 3:
                CurrentState = TwoFourDNPHState.AcidAndEthanol;
                return;

            case 5:
                CurrentState = TwoFourDNPHState.AcidAndAldehyde;
                return;

            case 6:
                CurrentState = TwoFourDNPHState.EthanolAndAldehyde;
                return;

            case 7:
                CurrentState = TwoFourDNPHState.AcidAndEthanolAndAldehyde;
                return;

            default:
                CurrentState = TwoFourDNPHState.Empty;
                return;
        }
    }

    private void UpdateVisualState()
    {
        switch (CurrentState)
        {
            case TwoFourDNPHState.Empty:
                CurrentVisualState = TwoFourDNPHVisualState.Empty;
                break;

            case TwoFourDNPHState.AcidOnly:
            case TwoFourDNPHState.EthanolOnly:
            case TwoFourDNPHState.AcidAndEthanol:
                CurrentVisualState = TwoFourDNPHVisualState.Clear;
                break;

            case TwoFourDNPHState.AldehydeOnly:
            case TwoFourDNPHState.AcidAndAldehyde:
            case TwoFourDNPHState.EthanolAndAldehyde:
            case TwoFourDNPHState.AcidAndEthanolAndAldehyde:
                CurrentVisualState = TwoFourDNPHVisualState.PaleYellow;
                break;

            case TwoFourDNPHState.PaleAmberIntermediate:
                CurrentVisualState = TwoFourDNPHVisualState.PaleAmber;
                break;

            case TwoFourDNPHState.LightOrangeIntermediate:
                CurrentVisualState = TwoFourDNPHVisualState.LightOrange;
                break;

            case TwoFourDNPHState.TwoFourDNPHSolution:
                CurrentVisualState = TwoFourDNPHVisualState.Amber;
                break;

            case TwoFourDNPHState.TwoFourDNPHTest:
                CurrentVisualState = TwoFourDNPHVisualState.RedPrecipitate;
                break;
        }
    }

    private void UpdateMaterial()
    {
        if (liquidRenderer == null)
            return;

        switch (CurrentVisualState)
        {
            case TwoFourDNPHVisualState.Clear:
                if (clearMaterial != null)
                    liquidRenderer.material = clearMaterial;
                break;

            case TwoFourDNPHVisualState.PaleYellow:
                if (paleYellowMaterial != null)
                    liquidRenderer.material = paleYellowMaterial;
                break;

            case TwoFourDNPHVisualState.PaleAmber:
                if (paleAmberMaterial != null)
                    liquidRenderer.material = paleAmberMaterial;
                break;

            case TwoFourDNPHVisualState.LightOrange:
                if (lightOrangeMaterial != null)
                    liquidRenderer.material = lightOrangeMaterial;
                break;

            case TwoFourDNPHVisualState.Amber:
                if (amberMaterial != null)
                    liquidRenderer.material = amberMaterial;
                break;

            case TwoFourDNPHVisualState.RedPrecipitate:
                if (redPrecipitateMaterial != null)
                    liquidRenderer.material = redPrecipitateMaterial;
                break;

            case TwoFourDNPHVisualState.Empty:
                break;
        }
    }

    private void UpdateBubbles()
    {
        if (liquidBubbles == null)
            return;

        if (CurrentState == TwoFourDNPHState.TwoFourDNPHTest)
        {
            if (bubbleRoutine != null)
                StopCoroutine(bubbleRoutine);

            bubbleRoutine = StartCoroutine(UpdateBubblesAfterFill());
        }
        else
        {
            if (bubbleRoutine != null)
            {
                StopCoroutine(bubbleRoutine);
                bubbleRoutine = null;
            }

            if (liquidBubbles.isPlaying)
                liquidBubbles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private IEnumerator UpdateBubblesAfterFill()
    {
        yield return new WaitForSeconds(fillDurationAnimation);

        UpdateBubbleShapeToLiquid();

        if (!liquidBubbles.isPlaying)
            liquidBubbles.Play();

        bubbleRoutine = null;
    }

    private void UpdateBubbleShapeToLiquid()
    {
        if (liquidBody == null || liquidBubbles == null)
            return;

        var shape = liquidBubbles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;

        Vector3 liquidPosition = liquidBody.localPosition;
        Vector3 liquidScale = liquidBody.localScale;

        shape.position = new Vector3(
            liquidPosition.x,
            liquidPosition.y * bubblePositionYMultiplier,
            liquidPosition.z
        );

        shape.scale = new Vector3(
            liquidScale.x * bubbleScaleXMultiplier,
            liquidScale.y * bubbleScaleYMultiplier,
            liquidScale.z * bubbleScaleZMultiplier
        );
    }

    protected override void UpdateReactionTubeVisual()
    {
        base.UpdateReactionTubeVisual();

        bool showLiquid = CurrentVisualState != TwoFourDNPHVisualState.Empty;

        if (liquidRenderer != null)
            liquidRenderer.enabled = showLiquid && liquidAmount > 0.001f;
    }

    private void RefreshVisuals()
    {
        UpdateState();
        UpdateVisualState();
        UpdateMaterial();
        UpdateReactionTubeVisual();
        UpdateBubbles();
    }

    private void ResetState()
    {
        CurrentState = TwoFourDNPHState.Empty;
        CurrentVisualState = TwoFourDNPHVisualState.Empty;

        AcidCount = 0;
        EthanolCount = 0;
        AldehydeCount = 0;
        PaleAmberIntermediateCount = 0;
        LightOrangeIntermediateCount = 0;
        TwoFourDNPHSolutionCount = 0;

        reactionSequence.Clear();

        if (liquidBubbles != null)
            liquidBubbles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (bubbleRoutine != null)
        {
            StopCoroutine(bubbleRoutine);
            bubbleRoutine = null;
        }
    }

    private TwoFourDNPHActionType GetActionFromReagent(ReagentType _reagent)
    {
        switch (_reagent)
        {
            case ReagentType.PhosphoricAcid:
                return TwoFourDNPHActionType.AddPhosphoricAcid;

            case ReagentType.Ethanol:
                return TwoFourDNPHActionType.AddEthanol;

            case ReagentType.PMethoxybenzaldehyde:
                return TwoFourDNPHActionType.AddPMethoxybenzaldehyde;

            default:
                return TwoFourDNPHActionType.AddPhosphoricAcid;
        }
    }

    private void AddReagentToProcedureTracker(ReagentType _reagent)
    {
        if (procedureTracker != null)
            procedureTracker.RegisterAction(
                TwoFourDNPHContainerType.ReactionTube,
                GetActionFromReagent(_reagent),
                validationOutline);
    }

    private void AddSolutionToProcedureTracker()
    {
        if (procedureTracker != null)
            procedureTracker.RegisterAction(
                TwoFourDNPHContainerType.ReactionTube,
                TwoFourDNPHActionType.AddTwoFourDNPHSolutionToReactionTube,
                validationOutline);
    }
}