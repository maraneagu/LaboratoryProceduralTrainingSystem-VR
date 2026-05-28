using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FehlingReactionTubeLiquid : ReactionTubeLiquid
{
    public enum FehlingState
    {
        Empty,

        SulfuricAcidOnly,
        NaOHOnly,
        GlucoseOnly,
        SulfuricAcidAndNaOH,
        SulfuricAcidAndGlucose,
        NaOHAndGlucose,
        SulfuricAcidAndNaOHAndGlucose,

        PaleWhiteIntermediate,
        PaleBlueIntermediate,
        PaleLightBlueIntermediate,
        TurquoiseIntermediate,
        PaleYellowIntermediate,

        FehlingASolution,
        FehlingBSolution,

        FehlingSolution,
        FehlingSolutionAndGlucose,
        FehlingTest
    }

    public enum FehlingVisualState
    {
        Empty,
        Clear,
        PaleWhite,
        PaleBlue,
        PaleLightBlue,
        Turquoise,
        PaleYellow,
        White,
        Blue,
        BlueSolution,
        UnheatedBlueSolution,
        RedPrecipitate
    }

    private enum FehlingAction
    {
        SulfuricAcid,
        NaOH,
        Glucose,
        Heat,
        PaleWhiteIntermediate,
        PaleBlueIntermediate,
        PaleLightBlueIntermediate,
        TurquoiseIntermediate,
        PaleYellowIntermediate,
        FehlingASolution,
        FehlingBSolution
    }

    [Header("Materials")]
    [SerializeField] private Material clearMaterial;
    [SerializeField] private Material paleWhiteMaterial;
    [SerializeField] private Material paleBlueMaterial;
    [SerializeField] private Material paleLightBlueMaterial;
    [SerializeField] private Material turquoiseMaterial;
    [SerializeField] private Material paleYellowMaterial;
    [SerializeField] private Material whiteMaterial;
    [SerializeField] private Material blueMaterial;
    [SerializeField] private Material blueSolutionMaterial;
    [SerializeField] private Material unheatedBlueSolutionMaterial;
    [SerializeField] private Material redPrecipitateMaterial;

    [Header("Bubbles")]
    [SerializeField] private ParticleSystem liquidBubbles;

    [Header("Bubble Shape Multipliers")]
    [SerializeField] private float bubblePositionYMultiplier = 1.5f;
    [SerializeField] private float bubbleScaleXMultiplier = 0.7857f;
    [SerializeField] private float bubbleScaleYMultiplier = 2.6f;
    [SerializeField] private float bubbleScaleZMultiplier = 0.7857f;

    [Header("Procedure Tracker")]
    [SerializeField] private FehlingProcedureTracker procedureTracker;

    [Header("Validation Outline")]
    [SerializeField] private ValidationOutline validationOutline;

    private Coroutine bubbleRoutine;

    public FehlingState CurrentState { get; private set; } = FehlingState.Empty;
    public FehlingVisualState CurrentVisualState { get; private set; } = FehlingVisualState.Empty;

    public int SulfuricAcidCount { get; private set; }
    public int NaOHCount { get; private set; }
    public int GlucoseCount { get; private set; }

    public int PaleWhiteIntermediateCount { get; private set; }
    public int PaleBlueIntermediateCount { get; private set; }
    public int PaleLightBlueIntermediateCount { get; private set; }
    public int TurquoiseIntermediateCount { get; private set; }
    public int PaleYellowIntermediateCount { get; private set; }

    public int FehlingASolutionCount { get; private set; }
    public int FehlingBSolutionCount { get; private set; }

    public bool HasSulfuricAcid => SulfuricAcidCount > 0;
    public bool HasNaOH => NaOHCount > 0;
    public bool HasGlucose => GlucoseCount > 0;

    public bool HasPaleWhiteIntermediate => PaleWhiteIntermediateCount > 0;
    public bool HasPaleBlueIntermediate => PaleBlueIntermediateCount > 0;
    public bool HasPaleLightBlueIntermediate => PaleLightBlueIntermediateCount > 0;
    public bool HasTurquoiseIntermediate => TurquoiseIntermediateCount > 0;
    public bool HasPaleYellowIntermediate => PaleYellowIntermediateCount > 0;

    public bool HasFehlingASolution => FehlingASolutionCount > 0;
    public bool HasFehlingBSolution => FehlingBSolutionCount > 0;

    private readonly List<FehlingAction> reactionSequence = new();

    protected override void Awake()
    {
        base.Awake();

        if (procedureTracker == null)
            procedureTracker = GetComponent<FehlingProcedureTracker>();

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

        Debug.Log($"[FehlingReactionTubeLiquid] Reagent Added: {_reagent}, Reaction State: {CurrentState}, Visual State: {CurrentVisualState}");
        return true;
    }

    public override bool AddSolution(BeakerLiquid _solution)
    {
        if (_solution == null)
            return false;

        if (IsFull())
            return false;

        if (!AddSolutionToSequence(_solution))
            return false;

        UpdateFillStep();
        RefreshVisuals();
        AddSolutionToProcedureTracker(_solution);

        Debug.Log($"[FehlingReactionTubeLiquid] Solution Added: {_solution.name}, Reaction State: {CurrentState}, Visual State: {CurrentVisualState}");
        return true;
    }

    public override void ApplyHeating()
    {
        if (CurrentState == FehlingState.FehlingTest)
            return;

        AddHeatingToSequence();
        RefreshVisuals();
        AddHeatingToProcedureTracker();

        Debug.Log($"[FehlingReactionTubeLiquid] Heating Applied, Reaction State: {CurrentState}, Visual State: {CurrentVisualState}");
    }

    private bool AddReagentToSequence(ReagentType _reagent)
    {
        switch (_reagent)
        {
            case ReagentType.SulfuricAcid:
                SulfuricAcidCount++;
                reactionSequence.Add(FehlingAction.SulfuricAcid);
                return true;

            case ReagentType.NaOH:
                NaOHCount++;
                reactionSequence.Add(FehlingAction.NaOH);
                return true;

            case ReagentType.Glucose:
                GlucoseCount++;
                reactionSequence.Add(FehlingAction.Glucose);
                return true;

            default:
                return false;
        }
    }

    private bool AddSolutionToSequence(BeakerLiquid _solution)
    {
        FehlingABeakerLiquid fehlingASolution = _solution as FehlingABeakerLiquid;
        if (fehlingASolution != null)
            return AddFehlingASolutionToSequence(fehlingASolution);

        FehlingBBeakerLiquid fehlingBSolution = _solution as FehlingBBeakerLiquid;
        if (fehlingBSolution != null)
            return AddFehlingBSolutionToSequence(fehlingBSolution);

        return false;
    }

    private bool AddFehlingASolutionToSequence(FehlingABeakerLiquid _solution)
    {
        switch (_solution.CurrentState)
        {
            case FehlingABeakerLiquid.FehlingAState.SulfuricAcidOnly:
            case FehlingABeakerLiquid.FehlingAState.BluePowderAndSulfuricAcid:
            case FehlingABeakerLiquid.FehlingAState.WhitePowderAndSulfuricAcid:
            case FehlingABeakerLiquid.FehlingAState.BlueAndWhitePowderAndSulfuricAcid:
                SulfuricAcidCount++;
                reactionSequence.Add(FehlingAction.SulfuricAcid);
                return true;

            case FehlingABeakerLiquid.FehlingAState.NaOHOnly:
            case FehlingABeakerLiquid.FehlingAState.BluePowderAndNaOH:
            case FehlingABeakerLiquid.FehlingAState.WhitePowderAndNaOH:
            case FehlingABeakerLiquid.FehlingAState.BlueAndWhitePowderAndNaOH:
                NaOHCount++;
                reactionSequence.Add(FehlingAction.NaOH);
                return true;

            case FehlingABeakerLiquid.FehlingAState.GlucoseOnly:
            case FehlingABeakerLiquid.FehlingAState.BluePowderAndGlucose:
            case FehlingABeakerLiquid.FehlingAState.WhitePowderAndGlucose:
            case FehlingABeakerLiquid.FehlingAState.BlueAndWhitePowderAndGlucose:
                GlucoseCount++;
                reactionSequence.Add(FehlingAction.Glucose);
                return true;

            case FehlingABeakerLiquid.FehlingAState.SulfuricAcidAndNaOH:
            case FehlingABeakerLiquid.FehlingAState.BluePowderAndSulfuricAcidAndNaOH:
            case FehlingABeakerLiquid.FehlingAState.WhitePowderAndSulfuricAcidAndNaOH:
            case FehlingABeakerLiquid.FehlingAState.BlueAndWhitePowderAndSulfuricAcidAndNaOH:
                SulfuricAcidCount++;
                NaOHCount++;
                reactionSequence.Add(FehlingAction.SulfuricAcid);
                reactionSequence.Add(FehlingAction.NaOH);
                return true;

            case FehlingABeakerLiquid.FehlingAState.SulfuricAcidAndGlucose:
            case FehlingABeakerLiquid.FehlingAState.BluePowderAndSulfuricAcidAndGlucose:
            case FehlingABeakerLiquid.FehlingAState.WhitePowderAndSulfuricAcidAndGlucose:
            case FehlingABeakerLiquid.FehlingAState.BlueAndWhitePowderAndSulfuricAcidAndGlucose:
                PaleYellowIntermediateCount++;
                reactionSequence.Add(FehlingAction.PaleYellowIntermediate);
                return true;

            case FehlingABeakerLiquid.FehlingAState.NaOHAndGlucose:
            case FehlingABeakerLiquid.FehlingAState.BluePowderAndNaOHAndGlucose:
            case FehlingABeakerLiquid.FehlingAState.WhitePowderAndNaOHAndGlucose:
            case FehlingABeakerLiquid.FehlingAState.BlueAndWhitePowderAndNaOHAndGlucose:
                PaleYellowIntermediateCount++;
                reactionSequence.Add(FehlingAction.PaleYellowIntermediate);
                return true;

            case FehlingABeakerLiquid.FehlingAState.SulfuricAcidAndNaOHAndGlucose:
            case FehlingABeakerLiquid.FehlingAState.BluePowderAndSulfuricAcidAndNaOHAndGlucose:
            case FehlingABeakerLiquid.FehlingAState.WhitePowderAndSulfuricAcidAndNaOHAndGlucose:
            case FehlingABeakerLiquid.FehlingAState.BlueAndWhitePowderAndSulfuricAcidAndNaOHAndGlucose:
                PaleYellowIntermediateCount++;
                reactionSequence.Add(FehlingAction.PaleYellowIntermediate);
                return true;

            case FehlingABeakerLiquid.FehlingAState.WhitePowderAndSulfuricAcidMixed:
            case FehlingABeakerLiquid.FehlingAState.WhitePowderAndNaOHMixed:
            case FehlingABeakerLiquid.FehlingAState.WhitePowderAndSulfuricAcidAndNaOHMixed:
                PaleWhiteIntermediateCount++;
                reactionSequence.Add(FehlingAction.PaleWhiteIntermediate);
                return true;

            case FehlingABeakerLiquid.FehlingAState.BluePowderAndSulfuricAcidMixed:
            case FehlingABeakerLiquid.FehlingAState.BluePowderAndNaOHMixed:
            case FehlingABeakerLiquid.FehlingAState.BluePowderAndSulfuricAcidAndNaOHMixed:
                PaleBlueIntermediateCount++;
                reactionSequence.Add(FehlingAction.PaleBlueIntermediate);
                return true;

            case FehlingABeakerLiquid.FehlingAState.BlueAndWhitePowderAndSulfuricAcidMixed:
            case FehlingABeakerLiquid.FehlingAState.BlueAndWhitePowderAndNaOHMixed:
            case FehlingABeakerLiquid.FehlingAState.BlueAndWhitePowderAndSulfuricAcidAndNaOHMixed:
                PaleLightBlueIntermediateCount++;
                reactionSequence.Add(FehlingAction.PaleLightBlueIntermediate);
                return true;

            case FehlingABeakerLiquid.FehlingAState.WhitePowderAndGlucoseMixed:
            case FehlingABeakerLiquid.FehlingAState.WhitePowderAndSulfuricAcidAndGlucoseMixed:
            case FehlingABeakerLiquid.FehlingAState.WhitePowderAndNaOHAndGlucoseMixed:
            case FehlingABeakerLiquid.FehlingAState.WhitePowderAndSulfuricAcidAndNaOHAndGlucoseMixed:
                PaleYellowIntermediateCount++;
                reactionSequence.Add(FehlingAction.PaleYellowIntermediate);
                return true;

            case FehlingABeakerLiquid.FehlingAState.BluePowderAndGlucoseMixed:
            case FehlingABeakerLiquid.FehlingAState.BluePowderAndSulfuricAcidAndGlucoseMixed:
            case FehlingABeakerLiquid.FehlingAState.BluePowderAndNaOHAndGlucoseMixed:
            case FehlingABeakerLiquid.FehlingAState.BluePowderAndSulfuricAcidAndNaOHAndGlucoseMixed:
            case FehlingABeakerLiquid.FehlingAState.BlueAndWhitePowderAndGlucoseMixed:
            case FehlingABeakerLiquid.FehlingAState.BlueAndWhitePowderAndSulfuricAcidAndGlucoseMixed:
            case FehlingABeakerLiquid.FehlingAState.BlueAndWhitePowderAndNaOHAndGlucoseMixed:
            case FehlingABeakerLiquid.FehlingAState.BlueAndWhitePowderAndSulfuricAcidAndNaOHAndGlucoseMixed:
                TurquoiseIntermediateCount++;
                reactionSequence.Add(FehlingAction.TurquoiseIntermediate);
                return true;

            case FehlingABeakerLiquid.FehlingAState.FehlingASolution:
                FehlingASolutionCount++;
                reactionSequence.Add(FehlingAction.FehlingASolution);
                return true;

            default:
                return false;
        }
    }

    private bool AddFehlingBSolutionToSequence(FehlingBBeakerLiquid _solution)
    {
        switch (_solution.CurrentState)
        {
            case FehlingBBeakerLiquid.FehlingBState.SulfuricAcidOnly:
            case FehlingBBeakerLiquid.FehlingBState.BluePowderAndSulfuricAcid:
            case FehlingBBeakerLiquid.FehlingBState.WhitePowderAndSulfuricAcid:
            case FehlingBBeakerLiquid.FehlingBState.BlueAndWhitePowderAndSulfuricAcid:
                SulfuricAcidCount++;
                reactionSequence.Add(FehlingAction.SulfuricAcid);
                return true;

            case FehlingBBeakerLiquid.FehlingBState.NaOHOnly:
            case FehlingBBeakerLiquid.FehlingBState.BluePowderAndNaOH:
            case FehlingBBeakerLiquid.FehlingBState.WhitePowderAndNaOH:
            case FehlingBBeakerLiquid.FehlingBState.BlueAndWhitePowderAndNaOH:
                NaOHCount++;
                reactionSequence.Add(FehlingAction.NaOH);
                return true;

            case FehlingBBeakerLiquid.FehlingBState.GlucoseOnly:
            case FehlingBBeakerLiquid.FehlingBState.BluePowderAndGlucose:
            case FehlingBBeakerLiquid.FehlingBState.WhitePowderAndGlucose:
            case FehlingBBeakerLiquid.FehlingBState.BlueAndWhitePowderAndGlucose:
                GlucoseCount++;
                reactionSequence.Add(FehlingAction.Glucose);
                return true;

            case FehlingBBeakerLiquid.FehlingBState.SulfuricAcidAndNaOH:
            case FehlingBBeakerLiquid.FehlingBState.BluePowderAndSulfuricAcidAndNaOH:
            case FehlingBBeakerLiquid.FehlingBState.WhitePowderAndSulfuricAcidAndNaOH:
            case FehlingBBeakerLiquid.FehlingBState.BlueAndWhitePowderAndSulfuricAcidAndNaOH:
                SulfuricAcidCount++;
                NaOHCount++;
                reactionSequence.Add(FehlingAction.SulfuricAcid);
                reactionSequence.Add(FehlingAction.NaOH);
                return true;

            case FehlingBBeakerLiquid.FehlingBState.SulfuricAcidAndGlucose:
            case FehlingBBeakerLiquid.FehlingBState.BluePowderAndSulfuricAcidAndGlucose:
            case FehlingBBeakerLiquid.FehlingBState.WhitePowderAndSulfuricAcidAndGlucose:
            case FehlingBBeakerLiquid.FehlingBState.BlueAndWhitePowderAndSulfuricAcidAndGlucose:
                PaleYellowIntermediateCount++;
                reactionSequence.Add(FehlingAction.PaleYellowIntermediate);
                return true;

            case FehlingBBeakerLiquid.FehlingBState.NaOHAndGlucose:
            case FehlingBBeakerLiquid.FehlingBState.BluePowderAndNaOHAndGlucose:
            case FehlingBBeakerLiquid.FehlingBState.WhitePowderAndNaOHAndGlucose:
            case FehlingBBeakerLiquid.FehlingBState.BlueAndWhitePowderAndNaOHAndGlucose:
                PaleYellowIntermediateCount++;
                reactionSequence.Add(FehlingAction.PaleYellowIntermediate);
                return true;

            case FehlingBBeakerLiquid.FehlingBState.SulfuricAcidAndNaOHAndGlucose:
            case FehlingBBeakerLiquid.FehlingBState.BluePowderAndSulfuricAcidAndNaOHAndGlucose:
            case FehlingBBeakerLiquid.FehlingBState.WhitePowderAndSulfuricAcidAndNaOHAndGlucose:
            case FehlingBBeakerLiquid.FehlingBState.BlueAndWhitePowderAndSulfuricAcidAndNaOHAndGlucose:
                PaleYellowIntermediateCount++;
                reactionSequence.Add(FehlingAction.PaleYellowIntermediate);
                return true;

            case FehlingBBeakerLiquid.FehlingBState.WhitePowderAndSulfuricAcidMixed:
            case FehlingBBeakerLiquid.FehlingBState.WhitePowderAndNaOHMixed:
            case FehlingBBeakerLiquid.FehlingBState.WhitePowderAndSulfuricAcidAndNaOHMixed:
                PaleWhiteIntermediateCount++;
                reactionSequence.Add(FehlingAction.PaleWhiteIntermediate);
                return true;

            case FehlingBBeakerLiquid.FehlingBState.BluePowderAndSulfuricAcidMixed:
            case FehlingBBeakerLiquid.FehlingBState.BluePowderAndNaOHMixed:
            case FehlingBBeakerLiquid.FehlingBState.BluePowderAndSulfuricAcidAndNaOHMixed:
                PaleBlueIntermediateCount++;
                reactionSequence.Add(FehlingAction.PaleBlueIntermediate);
                return true;

            case FehlingBBeakerLiquid.FehlingBState.BlueAndWhitePowderAndSulfuricAcidMixed:
            case FehlingBBeakerLiquid.FehlingBState.BlueAndWhitePowderAndNaOHMixed:
            case FehlingBBeakerLiquid.FehlingBState.BlueAndWhitePowderAndSulfuricAcidAndNaOHMixed:
                PaleLightBlueIntermediateCount++;
                reactionSequence.Add(FehlingAction.PaleLightBlueIntermediate);
                return true;

            case FehlingBBeakerLiquid.FehlingBState.WhitePowderAndGlucoseMixed:
            case FehlingBBeakerLiquid.FehlingBState.WhitePowderAndSulfuricAcidAndGlucoseMixed:
            case FehlingBBeakerLiquid.FehlingBState.WhitePowderAndNaOHAndGlucoseMixed:
            case FehlingBBeakerLiquid.FehlingBState.WhitePowderAndSulfuricAcidAndNaOHAndGlucoseMixed:
                PaleYellowIntermediateCount++;
                reactionSequence.Add(FehlingAction.PaleYellowIntermediate);
                return true;

            case FehlingBBeakerLiquid.FehlingBState.BluePowderAndGlucoseMixed:
            case FehlingBBeakerLiquid.FehlingBState.BluePowderAndSulfuricAcidAndGlucoseMixed:
            case FehlingBBeakerLiquid.FehlingBState.BluePowderAndNaOHAndGlucoseMixed:
            case FehlingBBeakerLiquid.FehlingBState.BluePowderAndSulfuricAcidAndNaOHAndGlucoseMixed:
            case FehlingBBeakerLiquid.FehlingBState.BlueAndWhitePowderAndGlucoseMixed:
            case FehlingBBeakerLiquid.FehlingBState.BlueAndWhitePowderAndSulfuricAcidAndGlucoseMixed:
            case FehlingBBeakerLiquid.FehlingBState.BlueAndWhitePowderAndNaOHAndGlucoseMixed:
            case FehlingBBeakerLiquid.FehlingBState.BlueAndWhitePowderAndSulfuricAcidAndNaOHAndGlucoseMixed:
                TurquoiseIntermediateCount++;
                reactionSequence.Add(FehlingAction.TurquoiseIntermediate);
                return true;

            case FehlingBBeakerLiquid.FehlingBState.FehlingBSolution:
                FehlingBSolutionCount++;
                reactionSequence.Add(FehlingAction.FehlingBSolution);
                return true;

            default:
                return false;
        }
    }

    private void AddHeatingToSequence()
    {
        reactionSequence.Add(FehlingAction.Heat);

        if (!HasFehlingTestSequence())
            return;

        CurrentState = FehlingState.FehlingTest;
    }

    private void UpdateFillStep()
    {
        CurrentFillStep++;
        FillReactionTube(CurrentFillStep);
    }

    private int FehlingCombinationKey()
    {
        int key = 0;

        if (HasSulfuricAcid)
            key |= 1;

        if (HasNaOH)
            key |= 2;

        if (HasGlucose)
            key |= 4;

        return key;
    }

    private bool HasFehlingSolutionSequence()
    {
        return reactionSequence.Count == 2 &&
               reactionSequence[0] == FehlingAction.FehlingASolution &&
               reactionSequence[1] == FehlingAction.FehlingBSolution;
    }

    private bool HasFehlingSolutionAndGlucoseSequence()
    {
        return reactionSequence.Count == 3 &&
               reactionSequence[0] == FehlingAction.FehlingASolution &&
               reactionSequence[1] == FehlingAction.FehlingBSolution &&
               reactionSequence[2] == FehlingAction.Glucose;
    }

    private bool HasFehlingTestSequence()
    {
        return reactionSequence.Count == 4 &&
               reactionSequence[0] == FehlingAction.FehlingASolution &&
               reactionSequence[1] == FehlingAction.FehlingBSolution &&
               reactionSequence[2] == FehlingAction.Glucose &&
               reactionSequence[3] == FehlingAction.Heat;
    }

    private void UpdateState()
    {
        if (CurrentState == FehlingState.FehlingTest)
            return;

        if (HasFehlingSolutionAndGlucoseSequence())
        {
            CurrentState = FehlingState.FehlingSolutionAndGlucose;
            return;
        }

        if (HasFehlingSolutionSequence())
        {
            CurrentState = FehlingState.FehlingSolution;
            return;
        }

        UpdateReactionState();
    }

    private void UpdateReactionState()
    {
        if (UpdateSolutionCompositionState())
            return;

        if (HasFehlingASolution)
        {
            CurrentState = FehlingState.FehlingASolution;
            return;
        }

        if (HasFehlingBSolution)
        {
            CurrentState = FehlingState.FehlingBSolution;
            return;
        }

        if (HasPaleWhiteIntermediate)
        {
            CurrentState = FehlingState.PaleWhiteIntermediate;
            return;
        }

        if (HasPaleYellowIntermediate)
        {
            CurrentState = FehlingState.PaleYellowIntermediate;
            return;
        }

        if (HasTurquoiseIntermediate)
        {
            CurrentState = FehlingState.TurquoiseIntermediate;
            return;
        }

        if (HasPaleLightBlueIntermediate)
        {
            CurrentState = FehlingState.PaleLightBlueIntermediate;
            return;
        }

        if (HasPaleBlueIntermediate)
        {
            CurrentState = FehlingState.PaleBlueIntermediate;
            return;
        }

        UpdateReagentCompositionState();
    }

    private bool UpdateSolutionCompositionState()
    {
        if (UpdateFehlingAAndFehlingBCompositionState())
            return true;

        if (UpdateFehlingACompositionState())
            return true;

        if (UpdateFehlingBCompositionState())
            return true;

        if (UpdatePaleWhiteCompositionState())
            return true;

        if (UpdatePaleYellowCompositionState())
            return true;

        if (UpdatePaleLightBlueCompositionState())
            return true;

        if (UpdatePaleBlueCompositionState())
            return true;

        return false;
    }

    private bool UpdateFehlingAAndFehlingBCompositionState()
    {
        if (HasFehlingASolution && HasFehlingBSolution)
        {
            CurrentState = HasGlucose
                ? FehlingState.FehlingSolutionAndGlucose
                : FehlingState.FehlingSolution;
            return true;
        }

        return false;
    }

    private bool UpdateFehlingACompositionState()
    {
        if (HasFehlingASolution && (HasGlucose || HasPaleYellowIntermediate || HasTurquoiseIntermediate))
        {
            CurrentState = FehlingState.TurquoiseIntermediate;
            return true;
        }

        if (HasFehlingASolution && (HasPaleWhiteIntermediate || HasPaleLightBlueIntermediate))
        {
            CurrentState = FehlingState.PaleLightBlueIntermediate;
            return true;
        }

        if (HasFehlingASolution && (HasSulfuricAcid || HasNaOH || HasPaleBlueIntermediate))
        {
            CurrentState = FehlingState.PaleBlueIntermediate;
            return true;
        }

        return false;
    }

    private bool UpdateFehlingBCompositionState()
    {
        if (HasFehlingBSolution && (HasGlucose && (HasPaleLightBlueIntermediate || HasPaleBlueIntermediate)) || HasTurquoiseIntermediate)
        {
            CurrentState = FehlingState.TurquoiseIntermediate;
            return true;
        }

        if (HasFehlingBSolution && (HasPaleBlueIntermediate || HasPaleLightBlueIntermediate))
        {
            CurrentState = FehlingState.PaleLightBlueIntermediate;
            return true;
        }

        if (HasFehlingBSolution && (HasGlucose || HasPaleYellowIntermediate))
        {
            CurrentState = FehlingState.PaleYellowIntermediate;
            return true;
        }

        if (HasFehlingBSolution && (HasSulfuricAcid || HasNaOH || HasPaleWhiteIntermediate))
        {
            CurrentState = FehlingState.PaleWhiteIntermediate;
            return true;
        }

        return false;
    }

    private bool UpdatePaleWhiteCompositionState()
    {
        if (HasPaleWhiteIntermediate && (HasGlucose && (HasPaleLightBlueIntermediate || HasPaleBlueIntermediate)) || HasTurquoiseIntermediate)
        {
            CurrentState = FehlingState.TurquoiseIntermediate;
            return true;
        }

        if (HasPaleWhiteIntermediate && (HasPaleLightBlueIntermediate || HasPaleBlueIntermediate))
        {
            CurrentState = FehlingState.PaleLightBlueIntermediate;
            return true;
        }

        if (HasPaleWhiteIntermediate && (HasGlucose || HasPaleYellowIntermediate))
        {
            CurrentState = FehlingState.PaleYellowIntermediate;
            return true;
        }

        return false;
    }

    private bool UpdatePaleYellowCompositionState()
    {
        if (HasPaleYellowIntermediate && (HasPaleLightBlueIntermediate || HasPaleBlueIntermediate || HasTurquoiseIntermediate))
        {
            CurrentState = FehlingState.TurquoiseIntermediate;
            return true;
        }

        return false;
    }

    private bool UpdatePaleLightBlueCompositionState()
    {
        if (HasPaleLightBlueIntermediate && (HasGlucose || HasTurquoiseIntermediate))
        {
            CurrentState = FehlingState.TurquoiseIntermediate;
            return true;
        }

        if (HasPaleLightBlueIntermediate && HasPaleBlueIntermediate)
        {
            CurrentState = FehlingState.PaleLightBlueIntermediate;
            return true;
        }

        return false;
    }

    private bool UpdatePaleBlueCompositionState()
    { 
        if (HasPaleBlueIntermediate && (HasGlucose || HasTurquoiseIntermediate))
        {
            CurrentState = FehlingState.TurquoiseIntermediate;
            return true;
        }

        return false;
    }

    private void UpdateReagentCompositionState()
    {
        switch (FehlingCombinationKey())
        {
            case 0:
                CurrentState = FehlingState.Empty;
                return;

            case 1:
                CurrentState = FehlingState.SulfuricAcidOnly;
                return;

            case 2:
                CurrentState = FehlingState.NaOHOnly;
                return;

            case 4:
                CurrentState = FehlingState.GlucoseOnly;
                return;

            case 3:
                CurrentState = FehlingState.SulfuricAcidAndNaOH;
                return;

            case 5:
                CurrentState = FehlingState.SulfuricAcidAndGlucose;
                return;

            case 6:
                CurrentState = FehlingState.NaOHAndGlucose;
                return;

            case 7:
                CurrentState = FehlingState.SulfuricAcidAndNaOHAndGlucose;
                return;

            default:
                CurrentState = FehlingState.Empty;
                return;
        }
    }

    private void UpdateVisualState()
    {
        switch (CurrentState)
        {
            case FehlingState.Empty:
                CurrentVisualState = FehlingVisualState.Empty;
                break;

            case FehlingState.SulfuricAcidOnly:
            case FehlingState.NaOHOnly:
            case FehlingState.SulfuricAcidAndNaOH:
                CurrentVisualState = FehlingVisualState.Clear;
                break;

            case FehlingState.PaleWhiteIntermediate:
                CurrentVisualState = FehlingVisualState.PaleWhite;
                break;

            case FehlingState.PaleBlueIntermediate:
                CurrentVisualState = FehlingVisualState.PaleBlue;
                break;

            case FehlingState.PaleLightBlueIntermediate:
                CurrentVisualState = FehlingVisualState.PaleLightBlue;
                break;

            case FehlingState.TurquoiseIntermediate:
                CurrentVisualState = FehlingVisualState.Turquoise;
                break;

            case FehlingState.GlucoseOnly:
            case FehlingState.SulfuricAcidAndGlucose:
            case FehlingState.NaOHAndGlucose:
            case FehlingState.SulfuricAcidAndNaOHAndGlucose:
            case FehlingState.PaleYellowIntermediate:
                CurrentVisualState = FehlingVisualState.PaleYellow;
                break;

            case FehlingState.FehlingASolution:
                CurrentVisualState = FehlingVisualState.Blue;
                break;

            case FehlingState.FehlingBSolution:
                CurrentVisualState = FehlingVisualState.White;
                break;

            case FehlingState.FehlingSolution:
                CurrentVisualState = FehlingVisualState.BlueSolution;
                break;

            case FehlingState.FehlingSolutionAndGlucose:
                CurrentVisualState = FehlingVisualState.UnheatedBlueSolution;
                break;

            case FehlingState.FehlingTest:
                CurrentVisualState = FehlingVisualState.RedPrecipitate;
                break;
        }
    }

    private void UpdateMaterial()
    {
        if (liquidRenderer == null)
            return;

        switch (CurrentVisualState)
        {
            case FehlingVisualState.Clear:
                if (clearMaterial != null)
                    liquidRenderer.material = clearMaterial;
                break;

            case FehlingVisualState.PaleWhite:
                if (paleWhiteMaterial != null)
                    liquidRenderer.material = paleWhiteMaterial;
                break;

            case FehlingVisualState.White:
                if (whiteMaterial != null)
                    liquidRenderer.material = whiteMaterial;
                break;

            case FehlingVisualState.PaleBlue:
                if (paleBlueMaterial != null)
                    liquidRenderer.material = paleBlueMaterial;
                break;

            case FehlingVisualState.PaleLightBlue:
                if (paleLightBlueMaterial != null)
                    liquidRenderer.material = paleLightBlueMaterial;
                break;

            case FehlingVisualState.Turquoise:
                if (turquoiseMaterial != null)
                    liquidRenderer.material = turquoiseMaterial;
                break;

            case FehlingVisualState.PaleYellow:
                if (paleYellowMaterial != null)
                    liquidRenderer.material = paleYellowMaterial;
                break;

            case FehlingVisualState.Blue:
                if (blueMaterial != null)
                    liquidRenderer.material = blueMaterial;
                break;

            case FehlingVisualState.BlueSolution:
                if (blueSolutionMaterial != null)
                    liquidRenderer.material = blueSolutionMaterial;
                break;

            case FehlingVisualState.UnheatedBlueSolution:
                if (unheatedBlueSolutionMaterial != null)
                    liquidRenderer.material = unheatedBlueSolutionMaterial;
                break;

            case FehlingVisualState.RedPrecipitate:
                if (redPrecipitateMaterial != null)
                    liquidRenderer.material = redPrecipitateMaterial;
                break;

            case FehlingVisualState.Empty:
                break;
        }
    }

    private void UpdateBubbles()
    {
        if (liquidBubbles == null)
            return;

        if (CurrentState == FehlingState.FehlingTest)
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

        bool showLiquid = CurrentVisualState != FehlingVisualState.Empty;

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
        CurrentState = FehlingState.Empty;
        CurrentVisualState = FehlingVisualState.Empty;

        SulfuricAcidCount = 0;
        NaOHCount = 0;
        GlucoseCount = 0;

        PaleWhiteIntermediateCount = 0;
        PaleBlueIntermediateCount = 0;
        PaleLightBlueIntermediateCount = 0;
        TurquoiseIntermediateCount = 0;
        PaleYellowIntermediateCount = 0;

        FehlingASolutionCount = 0;
        FehlingBSolutionCount = 0;

        reactionSequence.Clear();

        if (liquidBubbles != null)
            liquidBubbles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (bubbleRoutine != null)
        {
            StopCoroutine(bubbleRoutine);
            bubbleRoutine = null;
        }
    }

    private FehlingActionType GetActionFromReagent(ReagentType _reagent)
    {
        switch (_reagent)
        {
            case ReagentType.SulfuricAcid:
                return FehlingActionType.AddSulfuricAcid;

            case ReagentType.NaOH:
                return FehlingActionType.AddNaOH;

            case ReagentType.Glucose:
                return FehlingActionType.AddGlucose;

            default:
                return FehlingActionType.None;
        }
    }

    private FehlingActionType GetActionFromSolution(BeakerLiquid _solution)
    {
        if (_solution is FehlingABeakerLiquid)
            return FehlingActionType.AddFehlingA;

        if (_solution is FehlingBBeakerLiquid)
            return FehlingActionType.AddFehlingB;

        return FehlingActionType.None;
    }

    private void AddReagentToProcedureTracker(ReagentType _reagent)
    {
        if (procedureTracker != null)
            procedureTracker.RegisterAction(
                FehlingContainerType.ReactionTube,
                GetActionFromReagent(_reagent),
                validationOutline);
    }

    private void AddSolutionToProcedureTracker(BeakerLiquid _solution)
    {
        if (procedureTracker != null)
            procedureTracker.RegisterAction(
                FehlingContainerType.ReactionTube,
                GetActionFromSolution(_solution),
                validationOutline);
    }

    private void AddHeatingToProcedureTracker()
    {
        if (procedureTracker != null)
            procedureTracker.RegisterAction(
                FehlingContainerType.ReactionTube,
                FehlingActionType.ApplyHeating,
                validationOutline);
    }
}