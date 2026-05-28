using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BenedictReactionTubeLiquid : ReactionTubeLiquid
{
    public enum BenedictState
    {
        Empty,

        DistilledWaterOnly,
        UrineOnly,

        YellowIntermediate,
        PaleYellowIntermediate,
        WhiteIntermediate,
        PaleBlueIntermediate,
        BlueIntermediate,
        TurquoiseIntermediate,

        BenedictSolution,
        BenedictSolutionAndUrine,

        BenedictTest
    }

    public enum BenedictVisualState
    {
        Empty,
        Clear,
        Yellow,
        PaleYellow,
        White,
        PaleBlue,
        Blue,
        Turquoise,
        BlueSolution,
        UnheatedBlueSolution,
        RedPrecipitate
    }

    private enum BenedictAction
    {
        DistilledWater,
        Urine,
        YellowIntermediate,
        PaleYellowIntermediate,
        WhiteIntermediate,
        PaleBlueIntermediate,
        BlueIntermediate,
        TurquoiseIntermediate,
        BenedictSolution,
        Heat
    }

    [Header("Materials")]
    [SerializeField] private Material clearMaterial;
    [SerializeField] private Material yellowMaterial;
    [SerializeField] private Material paleYellowMaterial;
    [SerializeField] private Material whiteMaterial;
    [SerializeField] private Material paleBlueMaterial;
    [SerializeField] private Material blueMaterial;
    [SerializeField] private Material turquoiseMaterial;
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
    [SerializeField] private BenedictProcedureTracker procedureTracker;

    [Header("Validation Outline")]
    [SerializeField] private ValidationOutline validationOutline;

    private Coroutine bubbleRoutine;

    public BenedictState CurrentState { get; private set; } = BenedictState.Empty;
    public BenedictVisualState CurrentVisualState { get; private set; } = BenedictVisualState.Empty;

    public int DistilledWaterCount { get; private set; }
    public int UrineCount { get; private set; }

    public int YellowIntermediateCount { get; private set; }
    public int PaleYellowIntermediateCount { get; private set; }
    public int WhiteIntermediateCount { get; private set; }
    public int PaleBlueIntermediateCount { get; private set; }
    public int BlueIntermediateCount { get; private set; }
    public int TurquoiseIntermediateCount { get; private set; }

    public int BenedictSolutionCount { get; private set; }

    public bool HasDistilledWater => DistilledWaterCount > 0;
    public bool HasUrine => UrineCount > 0;

    public bool HasYellowIntermediate => YellowIntermediateCount > 0;
    public bool HasPaleYellowIntermediate => PaleYellowIntermediateCount > 0;
    public bool HasWhiteIntermediate => WhiteIntermediateCount > 0;
    public bool HasPaleBlueIntermediate => PaleBlueIntermediateCount > 0;
    public bool HasBlueIntermediate => BlueIntermediateCount > 0;
    public bool HasTurquoiseIntermediate => TurquoiseIntermediateCount > 0;

    public bool HasBenedictSolution => BenedictSolutionCount > 0;

    private readonly List<BenedictAction> reactionSequence = new();

    protected override void Awake()
    {
        base.Awake();

        if (procedureTracker == null)
            procedureTracker = GetComponent<BenedictProcedureTracker>();

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

        Debug.Log($"[BenedictReactionTubeLiquid] Reagent Added: {_reagent}, Reaction State: {CurrentState}, Visual State: {CurrentVisualState}");
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

        Debug.Log($"[BenedictReactionTubeLiquid] Solution Added: {_solution.name}, Reaction State: {CurrentState}, Visual State: {CurrentVisualState}");
        return true;
    }

    public override void ApplyHeating()
    {
        if (CurrentState == BenedictState.BenedictTest)
            return;

        AddHeatingToSequence();
        RefreshVisuals();
        AddHeatingToProcedureTracker();

        Debug.Log($"[BenedictReactionTubeLiquid] Heating Applied, Reaction State: {CurrentState}, Visual State: {CurrentVisualState}");
    }

    private bool AddReagentToSequence(ReagentType _reagent)
    {
        switch (_reagent)
        {
            case ReagentType.DistilledWater:
                DistilledWaterCount++;
                reactionSequence.Add(BenedictAction.DistilledWater);
                return true;

            case ReagentType.Urine:
                UrineCount++;
                reactionSequence.Add(BenedictAction.Urine);
                return true;

            default:
                return false;
        }
    }

    private bool AddSolutionToSequence(BeakerLiquid _solution)
    {
        BenedictBeakerLiquid benedictSolution = _solution as BenedictBeakerLiquid;
        if (benedictSolution != null)
            return AddBenedictSolutionToSequence(benedictSolution);

        return false;
    }

    private bool AddBenedictSolutionToSequence(BenedictBeakerLiquid _solution)
    {
        switch (_solution.CurrentState)
        {
            case BenedictBeakerLiquid.BenedictState.DistilledWaterOnly:
            case BenedictBeakerLiquid.BenedictState.CopperSulfateAndDistilledWater:
            case BenedictBeakerLiquid.BenedictState.CopperSulfateAndSodiumCitrateAndDistilledWater:
            case BenedictBeakerLiquid.BenedictState.CopperSulfateAndSodiumCarbonateAndDistilledWater:
            case BenedictBeakerLiquid.BenedictState.SodiumCitrateAndDistilledWater:
            case BenedictBeakerLiquid.BenedictState.SodiumCitrateAndSodiumCarbonateAndDistilledWater:
            case BenedictBeakerLiquid.BenedictState.SodiumCarbonateAndDistilledWater:
            case BenedictBeakerLiquid.BenedictState.PowdersAndDistilledWater:
                DistilledWaterCount++;
                reactionSequence.Add(BenedictAction.DistilledWater);
                return true;

            case BenedictBeakerLiquid.BenedictState.UrineOnly:
            case BenedictBeakerLiquid.BenedictState.CopperSulfateAndUrine:
            case BenedictBeakerLiquid.BenedictState.CopperSulfateAndSodiumCitrateAndUrine:
            case BenedictBeakerLiquid.BenedictState.CopperSulfateAndSodiumCarbonateAndUrine:
            case BenedictBeakerLiquid.BenedictState.SodiumCitrateAndUrine:
            case BenedictBeakerLiquid.BenedictState.SodiumCitrateAndSodiumCarbonateAndUrine:
            case BenedictBeakerLiquid.BenedictState.SodiumCarbonateAndUrine:
            case BenedictBeakerLiquid.BenedictState.PowdersAndUrine:
                UrineCount++;
                reactionSequence.Add(BenedictAction.Urine);
                return true;

            case BenedictBeakerLiquid.BenedictState.DistilledWaterAndUrine:
            case BenedictBeakerLiquid.BenedictState.CopperSulfateAndDistilledWaterAndUrine:
            case BenedictBeakerLiquid.BenedictState.CopperSulfateAndSodiumCitrateAndDistilledWaterAndUrine:
            case BenedictBeakerLiquid.BenedictState.CopperSulfateAndSodiumCarbonateAndDistilledWaterAndUrine:
            case BenedictBeakerLiquid.BenedictState.SodiumCitrateAndDistilledWaterAndUrine:
            case BenedictBeakerLiquid.BenedictState.SodiumCitrateAndSodiumCarbonateAndDistilledWaterAndUrine:
            case BenedictBeakerLiquid.BenedictState.SodiumCarbonateAndDistilledWaterAndUrine:
            case BenedictBeakerLiquid.BenedictState.PowdersAndDistilledWaterAndUrine:
                YellowIntermediateCount++;
                reactionSequence.Add(BenedictAction.YellowIntermediate);
                return true;

            case BenedictBeakerLiquid.BenedictState.SodiumCitrateAndDistilledWaterMixed:
            case BenedictBeakerLiquid.BenedictState.SodiumCarbonateAndDistilledWaterMixed:
            case BenedictBeakerLiquid.BenedictState.SodiumCitrateAndSodiumCarbonateAndDistilledWaterMixed:
                WhiteIntermediateCount++;
                reactionSequence.Add(BenedictAction.WhiteIntermediate);
                return true;

            case BenedictBeakerLiquid.BenedictState.SodiumCitrateAndUrineMixed:
            case BenedictBeakerLiquid.BenedictState.SodiumCarbonateAndUrineMixed:
            case BenedictBeakerLiquid.BenedictState.SodiumCitrateAndSodiumCarbonateAndUrineMixed:
            case BenedictBeakerLiquid.BenedictState.SodiumCitrateAndDistilledWaterAndUrineMixed:
            case BenedictBeakerLiquid.BenedictState.SodiumCarbonateAndDistilledWaterAndUrineMixed:
            case BenedictBeakerLiquid.BenedictState.SodiumCitrateAndSodiumCarbonateAndDistilledWaterAndUrineMixed:
                PaleYellowIntermediateCount++;
                reactionSequence.Add(BenedictAction.PaleYellowIntermediate);
                return true;

            case BenedictBeakerLiquid.BenedictState.CopperSulfateAndSodiumCitrateAndDistilledWaterMixed:
            case BenedictBeakerLiquid.BenedictState.CopperSulfateAndSodiumCarbonateAndDistilledWaterMixed:
            case BenedictBeakerLiquid.BenedictState.PowdersAndDistilledWaterMixed:
                PaleBlueIntermediateCount++;
                reactionSequence.Add(BenedictAction.PaleBlueIntermediate);
                return true;

            case BenedictBeakerLiquid.BenedictState.CopperSulfateAndDistilledWaterMixed:
                BlueIntermediateCount++;
                reactionSequence.Add(BenedictAction.BlueIntermediate);
                return true;

            case BenedictBeakerLiquid.BenedictState.CopperSulfateAndUrineMixed:
            case BenedictBeakerLiquid.BenedictState.CopperSulfateAndSodiumCitrateAndUrineMixed:
            case BenedictBeakerLiquid.BenedictState.CopperSulfateAndSodiumCarbonateAndUrineMixed:
            case BenedictBeakerLiquid.BenedictState.CopperSulfateAndDistilledWaterAndUrineMixed:
            case BenedictBeakerLiquid.BenedictState.CopperSulfateAndSodiumCitrateAndDistilledWaterAndUrineMixed:
            case BenedictBeakerLiquid.BenedictState.CopperSulfateAndSodiumCarbonateAndDistilledWaterAndUrineMixed:
            case BenedictBeakerLiquid.BenedictState.PowdersAndUrineMixed:
            case BenedictBeakerLiquid.BenedictState.PowdersAndDistilledWaterAndUrineMixed:
                TurquoiseIntermediateCount++;
                reactionSequence.Add(BenedictAction.TurquoiseIntermediate);
                return true;

            case BenedictBeakerLiquid.BenedictState.BenedictSolution:
                BenedictSolutionCount++;
                reactionSequence.Add(BenedictAction.BenedictSolution);
                return true;

            default:
                return false;
        }
    }

    private void AddHeatingToSequence()
    {
        reactionSequence.Add(BenedictAction.Heat);

        if (!HasBenedictTestSequence())
            return;

        CurrentState = BenedictState.BenedictTest;
    }

    private void UpdateFillStep()
    {
        CurrentFillStep++;
        FillReactionTube(CurrentFillStep);
    }

    private bool HasBenedictSolutionSequence()
    {
        return reactionSequence.Count == 1 &&
               reactionSequence[0] == BenedictAction.BenedictSolution;
    }

    private bool HasBenedictSolutionAndUrineSequence()
    {
        return reactionSequence.Count == 2 &&
               reactionSequence[0] == BenedictAction.BenedictSolution &&
               reactionSequence[1] == BenedictAction.Urine;
    }

    private bool HasBenedictTestSequence()
    {
        return reactionSequence.Count == 3 &&
               reactionSequence[0] == BenedictAction.BenedictSolution &&
               reactionSequence[1] == BenedictAction.Urine &&
               reactionSequence[2] == BenedictAction.Heat;
    }

    private void UpdateState()
    {
        if (CurrentState == BenedictState.BenedictTest)
            return;

        if (HasBenedictSolutionAndUrineSequence())
        {
            CurrentState = BenedictState.BenedictSolutionAndUrine;
            return;
        }

        if (HasBenedictSolutionSequence())
        {
            CurrentState = BenedictState.BenedictSolution;
            return;
        }

        UpdateReactionState();
    }

    private void UpdateReactionState()
    {
        if (HasBenedictSolution && HasUrine)
        {
            CurrentState = BenedictState.BenedictSolutionAndUrine;
            return;
        }

        if (HasBenedictSolution)
        {
            CurrentState = BenedictState.BenedictSolution;
            return;
        }

        if (HasTurquoiseIntermediate)
        {
            CurrentState = BenedictState.TurquoiseIntermediate;
            return;
        }

        if (HasBlueIntermediate)
        {
            if (HasUrine)
            {
                CurrentState = BenedictState.TurquoiseIntermediate;
                return;
            }

            CurrentState = BenedictState.BlueIntermediate;
            return;
        }

        if (HasPaleBlueIntermediate)
        {
            if (HasUrine)
            {
                CurrentState = BenedictState.TurquoiseIntermediate;
                return;
            }

            CurrentState = BenedictState.PaleBlueIntermediate;
            return;
        }

        if (HasPaleYellowIntermediate)
        {
            if (HasUrine)
            {
                CurrentState = BenedictState.YellowIntermediate;
                return;
            }

            CurrentState = BenedictState.PaleYellowIntermediate;
            return;
        }

        if (HasWhiteIntermediate)
        {
            if (HasUrine)
            {
                CurrentState = BenedictState.PaleYellowIntermediate;
                return;
            }

            CurrentState = BenedictState.WhiteIntermediate;
            return;
        }

        if (HasUrine)
        {
            CurrentState = BenedictState.UrineOnly;
            return;
        }

        CurrentState = BenedictState.Empty;
    }

    private void UpdateVisualState()
    {
        switch (CurrentState)
        {
            case BenedictState.Empty:
                CurrentVisualState = BenedictVisualState.Empty;
                break;

            case BenedictState.DistilledWaterOnly:
                CurrentVisualState = BenedictVisualState.Clear;
                break;

            case BenedictState.UrineOnly:
                CurrentVisualState = BenedictVisualState.Yellow;
                break;

            case BenedictState.PaleYellowIntermediate:
                CurrentVisualState = BenedictVisualState.PaleYellow;
                break;

            case BenedictState.YellowIntermediate:
                CurrentVisualState = BenedictVisualState.Yellow;
                break;

            case BenedictState.WhiteIntermediate:
                CurrentVisualState = BenedictVisualState.White;
                break;

            case BenedictState.PaleBlueIntermediate:
                CurrentVisualState = BenedictVisualState.PaleBlue;
                break;

            case BenedictState.BlueIntermediate:
                CurrentVisualState = BenedictVisualState.Blue;
                break;

            case BenedictState.TurquoiseIntermediate:
                CurrentVisualState = BenedictVisualState.Turquoise;
                break;

            case BenedictState.BenedictSolution:
                CurrentVisualState = BenedictVisualState.BlueSolution;
                break;

            case BenedictState.BenedictSolutionAndUrine:
                CurrentVisualState = BenedictVisualState.UnheatedBlueSolution;
                break;

            case BenedictState.BenedictTest:
                CurrentVisualState = BenedictVisualState.RedPrecipitate;
                break;
        }
    }

    private void UpdateMaterial()
    {
        if (liquidRenderer == null)
            return;

        switch (CurrentVisualState)
        {
            case BenedictVisualState.Clear:
                if (clearMaterial != null)
                    liquidRenderer.material = clearMaterial;
                break;

            case BenedictVisualState.Yellow:
                if (yellowMaterial != null)
                    liquidRenderer.material = yellowMaterial;
                break;

            case BenedictVisualState.PaleYellow:
                if (paleYellowMaterial != null)
                    liquidRenderer.material = paleYellowMaterial;
                break;

            case BenedictVisualState.White:
                if (whiteMaterial != null)
                    liquidRenderer.material = whiteMaterial;
                break;

            case BenedictVisualState.PaleBlue:
                if (paleBlueMaterial != null)
                    liquidRenderer.material = paleBlueMaterial;
                break;

            case BenedictVisualState.Blue:
                if (blueMaterial != null)
                    liquidRenderer.material = blueMaterial;
                break;

            case BenedictVisualState.Turquoise:
                if (turquoiseMaterial != null)
                    liquidRenderer.material = turquoiseMaterial;
                break;

            case BenedictVisualState.BlueSolution:
                if (blueSolutionMaterial != null)
                    liquidRenderer.material = blueSolutionMaterial;
                break;

            case BenedictVisualState.UnheatedBlueSolution:
                if (unheatedBlueSolutionMaterial != null)
                    liquidRenderer.material = unheatedBlueSolutionMaterial;
                break;

            case BenedictVisualState.RedPrecipitate:
                if (redPrecipitateMaterial != null)
                    liquidRenderer.material = redPrecipitateMaterial;
                break;

            case BenedictVisualState.Empty:
                break;
        }
    }

    private void UpdateBubbles()
    {
        if (liquidBubbles == null)
            return;

        if (CurrentState == BenedictState.BenedictTest)
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

        bool showLiquid = CurrentVisualState != BenedictVisualState.Empty;

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
        CurrentState = BenedictState.Empty;
        CurrentVisualState = BenedictVisualState.Empty;

        UrineCount = 0;

        PaleYellowIntermediateCount = 0;
        WhiteIntermediateCount = 0;
        PaleBlueIntermediateCount = 0;
        BlueIntermediateCount = 0;
        TurquoiseIntermediateCount = 0;

        BenedictSolutionCount = 0;

        reactionSequence.Clear();

        if (liquidBubbles != null)
            liquidBubbles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (bubbleRoutine != null)
        {
            StopCoroutine(bubbleRoutine);
            bubbleRoutine = null;
        }
    }

    private BenedictActionType GetActionFromReagent(ReagentType _reagent)
    {
        switch (_reagent)
        {
            case ReagentType.DistilledWater:
                return BenedictActionType.AddDistilledWater;

            case ReagentType.Urine:
                return BenedictActionType.AddUrine;

            default:
                return BenedictActionType.None;
        }
    }

    private BenedictActionType GetActionFromSolution(BeakerLiquid _solution)
    {
        if (_solution is BenedictBeakerLiquid)
            return BenedictActionType.AddBenedictSolution;

        return BenedictActionType.None;
    }

    private void AddReagentToProcedureTracker(ReagentType _reagent)
    {
        if (procedureTracker != null)
            procedureTracker.RegisterAction(
                BenedictContainerType.ReactionTube,
                GetActionFromReagent(_reagent),
                validationOutline);
    }

    private void AddSolutionToProcedureTracker(BeakerLiquid _solution)
    {
        if (procedureTracker != null)
            procedureTracker.RegisterAction(
                BenedictContainerType.ReactionTube,
                GetActionFromSolution(_solution),
                validationOutline);
    }

    private void AddHeatingToProcedureTracker()
    {
        if (procedureTracker != null)
            procedureTracker.RegisterAction(
                BenedictContainerType.ReactionTube,
                BenedictActionType.ApplyHeating,
                validationOutline);
    }
}