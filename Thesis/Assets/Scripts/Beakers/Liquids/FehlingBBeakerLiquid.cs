using System.Collections.Generic;
using UnityEngine;

public class FehlingBBeakerLiquid : BeakerLiquid
{
    public enum FehlingBState
    {
        Empty,

        SulfuricAcidOnly,
        NaOHOnly,
        GlucoseOnly,
        SulfuricAcidAndNaOH,
        SulfuricAcidAndGlucose,
        NaOHAndGlucose,
        SulfuricAcidAndNaOHAndGlucose,

        BluePowderOnly,
        WhitePowderOnly,
        BlueAndWhitePowderOnly,

        BluePowderAndSulfuricAcid,
        BluePowderAndNaOH,
        BluePowderAndGlucose,
        BluePowderAndSulfuricAcidAndNaOH,
        BluePowderAndSulfuricAcidAndGlucose,
        BluePowderAndNaOHAndGlucose,
        BluePowderAndSulfuricAcidAndNaOHAndGlucose,

        WhitePowderAndSulfuricAcid,
        WhitePowderAndNaOH,
        WhitePowderAndGlucose,
        WhitePowderAndSulfuricAcidAndNaOH,
        WhitePowderAndSulfuricAcidAndGlucose,
        WhitePowderAndNaOHAndGlucose,
        WhitePowderAndSulfuricAcidAndNaOHAndGlucose,

        BlueAndWhitePowderAndSulfuricAcid,
        BlueAndWhitePowderAndNaOH,
        BlueAndWhitePowderAndGlucose,
        BlueAndWhitePowderAndSulfuricAcidAndNaOH,
        BlueAndWhitePowderAndSulfuricAcidAndGlucose,
        BlueAndWhitePowderAndNaOHAndGlucose,
        BlueAndWhitePowderAndSulfuricAcidAndNaOHAndGlucose,

        BluePowderAndSulfuricAcidMixed,
        BluePowderAndNaOHMixed,
        BluePowderAndGlucoseMixed,
        BluePowderAndSulfuricAcidAndNaOHMixed,
        BluePowderAndSulfuricAcidAndGlucoseMixed,
        BluePowderAndNaOHAndGlucoseMixed,
        BluePowderAndSulfuricAcidAndNaOHAndGlucoseMixed,

        WhitePowderAndSulfuricAcidMixed,
        WhitePowderAndNaOHMixed,
        WhitePowderAndGlucoseMixed,
        WhitePowderAndSulfuricAcidAndNaOHMixed,
        WhitePowderAndSulfuricAcidAndGlucoseMixed,
        WhitePowderAndNaOHAndGlucoseMixed,
        WhitePowderAndSulfuricAcidAndNaOHAndGlucoseMixed,

        BlueAndWhitePowderAndSulfuricAcidMixed,
        BlueAndWhitePowderAndNaOHMixed,
        BlueAndWhitePowderAndGlucoseMixed,
        BlueAndWhitePowderAndSulfuricAcidAndNaOHMixed,
        BlueAndWhitePowderAndSulfuricAcidAndGlucoseMixed,
        BlueAndWhitePowderAndNaOHAndGlucoseMixed,
        BlueAndWhitePowderAndSulfuricAcidAndNaOHAndGlucoseMixed,

        FehlingBSolution
    }

    public enum FehlingBVisualState
    {
        Empty,
        PowderOnly,
        Clear,
        PaleYellow,
        PaleWhite,
        PaleBlue,
        PaleLightBlue,
        Turquoise,
        White
    }

    private enum FehlingBAction
    {
        BluePowder,
        WhitePowder,
        SulfuricAcid,
        NaOH,
        Glucose,
        Mix
    }

    [Header("Materials")]
    [SerializeField] private Material clearMaterial;
    [SerializeField] private Material paleYellowMaterial;
    [SerializeField] private Material paleWhiteMaterial;
    [SerializeField] private Material paleBlueMaterial;
    [SerializeField] private Material paleLightBlueMaterial;
    [SerializeField] private Material turquoiseMaterial;
    [SerializeField] private Material whiteMaterial;

    [Header("Powders")]
    [SerializeField] private GameObject bluePowder;
    [SerializeField] private GameObject whitePowder;

    [Header("Procedure Tracker")]
    [SerializeField] private FehlingProcedureTracker procedureTracker;

    [Header("Validation Outline")]
    [SerializeField] private ValidationOutline validationOutline;

    public FehlingBState CurrentState { get; private set; } = FehlingBState.Empty;
    public FehlingBVisualState CurrentVisualState { get; private set; } = FehlingBVisualState.Empty;

    public int BluePowderCount { get; private set; }
    public int WhitePowderCount { get; private set; }

    public int MixedBluePowderCount { get; private set; }
    public int MixedWhitePowderCount { get; private set; }

    public int SulfuricAcidCount { get; private set; }
    public int NaOHCount { get; private set; }
    public int GlucoseCount { get; private set; }
    public int MixCount { get; private set; }

    public bool HasBluePowder => BluePowderCount > 0;
    public bool HasWhitePowder => WhitePowderCount > 0;

    public bool HasMixedBluePowder => MixedBluePowderCount > 0;
    public bool HasMixedWhitePowder => MixedWhitePowderCount > 0;

    public int UnmixedBluePowderCount => Mathf.Max(0, BluePowderCount - MixedBluePowderCount);
    public int UnmixedWhitePowderCount => Mathf.Max(0, WhitePowderCount - MixedWhitePowderCount);

    public bool HasUnmixedBluePowder => UnmixedBluePowderCount > 0;
    public bool HasUnmixedWhitePowder => UnmixedWhitePowderCount > 0;

    public bool HasSulfuricAcid => SulfuricAcidCount > 0;
    public bool HasNaOH => NaOHCount > 0;
    public bool HasGlucose => GlucoseCount > 0;

    public bool HasPowder => HasBluePowder || HasWhitePowder;
    public bool HasMixedPowder => HasMixedBluePowder || HasMixedWhitePowder;
    public bool HasUnmixedPowder => HasUnmixedBluePowder || HasUnmixedWhitePowder;

    private readonly List<FehlingBAction> reactionSequence = new();

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

        Debug.Log($"[FehlingBBeakerLiquid] Reagent Added: {_reagent}, Reaction State: {CurrentState}, Visual State: {CurrentVisualState}");
        return true;
    }

    public override bool AddSolution(BeakerLiquid _solution)
    {
        return false;
    }

    public override bool AddPowder(Powder _powder)
    {
        if (_powder == null)
            return false;

        switch (_powder.PowderType)
        {
            case PowderType.CopperSulfate:
                if (HasUnmixedBluePowder)
                    return false;

                AddBluePowderToSequence();
                break;

            case PowderType.PotassiumTartrate:
                if (HasUnmixedWhitePowder)
                    return false;

                AddWhitePowderToSequence();
                break;

            default:
                return false;
        }

        RefreshVisuals();
        AddPowderToProcedureTracker(_powder);

        Debug.Log($"[FehlingBBeakerLiquid] Powder Added: {_powder.PowderType}, Reaction State: {CurrentState}, Visual State: {CurrentVisualState}");
        return true;
    }

    public override void ApplyMixing()
    {
        if (GetLiquidCount() == 0)
            return;

        if (!HasUnmixedPowder)
            return;

        AddMixingToSequence();
        RefreshVisuals();
        AddMixingToProcedureTracker();

        Debug.Log($"[FehlingBBeakerLiquid] Mixing Applied, Reaction State: {CurrentState}, Visual State: {CurrentVisualState}");
    }

    public override void EmptyBeaker()
    {
        ResetState();

        CurrentFillStep = 0;
        SetLiquidAmount(0f);

        RefreshVisuals();
    }

    private bool AddReagentToSequence(ReagentType _reagent)
    {
        switch (_reagent)
        {
            case ReagentType.SulfuricAcid:
                SulfuricAcidCount++;
                reactionSequence.Add(FehlingBAction.SulfuricAcid);
                return true;

            case ReagentType.NaOH:
                NaOHCount++;
                reactionSequence.Add(FehlingBAction.NaOH);
                return true;

            case ReagentType.Glucose:
                GlucoseCount++;
                reactionSequence.Add(FehlingBAction.Glucose);
                return true;

            default:
                return false;
        }
    }

    private void AddBluePowderToSequence()
    {
        BluePowderCount++;
        reactionSequence.Add(FehlingBAction.BluePowder);
    }

    private void AddWhitePowderToSequence()
    {
        WhitePowderCount++;
        reactionSequence.Add(FehlingBAction.WhitePowder);
    }

    private void AddMixingToSequence()
    {
        MixCount++;

        MixedBluePowderCount = BluePowderCount;
        MixedWhitePowderCount = WhitePowderCount;

        reactionSequence.Add(FehlingBAction.Mix);
    }

    private void UpdateFillStep()
    {
        CurrentFillStep++;
        FillBeaker(CurrentFillStep);
    }

    private int PowderCombinationKey()
    {
        int key = 0;

        if (HasBluePowder)
            key |= 1;

        if (HasWhitePowder)
            key |= 2;

        return key;
    }

    private int MixedPowderCombinationKey()
    {
        int key = 0;

        if (HasMixedBluePowder)
            key |= 1;

        if (HasMixedWhitePowder)
            key |= 2;

        return key;
    }

    private int UnmixedPowderCombinationKey()
    {
        int key = 0;

        if (HasUnmixedBluePowder)
            key |= 1;

        if (HasUnmixedWhitePowder)
            key |= 2;

        return key;
    }

    private int ReagentCombinationKey()
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

    private bool HasFehlingBSequence()
    {
        return reactionSequence.Count == 3 &&
               reactionSequence[0] == FehlingBAction.WhitePowder &&
               reactionSequence[1] == FehlingBAction.NaOH &&
               reactionSequence[2] == FehlingBAction.Mix;
    }

    private void UpdateState()
    {
        int liquidKey = ReagentCombinationKey();

        if (HasFehlingBSequence())
        {
            CurrentState = FehlingBState.FehlingBSolution;
            return;
        }

        if (HasMixedPowder)
        {
            UpdateMixedPowderState(MixedPowderCombinationKey(), liquidKey);
            return;
        }

        int powderKey = PowderCombinationKey();

        if (powderKey == 0)
        {
            UpdateNoPowderState(liquidKey);
            return;
        }

        UpdateUnmixedPowderState(powderKey, liquidKey);
    }

    private void UpdateNoPowderState(int liquidKey)
    {
        switch (liquidKey)
        {
            case 0:
                CurrentState = FehlingBState.Empty;
                return;
            case 1:
                CurrentState = FehlingBState.SulfuricAcidOnly;
                return;
            case 2:
                CurrentState = FehlingBState.NaOHOnly;
                return;
            case 4:
                CurrentState = FehlingBState.GlucoseOnly;
                return;
            case 3:
                CurrentState = FehlingBState.SulfuricAcidAndNaOH;
                return;
            case 5:
                CurrentState = FehlingBState.SulfuricAcidAndGlucose;
                return;
            case 6:
                CurrentState = FehlingBState.NaOHAndGlucose;
                return;
            case 7:
                CurrentState = FehlingBState.SulfuricAcidAndNaOHAndGlucose;
                return;
            default:
                CurrentState = FehlingBState.Empty;
                return;
        }
    }

    private void UpdateUnmixedPowderState(int powderKey, int liquidKey)
    {
        if (liquidKey == 0)
        {
            switch (powderKey)
            {
                case 1:
                    CurrentState = FehlingBState.BluePowderOnly;
                    return;
                case 2:
                    CurrentState = FehlingBState.WhitePowderOnly;
                    return;
                case 3:
                    CurrentState = FehlingBState.BlueAndWhitePowderOnly;
                    return;
                default:
                    CurrentState = FehlingBState.Empty;
                    return;
            }
        }

        switch (powderKey)
        {
            case 1:
                UpdateUnmixedBluePowderState(liquidKey);
                return;
            case 2:
                UpdateUnmixedWhitePowderState(liquidKey);
                return;
            case 3:
                UpdateUnmixedBlueAndWhitePowderState(liquidKey);
                return;
            default:
                CurrentState = FehlingBState.Empty;
                return;
        }
    }

    private void UpdateMixedPowderState(int powderKey, int liquidKey)
    {
        if (liquidKey == 0)
        {
            switch (powderKey)
            {
                case 1:
                    CurrentState = FehlingBState.BluePowderOnly;
                    return;
                case 2:
                    CurrentState = FehlingBState.WhitePowderOnly;
                    return;
                case 3:
                    CurrentState = FehlingBState.BlueAndWhitePowderOnly;
                    return;
                default:
                    CurrentState = FehlingBState.Empty;
                    return;
            }
        }

        switch (powderKey)
        {
            case 1:
                UpdateMixedBluePowderState(liquidKey);
                return;
            case 2:
                UpdateMixedWhitePowderState(liquidKey);
                return;
            case 3:
                UpdateMixedBlueAndWhitePowderState(liquidKey);
                return;
            default:
                CurrentState = FehlingBState.Empty;
                return;
        }
    }

    private void UpdateUnmixedBluePowderState(int liquidKey)
    {
        switch (liquidKey)
        {
            case 1:
                CurrentState = FehlingBState.BluePowderAndSulfuricAcid;
                return;
            case 2:
                CurrentState = FehlingBState.BluePowderAndNaOH;
                return;
            case 4:
                CurrentState = FehlingBState.BluePowderAndGlucose;
                return;
            case 3:
                CurrentState = FehlingBState.BluePowderAndSulfuricAcidAndNaOH;
                return;
            case 5:
                CurrentState = FehlingBState.BluePowderAndSulfuricAcidAndGlucose;
                return;
            case 6:
                CurrentState = FehlingBState.BluePowderAndNaOHAndGlucose;
                return;
            case 7:
                CurrentState = FehlingBState.BluePowderAndSulfuricAcidAndNaOHAndGlucose;
                return;
            default:
                CurrentState = FehlingBState.BluePowderOnly;
                return;
        }
    }

    private void UpdateUnmixedWhitePowderState(int liquidKey)
    {
        switch (liquidKey)
        {
            case 1:
                CurrentState = FehlingBState.WhitePowderAndSulfuricAcid;
                return;
            case 2:
                CurrentState = FehlingBState.WhitePowderAndNaOH;
                return;
            case 4:
                CurrentState = FehlingBState.WhitePowderAndGlucose;
                return;
            case 3:
                CurrentState = FehlingBState.WhitePowderAndSulfuricAcidAndNaOH;
                return;
            case 5:
                CurrentState = FehlingBState.WhitePowderAndSulfuricAcidAndGlucose;
                return;
            case 6:
                CurrentState = FehlingBState.WhitePowderAndNaOHAndGlucose;
                return;
            case 7:
                CurrentState = FehlingBState.WhitePowderAndSulfuricAcidAndNaOHAndGlucose;
                return;
            default:
                CurrentState = FehlingBState.WhitePowderOnly;
                return;
        }
    }

    private void UpdateUnmixedBlueAndWhitePowderState(int liquidKey)
    {
        switch (liquidKey)
        {
            case 1:
                CurrentState = FehlingBState.BlueAndWhitePowderAndSulfuricAcid;
                return;
            case 2:
                CurrentState = FehlingBState.BlueAndWhitePowderAndNaOH;
                return;
            case 4:
                CurrentState = FehlingBState.BlueAndWhitePowderAndGlucose;
                return;
            case 3:
                CurrentState = FehlingBState.BlueAndWhitePowderAndSulfuricAcidAndNaOH;
                return;
            case 5:
                CurrentState = FehlingBState.BlueAndWhitePowderAndSulfuricAcidAndGlucose;
                return;
            case 6:
                CurrentState = FehlingBState.BlueAndWhitePowderAndNaOHAndGlucose;
                return;
            case 7:
                CurrentState = FehlingBState.BlueAndWhitePowderAndSulfuricAcidAndNaOHAndGlucose;
                return;
            default:
                CurrentState = FehlingBState.BlueAndWhitePowderOnly;
                return;
        }
    }

    private void UpdateMixedBluePowderState(int liquidKey)
    {
        switch (liquidKey)
        {
            case 1:
                CurrentState = FehlingBState.BluePowderAndSulfuricAcidMixed;
                return;
            case 2:
                CurrentState = FehlingBState.BluePowderAndNaOHMixed;
                return;
            case 4:
                CurrentState = FehlingBState.BluePowderAndGlucoseMixed;
                return;
            case 3:
                CurrentState = FehlingBState.BluePowderAndSulfuricAcidAndNaOHMixed;
                return;
            case 5:
                CurrentState = FehlingBState.BluePowderAndSulfuricAcidAndGlucoseMixed;
                return;
            case 6:
                CurrentState = FehlingBState.BluePowderAndNaOHAndGlucoseMixed;
                return;
            case 7:
                CurrentState = FehlingBState.BluePowderAndSulfuricAcidAndNaOHAndGlucoseMixed;
                return;
            default:
                CurrentState = FehlingBState.BluePowderOnly;
                return;
        }
    }

    private void UpdateMixedWhitePowderState(int liquidKey)
    {
        switch (liquidKey)
        {
            case 1:
                CurrentState = FehlingBState.WhitePowderAndSulfuricAcidMixed;
                return;
            case 2:
                CurrentState = FehlingBState.WhitePowderAndNaOHMixed;
                return;
            case 4:
                CurrentState = FehlingBState.WhitePowderAndGlucoseMixed;
                return;
            case 3:
                CurrentState = FehlingBState.WhitePowderAndSulfuricAcidAndNaOHMixed;
                return;
            case 5:
                CurrentState = FehlingBState.WhitePowderAndSulfuricAcidAndGlucoseMixed;
                return;
            case 6:
                CurrentState = FehlingBState.WhitePowderAndNaOHAndGlucoseMixed;
                return;
            case 7:
                CurrentState = FehlingBState.WhitePowderAndSulfuricAcidAndNaOHAndGlucoseMixed;
                return;
            default:
                CurrentState = FehlingBState.WhitePowderOnly;
                return;
        }
    }

    private void UpdateMixedBlueAndWhitePowderState(int liquidKey)
    {
        switch (liquidKey)
        {
            case 1:
                CurrentState = FehlingBState.BlueAndWhitePowderAndSulfuricAcidMixed;
                return;
            case 2:
                CurrentState = FehlingBState.BlueAndWhitePowderAndNaOHMixed;
                return;
            case 4:
                CurrentState = FehlingBState.BlueAndWhitePowderAndGlucoseMixed;
                return;
            case 3:
                CurrentState = FehlingBState.BlueAndWhitePowderAndSulfuricAcidAndNaOHMixed;
                return;
            case 5:
                CurrentState = FehlingBState.BlueAndWhitePowderAndSulfuricAcidAndGlucoseMixed;
                return;
            case 6:
                CurrentState = FehlingBState.BlueAndWhitePowderAndNaOHAndGlucoseMixed;
                return;
            case 7:
                CurrentState = FehlingBState.BlueAndWhitePowderAndSulfuricAcidAndNaOHAndGlucoseMixed;
                return;
            default:
                CurrentState = FehlingBState.BlueAndWhitePowderOnly;
                return;
        }
    }

    private void UpdateVisualState()
    {
        int liquidKey = ReagentCombinationKey();

        if (!HasPowder && liquidKey == 0)
        {
            CurrentVisualState = FehlingBVisualState.Empty;
            return;
        }

        if (!HasMixedPowder)
        {
            CurrentVisualState = liquidKey == 0
                ? FehlingBVisualState.PowderOnly
                : HasGlucose
                    ? FehlingBVisualState.PaleYellow
                    : FehlingBVisualState.Clear;
            return;
        }

        if (HasFehlingBSequence())
        {
            CurrentVisualState = FehlingBVisualState.White;
            return;
        }

        if (HasMixedBluePowder && HasGlucose)
        {
            CurrentVisualState = FehlingBVisualState.Turquoise;
            return;
        }

        if (HasMixedBluePowder && HasMixedWhitePowder)
        {
            CurrentVisualState = FehlingBVisualState.PaleLightBlue;
            return;
        }

        if (HasMixedBluePowder)
        {
            CurrentVisualState = FehlingBVisualState.PaleBlue;
            return;
        }

        if (HasMixedWhitePowder)
        {
            CurrentVisualState = HasGlucose
                ? FehlingBVisualState.PaleYellow
                : FehlingBVisualState.PaleWhite;
            return;
        }

        CurrentVisualState = HasGlucose
            ? FehlingBVisualState.PaleYellow
            : FehlingBVisualState.Clear;
    }

    private void UpdateMaterial()
    {
        if (liquidRenderer == null)
            return;

        switch (CurrentVisualState)
        {
            case FehlingBVisualState.Clear:
                if (clearMaterial != null)
                    liquidRenderer.material = clearMaterial;
                break;

            case FehlingBVisualState.PaleYellow:
                if (paleYellowMaterial != null)
                    liquidRenderer.material = paleYellowMaterial;
                break;

            case FehlingBVisualState.PaleWhite:
                if (paleWhiteMaterial != null)
                    liquidRenderer.material = paleWhiteMaterial;
                break;

            case FehlingBVisualState.White:
                if (whiteMaterial != null)
                    liquidRenderer.material = whiteMaterial;
                break;

            case FehlingBVisualState.PaleBlue:
                if (paleBlueMaterial != null)
                    liquidRenderer.material = paleBlueMaterial;
                break;

            case FehlingBVisualState.PaleLightBlue:
                if (paleLightBlueMaterial != null)
                    liquidRenderer.material = paleLightBlueMaterial;
                break;

            case FehlingBVisualState.Turquoise:
                if (turquoiseMaterial != null)
                    liquidRenderer.material = turquoiseMaterial;
                break;
        }
    }

    private void UpdatePowderVisuals()
    {
        int unmixedPowderKey = UnmixedPowderCombinationKey();

        bool showBlue = unmixedPowderKey == 1 || unmixedPowderKey == 3;
        bool showWhite = unmixedPowderKey == 2 || unmixedPowderKey == 3;

        if (bluePowder != null)
            bluePowder.SetActive(showBlue);

        if (whitePowder != null)
            whitePowder.SetActive(showWhite);

        if (bluePowder != null && showBlue)
        {
            Vector3 position = bluePowder.transform.localPosition;
            position.x = unmixedPowderKey == 3 ? -0.01f : 0f;
            bluePowder.transform.localPosition = position;
        }

        if (whitePowder != null && showWhite)
        {
            Vector3 position = whitePowder.transform.localPosition;
            position.x = unmixedPowderKey == 3 ? 0.01f : 0f;
            whitePowder.transform.localPosition = position;
        }
    }

    protected override void UpdateBeakerVisual()
    {
        base.UpdateBeakerVisual();

        bool showLiquid =
            CurrentVisualState == FehlingBVisualState.Clear ||
            CurrentVisualState == FehlingBVisualState.PaleYellow ||
            CurrentVisualState == FehlingBVisualState.PaleWhite ||
            CurrentVisualState == FehlingBVisualState.White ||
            CurrentVisualState == FehlingBVisualState.PaleBlue ||
            CurrentVisualState == FehlingBVisualState.PaleLightBlue ||
            CurrentVisualState == FehlingBVisualState.Turquoise;

        if (liquidRenderer != null)
            liquidRenderer.enabled = showLiquid && liquidAmount > 0.001f;
    }

    private void RefreshVisuals()
    {
        UpdateState();
        UpdateVisualState();
        UpdateMaterial();
        UpdatePowderVisuals();
        UpdateBeakerVisual();
    }

    private void ResetState()
    {
        CurrentState = FehlingBState.Empty;
        CurrentVisualState = FehlingBVisualState.Empty;

        BluePowderCount = 0;
        WhitePowderCount = 0;

        MixedBluePowderCount = 0;
        MixedWhitePowderCount = 0;

        SulfuricAcidCount = 0;
        NaOHCount = 0;
        GlucoseCount = 0;
        MixCount = 0;

        reactionSequence.Clear();
    }

    private int GetLiquidCount()
    {
        int count = 0;

        if (HasSulfuricAcid) count++;
        if (HasNaOH) count++;
        if (HasGlucose) count++;

        return count;
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

    private FehlingActionType GetActionFromPowder(PowderType _powder)
    {
        switch (_powder)
        {
            case PowderType.CopperSulfate:
                return FehlingActionType.AddCopperSulfate;

            case PowderType.PotassiumTartrate:
                return FehlingActionType.AddPotassiumTartrate;

            default:
                return FehlingActionType.None;
        }
    }

    private void AddReagentToProcedureTracker(ReagentType _reagent)
    {
        if (procedureTracker != null)
            procedureTracker.RegisterAction(
                FehlingContainerType.FehlingBBeaker,
                GetActionFromReagent(_reagent),
                validationOutline);
    }

    private void AddPowderToProcedureTracker(Powder _powder)
    {
        if (procedureTracker != null)
            procedureTracker.RegisterAction(
                FehlingContainerType.FehlingBBeaker,
                GetActionFromPowder(_powder.PowderType),
                validationOutline);
    }

    private void AddMixingToProcedureTracker()
    {
        if (procedureTracker != null)
            procedureTracker.RegisterAction(
                FehlingContainerType.FehlingBBeaker,
                FehlingActionType.ApplyMixingToFehlingB,
                validationOutline);
    }
}