using System.Collections.Generic;
using UnityEngine;

public class FehlingABeakerLiquid : BeakerLiquid
{
    public enum FehlingAState
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

        FehlingASolution
    }

    public enum FehlingAVisualState
    {
        Empty,
        PowderOnly,
        Clear,
        PaleYellow,
        PaleWhite,
        PaleBlue,
        PaleLightBlue,
        Turquoise,
        Blue
    }

    private enum FehlingAAction
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
    [SerializeField] private Material blueMaterial;

    [Header("Powders")]
    [SerializeField] private GameObject copperSulfatePowder;
    [SerializeField] private GameObject potassiumTartratePowder;

    [Header("Procedure Tracker")]
    [SerializeField] private FehlingProcedureTracker procedureTracker;

    [Header("Validation Outline")]
    [SerializeField] private ValidationOutline validationOutline;

    public FehlingAState CurrentState { get; private set; } = FehlingAState.Empty;
    public FehlingAVisualState CurrentVisualState { get; private set; } = FehlingAVisualState.Empty;

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

    private readonly List<FehlingAAction> reactionSequence = new();

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

        Debug.Log($"[FehlingABeakerLiquid] Reagent Added: {_reagent}, Reaction State: {CurrentState}, Visual State: {CurrentVisualState}");
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
        
        Debug.Log($"[FehlingABeakerLiquid] Powder Added: {_powder.PowderType}, Reaction State: {CurrentState}, Visual State: {CurrentVisualState}");
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

        Debug.Log($"[FehlingABeakerLiquid] Mixing Applied, Reaction State: {CurrentState}, Visual State: {CurrentVisualState}");
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
                reactionSequence.Add(FehlingAAction.SulfuricAcid);
                return true;

            case ReagentType.NaOH:
                NaOHCount++;
                reactionSequence.Add(FehlingAAction.NaOH);
                return true;

            case ReagentType.Glucose:
                GlucoseCount++;
                reactionSequence.Add(FehlingAAction.Glucose);
                return true;

            default:
                return false;
        }
    }

    private void AddBluePowderToSequence()
    {
        BluePowderCount++;
        reactionSequence.Add(FehlingAAction.BluePowder);
    }

    private void AddWhitePowderToSequence()
    {
        WhitePowderCount++;
        reactionSequence.Add(FehlingAAction.WhitePowder);
    }

    private void AddMixingToSequence()
    {
        MixCount++;

        MixedBluePowderCount = BluePowderCount;
        MixedWhitePowderCount = WhitePowderCount;

        reactionSequence.Add(FehlingAAction.Mix);
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

    private bool HasFehlingASequence()
    {
        return reactionSequence.Count == 3 &&
               reactionSequence[0] == FehlingAAction.BluePowder &&
               reactionSequence[1] == FehlingAAction.SulfuricAcid &&
               reactionSequence[2] == FehlingAAction.Mix;
    }

    private void UpdateState()
    {
        int liquidKey = ReagentCombinationKey();

        if (HasFehlingASequence())
        {
            CurrentState = FehlingAState.FehlingASolution;
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
                CurrentState = FehlingAState.Empty;
                return;

            case 1:
                CurrentState = FehlingAState.SulfuricAcidOnly;
                return;

            case 2:
                CurrentState = FehlingAState.NaOHOnly;
                return;

            case 4:
                CurrentState = FehlingAState.GlucoseOnly;
                return;

            case 3:
                CurrentState = FehlingAState.SulfuricAcidAndNaOH;
                return;

            case 5:
                CurrentState = FehlingAState.SulfuricAcidAndGlucose;
                return;

            case 6:
                CurrentState = FehlingAState.NaOHAndGlucose;
                return;

            case 7:
                CurrentState = FehlingAState.SulfuricAcidAndNaOHAndGlucose;
                return;

            default:
                CurrentState = FehlingAState.Empty;
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
                    CurrentState = FehlingAState.BluePowderOnly;
                    return;

                case 2:
                    CurrentState = FehlingAState.WhitePowderOnly;
                    return;

                case 3:
                    CurrentState = FehlingAState.BlueAndWhitePowderOnly;
                    return;

                default:
                    CurrentState = FehlingAState.Empty;
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
                CurrentState = FehlingAState.Empty;
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
                    CurrentState = FehlingAState.BluePowderOnly;
                    return;

                case 2:
                    CurrentState = FehlingAState.WhitePowderOnly;
                    return;

                case 3:
                    CurrentState = FehlingAState.BlueAndWhitePowderOnly;
                    return;

                default:
                    CurrentState = FehlingAState.Empty;
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
                CurrentState = FehlingAState.Empty;
                return;
        }
    }

    private void UpdateUnmixedBluePowderState(int liquidKey)
    {
        switch (liquidKey)
        {
            case 1:
                CurrentState = FehlingAState.BluePowderAndSulfuricAcid;
                return;

            case 2:
                CurrentState = FehlingAState.BluePowderAndNaOH;
                return;

            case 4:
                CurrentState = FehlingAState.BluePowderAndGlucose;
                return;

            case 3:
                CurrentState = FehlingAState.BluePowderAndSulfuricAcidAndNaOH;
                return;

            case 5:
                CurrentState = FehlingAState.BluePowderAndSulfuricAcidAndGlucose;
                return;

            case 6:
                CurrentState = FehlingAState.BluePowderAndNaOHAndGlucose;
                return;

            case 7:
                CurrentState = FehlingAState.BluePowderAndSulfuricAcidAndNaOHAndGlucose;
                return;

            default:
                CurrentState = FehlingAState.BluePowderOnly;
                return;
        }
    }

    private void UpdateUnmixedWhitePowderState(int liquidKey)
    {
        switch (liquidKey)
        {
            case 1:
                CurrentState = FehlingAState.WhitePowderAndSulfuricAcid;
                return;

            case 2:
                CurrentState = FehlingAState.WhitePowderAndNaOH;
                return;

            case 4:
                CurrentState = FehlingAState.WhitePowderAndGlucose;
                return;

            case 3:
                CurrentState = FehlingAState.WhitePowderAndSulfuricAcidAndNaOH;
                return;

            case 5:
                CurrentState = FehlingAState.WhitePowderAndSulfuricAcidAndGlucose;
                return;

            case 6:
                CurrentState = FehlingAState.WhitePowderAndNaOHAndGlucose;
                return;

            case 7:
                CurrentState = FehlingAState.WhitePowderAndSulfuricAcidAndNaOHAndGlucose;
                return;

            default:
                CurrentState = FehlingAState.WhitePowderOnly;
                return;
        }
    }

    private void UpdateUnmixedBlueAndWhitePowderState(int liquidKey)
    {
        switch (liquidKey)
        {
            case 1:
                CurrentState = FehlingAState.BlueAndWhitePowderAndSulfuricAcid;
                return;

            case 2:
                CurrentState = FehlingAState.BlueAndWhitePowderAndNaOH;
                return;

            case 4:
                CurrentState = FehlingAState.BlueAndWhitePowderAndGlucose;
                return;

            case 3:
                CurrentState = FehlingAState.BlueAndWhitePowderAndSulfuricAcidAndNaOH;
                return;

            case 5:
                CurrentState = FehlingAState.BlueAndWhitePowderAndSulfuricAcidAndGlucose;
                return;

            case 6:
                CurrentState = FehlingAState.BlueAndWhitePowderAndNaOHAndGlucose;
                return;

            case 7:
                CurrentState = FehlingAState.BlueAndWhitePowderAndSulfuricAcidAndNaOHAndGlucose;
                return;

            default:
                CurrentState = FehlingAState.BlueAndWhitePowderOnly;
                return;
        }
    }

    private void UpdateMixedBluePowderState(int liquidKey)
    {
        switch (liquidKey)
        {
            case 1:
                CurrentState = FehlingAState.BluePowderAndSulfuricAcidMixed;
                return;

            case 2:
                CurrentState = FehlingAState.BluePowderAndNaOHMixed;
                return;

            case 4:
                CurrentState = FehlingAState.BluePowderAndGlucoseMixed;
                return;

            case 3:
                CurrentState = FehlingAState.BluePowderAndSulfuricAcidAndNaOHMixed;
                return;

            case 5:
                CurrentState = FehlingAState.BluePowderAndSulfuricAcidAndGlucoseMixed;
                return;

            case 6:
                CurrentState = FehlingAState.BluePowderAndNaOHAndGlucoseMixed;
                return;

            case 7:
                CurrentState = FehlingAState.BluePowderAndSulfuricAcidAndNaOHAndGlucoseMixed;
                return;

            default:
                CurrentState = FehlingAState.BluePowderOnly;
                return;
        }
    }

    private void UpdateMixedWhitePowderState(int liquidKey)
    {
        switch (liquidKey)
        {
            case 1:
                CurrentState = FehlingAState.WhitePowderAndSulfuricAcidMixed;
                return;

            case 2:
                CurrentState = FehlingAState.WhitePowderAndNaOHMixed;
                return;

            case 4:
                CurrentState = FehlingAState.WhitePowderAndGlucoseMixed;
                return;

            case 3:
                CurrentState = FehlingAState.WhitePowderAndSulfuricAcidAndNaOHMixed;
                return;

            case 5:
                CurrentState = FehlingAState.WhitePowderAndSulfuricAcidAndGlucoseMixed;
                return;

            case 6:
                CurrentState = FehlingAState.WhitePowderAndNaOHAndGlucoseMixed;
                return;

            case 7:
                CurrentState = FehlingAState.WhitePowderAndSulfuricAcidAndNaOHAndGlucoseMixed;
                return;

            default:
                CurrentState = FehlingAState.WhitePowderOnly;
                return;
        }
    }

    private void UpdateMixedBlueAndWhitePowderState(int liquidKey)
    {
        switch (liquidKey)
        {
            case 1:
                CurrentState = FehlingAState.BlueAndWhitePowderAndSulfuricAcidMixed;
                return;

            case 2:
                CurrentState = FehlingAState.BlueAndWhitePowderAndNaOHMixed;
                return;

            case 4:
                CurrentState = FehlingAState.BlueAndWhitePowderAndGlucoseMixed;
                return;

            case 3:
                CurrentState = FehlingAState.BlueAndWhitePowderAndSulfuricAcidAndNaOHMixed;
                return;

            case 5:
                CurrentState = FehlingAState.BlueAndWhitePowderAndSulfuricAcidAndGlucoseMixed;
                return;

            case 6:
                CurrentState = FehlingAState.BlueAndWhitePowderAndNaOHAndGlucoseMixed;
                return;

            case 7:
                CurrentState = FehlingAState.BlueAndWhitePowderAndSulfuricAcidAndNaOHAndGlucoseMixed;
                return;

            default:
                CurrentState = FehlingAState.BlueAndWhitePowderOnly;
                return;
        }
    }

    private void UpdateVisualState()
    {
        int liquidKey = ReagentCombinationKey();

        if (!HasPowder && liquidKey == 0)
        {
            CurrentVisualState = FehlingAVisualState.Empty;
            return;
        }

        if (!HasMixedPowder)
        {
            CurrentVisualState = liquidKey == 0
                ? FehlingAVisualState.PowderOnly
                : HasGlucose
                    ? FehlingAVisualState.PaleYellow
                    : FehlingAVisualState.Clear;
            return;
        }

        if (HasFehlingASequence())
        {
            CurrentVisualState = FehlingAVisualState.Blue;
            return;
        }

        if (HasMixedBluePowder && HasGlucose)
        {
            CurrentVisualState = FehlingAVisualState.Turquoise;
            return;
        }

        if (HasMixedBluePowder && HasMixedWhitePowder)
        {
            CurrentVisualState = FehlingAVisualState.PaleLightBlue;
            return;
        }

        if (HasMixedBluePowder)
        {
            CurrentVisualState = FehlingAVisualState.PaleBlue;
            return;
        }

        if (HasMixedWhitePowder)
        {
            CurrentVisualState = HasGlucose
                ? FehlingAVisualState.PaleYellow
                : FehlingAVisualState.PaleWhite;
            return;
        }

        CurrentVisualState = HasGlucose
            ? FehlingAVisualState.PaleYellow
            : FehlingAVisualState.Clear;
    }

    private void UpdateMaterial()
    {
        if (liquidRenderer == null)
            return;

        switch (CurrentVisualState)
        {
            case FehlingAVisualState.Clear:
                if (clearMaterial != null)
                    liquidRenderer.material = clearMaterial;
                break;

            case FehlingAVisualState.PaleYellow:
                if (paleYellowMaterial != null)
                    liquidRenderer.material = paleYellowMaterial;
                break;

            case FehlingAVisualState.PaleWhite:
                if (paleWhiteMaterial != null)
                    liquidRenderer.material = paleWhiteMaterial;
                break;

            case FehlingAVisualState.PaleBlue:
                if (paleBlueMaterial != null)
                    liquidRenderer.material = paleBlueMaterial;
                break;

            case FehlingAVisualState.PaleLightBlue:
                if (paleLightBlueMaterial != null)
                    liquidRenderer.material = paleLightBlueMaterial;
                break;

            case FehlingAVisualState.Turquoise:
                if (turquoiseMaterial != null)
                    liquidRenderer.material = turquoiseMaterial;
                break;

            case FehlingAVisualState.Blue:
                if (blueMaterial != null)
                    liquidRenderer.material = blueMaterial;
                break;
        }
    }

    private void UpdatePowderVisuals()
    {
        int unmixedPowderKey = UnmixedPowderCombinationKey();

        bool showBlue = unmixedPowderKey == 1 || unmixedPowderKey == 3;
        bool showWhite = unmixedPowderKey == 2 || unmixedPowderKey == 3;

        if (copperSulfatePowder != null)
            copperSulfatePowder.SetActive(showBlue);

        if (potassiumTartratePowder != null)
            potassiumTartratePowder.SetActive(showWhite);

        if (copperSulfatePowder != null && showBlue)
        {
            Vector3 position = copperSulfatePowder.transform.localPosition;
            position.x = unmixedPowderKey == 3 ? -0.01f : 0f;
            copperSulfatePowder.transform.localPosition = position;
        }

        if (potassiumTartratePowder != null && showWhite)
        {
            Vector3 position = potassiumTartratePowder.transform.localPosition;
            position.x = unmixedPowderKey == 3 ? 0.01f : 0f;
            potassiumTartratePowder.transform.localPosition = position;
        }
    }

    protected override void UpdateBeakerVisual()
    {
        base.UpdateBeakerVisual();

        bool showLiquid =
            CurrentVisualState == FehlingAVisualState.Clear ||
            CurrentVisualState == FehlingAVisualState.PaleYellow ||
            CurrentVisualState == FehlingAVisualState.PaleWhite ||
            CurrentVisualState == FehlingAVisualState.PaleBlue ||
            CurrentVisualState == FehlingAVisualState.PaleLightBlue ||
            CurrentVisualState == FehlingAVisualState.Turquoise ||
            CurrentVisualState == FehlingAVisualState.Blue;

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
        CurrentState = FehlingAState.Empty;
        CurrentVisualState = FehlingAVisualState.Empty;

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
                return FehlingActionType.AddSulfuricAcid;
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
                return FehlingActionType.AddCopperSulfate;
        }
    }

    private void AddReagentToProcedureTracker(ReagentType _reagent)
    {
        if (procedureTracker != null)
            procedureTracker.RegisterAction(
                FehlingContainerType.FehlingABeaker,
                GetActionFromReagent(_reagent),
                validationOutline);
    }

    private void AddPowderToProcedureTracker(Powder _powder)
    {
        if (procedureTracker != null)
            procedureTracker.RegisterAction(
                FehlingContainerType.FehlingABeaker,
                GetActionFromPowder(_powder.PowderType),
                validationOutline);
    }

    private void AddMixingToProcedureTracker()
    {
        if (procedureTracker != null)
            procedureTracker.RegisterAction(
                FehlingContainerType.FehlingABeaker,
                FehlingActionType.ApplyMixingToFehlingA,
                validationOutline);
    }
}