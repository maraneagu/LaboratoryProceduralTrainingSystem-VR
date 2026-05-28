using System.Collections.Generic;
using UnityEngine;

public class TwoFourDNPHBeakerLiquid : BeakerLiquid
{
    public enum TwoFourDNPHState
    {
        Empty,

        PowderOnly,

        AcidOnly,
        EthanolOnly,
        AldehydeOnly,

        AcidAndEthanol,
        AcidAndAldehyde,
        EthanolAndAldehyde,
        AcidAndEthanolAndAldehyde,

        PowderAndAcid,
        PowderAndEthanol,
        PowderAndAldehyde,

        PowderAndAcidAndEthanol,
        PowderAndAcidAndAldehyde,
        PowderAndEthanolAndAldehyde,
        PowderAndAcidAndEthanolAndAldehyde,

        PowderAndAcidMixed,
        PowderAndEthanolMixed,
        PowderAndAldehydeMixed,

        PowderAndAcidAndEthanolMixed,
        PowderAndAcidAndAldehydeMixed,
        PowderAndEthanolAndAldehydeMixed,
        PowderAndAcidAndEthanolAndAldehydeMixed,

        TwoFourDNPHSolution,
        TwoFourDNPHTest
    }

    public enum TwoFourDNPHVisualState
    {
        Empty,
        PowderOnly,
        Clear,
        PaleYellow,
        PaleAmber,
        LightOrange,
        Amber
    }

    private enum TwoFourDNPHAction
    {
        Powder,
        Acid,
        Ethanol,
        Aldehyde,
        Mix
    }

    [Header("Materials")]
    [SerializeField] private Material clearMaterial;
    [SerializeField] private Material paleYellowMaterial;
    [SerializeField] private Material paleAmberMaterial;
    [SerializeField] private Material lightOrangeMaterial;
    [SerializeField] private Material amberMaterial;

    [Header("Powder")]
    [SerializeField] private GameObject powder;

    [Header("Procedure Tracker")]
    [SerializeField] private TwoFourDNPHProcedureTracker procedureTracker;

    [Header("Validation Outline")]
    [SerializeField] private ValidationOutline validationOutline;

    public TwoFourDNPHState CurrentState { get; private set; } = TwoFourDNPHState.Empty;
    public TwoFourDNPHVisualState CurrentVisualState { get; private set; } = TwoFourDNPHVisualState.Empty;

    public int PowderCount { get; private set; }
    public int MixedPowderCount { get; private set; }

    public int AcidCount { get; private set; }
    public int EthanolCount { get; private set; }
    public int AldehydeCount { get; private set; }
    public int MixCount { get; private set; }

    public bool HasPowder => PowderCount > 0;
    public bool HasMixedPowder => MixedPowderCount > 0;

    public int UnmixedPowderCount => Mathf.Max(0, PowderCount - MixedPowderCount);
    public bool HasUnmixedPowder => UnmixedPowderCount > 0;

    public bool HasAcid => AcidCount > 0;
    public bool HasEthanol => EthanolCount > 0;
    public bool HasAldehyde => AldehydeCount > 0;

    private readonly List<TwoFourDNPHAction> reactionSequence = new();

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

        Debug.Log($"[TwoFourDNPHBeakerLiquid] Reagent Added: {_reagent}, Reaction State: {CurrentState}, Visual State: {CurrentVisualState}");
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

        AddPowderToSequence();
        RefreshVisuals();
        AddPowderToProcedureTracker();

        Debug.Log($"[TwoFourDNPHBeakerLiquid] Powder Added: {_powder.PowderType}, Reaction State: {CurrentState}, Visual State: {CurrentVisualState}");
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

        Debug.Log($"[TwoFourDNPHBeakerLiquid] Mixing Applied, Reaction State: {CurrentState}, Visual State: {CurrentVisualState}");
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

    private void AddPowderToSequence()
    {
        PowderCount++;
        reactionSequence.Add(TwoFourDNPHAction.Powder);
    }

    private void AddMixingToSequence()
    {
        MixCount++;
        MixedPowderCount = PowderCount;
        reactionSequence.Add(TwoFourDNPHAction.Mix);
    }

    private void UpdateFillStep()
    {
        CurrentFillStep++;
        FillBeaker(CurrentFillStep);
    }

    private bool HasTwoFourDNPHSequence()
    {
        return reactionSequence.Count == 4 &&
               reactionSequence[0] == TwoFourDNPHAction.Powder &&
               reactionSequence[1] == TwoFourDNPHAction.Acid &&
               reactionSequence[2] == TwoFourDNPHAction.Mix &&
               reactionSequence[3] == TwoFourDNPHAction.Ethanol;
    }

    private bool HasTwoFourDNPHTestSequence()
    {
        return reactionSequence.Count == 5 &&
               reactionSequence[0] == TwoFourDNPHAction.Powder &&
               reactionSequence[1] == TwoFourDNPHAction.Acid &&
               reactionSequence[2] == TwoFourDNPHAction.Mix &&
               reactionSequence[3] == TwoFourDNPHAction.Ethanol &&
               reactionSequence[4] == TwoFourDNPHAction.Aldehyde;
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

    private void UpdateState()
    {
        if (HasTwoFourDNPHTestSequence())
        {
            CurrentState = TwoFourDNPHState.TwoFourDNPHTest;
            return;
        }

        if (HasTwoFourDNPHSequence())
        {
            CurrentState = TwoFourDNPHState.TwoFourDNPHSolution;
            return;
        }

        UpdateReactionState();
    }

    private void UpdateReactionState()
    {
        int key = TwoFourDNPHCombinationKey();

        if (!HasPowder)
        {
            UpdateNoPowderState(key);
            return;
        }

        if (HasMixedPowder)
        {
            UpdateMixedPowderState(key);
            return;
        }

        UpdateUnmixedPowderState(key);
    }

    private void UpdateNoPowderState(int key)
    {
        switch (key)
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

    private void UpdateUnmixedPowderState(int key)
    {
        switch (key)
        {
            case 0:
                CurrentState = TwoFourDNPHState.PowderOnly;
                return;

            case 1:
                CurrentState = TwoFourDNPHState.PowderAndAcid;
                return;

            case 2:
                CurrentState = TwoFourDNPHState.PowderAndEthanol;
                return;

            case 4:
                CurrentState = TwoFourDNPHState.PowderAndAldehyde;
                return;

            case 3:
                CurrentState = TwoFourDNPHState.PowderAndAcidAndEthanol;
                return;

            case 5:
                CurrentState = TwoFourDNPHState.PowderAndAcidAndAldehyde;
                return;

            case 6:
                CurrentState = TwoFourDNPHState.PowderAndEthanolAndAldehyde;
                return;

            case 7:
                CurrentState = TwoFourDNPHState.PowderAndAcidAndEthanolAndAldehyde;
                return;

            default:
                CurrentState = TwoFourDNPHState.PowderOnly;
                return;
        }
    }

    private void UpdateMixedPowderState(int key)
    {
        switch (key)
        {
            case 0:
                CurrentState = TwoFourDNPHState.PowderOnly;
                return;

            case 1:
                CurrentState = TwoFourDNPHState.PowderAndAcidMixed;
                return;

            case 2:
                CurrentState = TwoFourDNPHState.PowderAndEthanolMixed;
                return;

            case 4:
                CurrentState = TwoFourDNPHState.PowderAndAldehydeMixed;
                return;

            case 3:
                CurrentState = TwoFourDNPHState.PowderAndAcidAndEthanolMixed;
                return;

            case 5:
                CurrentState = TwoFourDNPHState.PowderAndAcidAndAldehydeMixed;
                return;

            case 6:
                CurrentState = TwoFourDNPHState.PowderAndEthanolAndAldehydeMixed;
                return;

            case 7:
                CurrentState = TwoFourDNPHState.PowderAndAcidAndEthanolAndAldehydeMixed;
                return;

            default:
                CurrentState = TwoFourDNPHState.PowderOnly;
                return;
        }
    }

    private void UpdateVisualState()
    {
        int key = TwoFourDNPHCombinationKey();

        if (!HasPowder && key == 0)
        {
            CurrentVisualState = TwoFourDNPHVisualState.Empty;
            return;
        }

        if (!HasMixedPowder)
        {
            CurrentVisualState = key == 0
                ? TwoFourDNPHVisualState.PowderOnly
                : HasAldehyde
                    ? TwoFourDNPHVisualState.PaleYellow
                    : TwoFourDNPHVisualState.Clear;
            return;
        }

        if (HasTwoFourDNPHSequence())
        {
            CurrentVisualState = TwoFourDNPHVisualState.Amber;
            return;
        }

        if (HasTwoFourDNPHTestSequence())
        {
            CurrentVisualState = TwoFourDNPHVisualState.PaleAmber;
            return;
        }

        if (HasAldehyde)
        {
            CurrentVisualState = TwoFourDNPHVisualState.LightOrange;
            return;
        }

        CurrentVisualState = TwoFourDNPHVisualState.PaleAmber;
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
        }
    }

    private void UpdateSolidVisuals()
    {
        if (powder == null)
            return;

        powder.SetActive(HasUnmixedPowder);
    }

    protected override void UpdateBeakerVisual()
    {
        base.UpdateBeakerVisual();

        bool showLiquid =
            CurrentVisualState == TwoFourDNPHVisualState.Clear ||
            CurrentVisualState == TwoFourDNPHVisualState.PaleYellow ||
            CurrentVisualState == TwoFourDNPHVisualState.PaleAmber ||
            CurrentVisualState == TwoFourDNPHVisualState.LightOrange ||
            CurrentVisualState == TwoFourDNPHVisualState.Amber;

        if (liquidRenderer != null)
            liquidRenderer.enabled = showLiquid && liquidAmount > 0.001f;
    }

    private void RefreshVisuals()
    {
        UpdateState();
        UpdateVisualState();
        UpdateMaterial();
        UpdateSolidVisuals();
        UpdateBeakerVisual();
    }

    private void ResetState()
    {
        CurrentState = TwoFourDNPHState.Empty;
        CurrentVisualState = TwoFourDNPHVisualState.Empty;

        PowderCount = 0;
        MixedPowderCount = 0;

        AcidCount = 0;
        EthanolCount = 0;
        AldehydeCount = 0;
        MixCount = 0;

        reactionSequence.Clear();
    }

    private int GetLiquidCount()
    {
        int count = 0;

        if (HasAcid) count++;
        if (HasEthanol) count++;
        if (HasAldehyde) count++;

        return count;
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
                TwoFourDNPHContainerType.Beaker,
                GetActionFromReagent(_reagent),
                validationOutline);
    }

    private void AddPowderToProcedureTracker()
    {
        if (procedureTracker != null)
            procedureTracker.RegisterAction(
                TwoFourDNPHContainerType.Beaker,
                TwoFourDNPHActionType.AddTwoFourDNPH,
                validationOutline);
    }

    private void AddMixingToProcedureTracker()
    {
        if (procedureTracker != null)
            procedureTracker.RegisterAction(
                TwoFourDNPHContainerType.Beaker,
                TwoFourDNPHActionType.ApplyMixing,
                validationOutline);
    }
}