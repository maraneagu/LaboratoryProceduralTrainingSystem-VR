using System.Collections.Generic;
using UnityEngine;

public class BenedictBeakerLiquid : BeakerLiquid
{
    public enum BenedictState
    {
        Empty,

        DistilledWaterOnly,
        UrineOnly,
        DistilledWaterAndUrine,

        CopperSulfateOnly,
        SodiumCitrateOnly,
        SodiumCarbonateOnly,
        CopperSulfateAndSodiumCitrateOnly,
        CopperSulfateAndSodiumCarbonateOnly,
        SodiumCitrateAndSodiumCarbonateOnly,
        PowdersOnly,

        CopperSulfateAndDistilledWater,
        SodiumCitrateAndDistilledWater,
        SodiumCarbonateAndDistilledWater,
        CopperSulfateAndSodiumCitrateAndDistilledWater,
        CopperSulfateAndSodiumCarbonateAndDistilledWater,
        SodiumCitrateAndSodiumCarbonateAndDistilledWater,
        PowdersAndDistilledWater,

        CopperSulfateAndUrine,
        SodiumCitrateAndUrine,
        SodiumCarbonateAndUrine,
        CopperSulfateAndSodiumCitrateAndUrine,
        CopperSulfateAndSodiumCarbonateAndUrine,
        SodiumCitrateAndSodiumCarbonateAndUrine,
        PowdersAndUrine,

        CopperSulfateAndDistilledWaterAndUrine,
        SodiumCitrateAndDistilledWaterAndUrine,
        SodiumCarbonateAndDistilledWaterAndUrine,
        CopperSulfateAndSodiumCitrateAndDistilledWaterAndUrine,
        CopperSulfateAndSodiumCarbonateAndDistilledWaterAndUrine,
        SodiumCitrateAndSodiumCarbonateAndDistilledWaterAndUrine,
        PowdersAndDistilledWaterAndUrine,

        CopperSulfateAndDistilledWaterMixed,
        SodiumCitrateAndDistilledWaterMixed,
        SodiumCarbonateAndDistilledWaterMixed,
        CopperSulfateAndSodiumCitrateAndDistilledWaterMixed,
        CopperSulfateAndSodiumCarbonateAndDistilledWaterMixed,
        SodiumCitrateAndSodiumCarbonateAndDistilledWaterMixed,
        PowdersAndDistilledWaterMixed,

        CopperSulfateAndUrineMixed,
        SodiumCitrateAndUrineMixed,
        SodiumCarbonateAndUrineMixed,
        CopperSulfateAndSodiumCitrateAndUrineMixed,
        CopperSulfateAndSodiumCarbonateAndUrineMixed,
        SodiumCitrateAndSodiumCarbonateAndUrineMixed,
        PowdersAndUrineMixed,

        CopperSulfateAndDistilledWaterAndUrineMixed,
        SodiumCitrateAndDistilledWaterAndUrineMixed,
        SodiumCarbonateAndDistilledWaterAndUrineMixed,
        CopperSulfateAndSodiumCitrateAndDistilledWaterAndUrineMixed,
        CopperSulfateAndSodiumCarbonateAndDistilledWaterAndUrineMixed,
        SodiumCitrateAndSodiumCarbonateAndDistilledWaterAndUrineMixed,
        PowdersAndDistilledWaterAndUrineMixed,

        BenedictSolution
    }

    public enum BenedictVisualState
    {
        Empty,
        PowderOnly,
        Clear,
        Yellow,
        PaleYellow,
        White,
        PaleBlue,
        Blue,
        Turquoise,
        BlueSolution
    }

    private enum BenedictAction
    {
        DistilledWater,
        Urine,
        CopperSulfate,
        SodiumCitrate,
        SodiumCarbonate,
        Mix
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

    [Header("Powders")]
    [SerializeField] private GameObject copperSulfatePowder;
    [SerializeField] private GameObject sodiumCitratePowder;
    [SerializeField] private GameObject sodiumCarbonatePowder;

    [Header("Procedure Tracker")]
    [SerializeField] private BenedictProcedureTracker procedureTracker;

    [Header("Validation Outline")]
    [SerializeField] private ValidationOutline validationOutline;

    public BenedictState CurrentState { get; private set; } = BenedictState.Empty;
    public BenedictVisualState CurrentVisualState { get; private set; } = BenedictVisualState.Empty;

    public int DistilledWaterCount { get; private set; }
    public int UrineCount { get; private set; }

    public int CopperSulfateCount { get; private set; }
    public int SodiumCitrateCount { get; private set; }
    public int SodiumCarbonateCount { get; private set; }

    public int MixedCopperSulfateCount { get; private set; }
    public int MixedSodiumCitrateCount { get; private set; }
    public int MixedSodiumCarbonateCount { get; private set; }

    public int MixCount { get; private set; }

    public bool HasDistilledWater => DistilledWaterCount > 0;
    public bool HasUrine => UrineCount > 0;

    public bool HasCopperSulfate => CopperSulfateCount > 0;
    public bool HasSodiumCitrate => SodiumCitrateCount > 0;
    public bool HasSodiumCarbonate => SodiumCarbonateCount > 0;

    public bool HasMixedCopperSulfate => MixedCopperSulfateCount > 0;
    public bool HasMixedSodiumCitrate => MixedSodiumCitrateCount > 0;
    public bool HasMixedSodiumCarbonate => MixedSodiumCarbonateCount > 0;

    public int UnmixedCopperSulfateCount => Mathf.Max(0, CopperSulfateCount - MixedCopperSulfateCount);
    public int UnmixedSodiumCitrateCount => Mathf.Max(0, SodiumCitrateCount - MixedSodiumCitrateCount);
    public int UnmixedSodiumCarbonateCount => Mathf.Max(0, SodiumCarbonateCount - MixedSodiumCarbonateCount);

    public bool HasUnmixedCopperSulfate => UnmixedCopperSulfateCount > 0;
    public bool HasUnmixedSodiumCitrate => UnmixedSodiumCitrateCount > 0;
    public bool HasUnmixedSodiumCarbonate => UnmixedSodiumCarbonateCount > 0;

    public bool HasPowder => HasCopperSulfate || HasSodiumCitrate || HasSodiumCarbonate;
    public bool HasMixedPowder => HasMixedCopperSulfate || HasMixedSodiumCitrate || HasMixedSodiumCarbonate;
    public bool HasUnmixedPowder => HasUnmixedCopperSulfate || HasUnmixedSodiumCitrate || HasUnmixedSodiumCarbonate;

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

        Debug.Log($"[BenedictBeakerLiquid] Reagent Added: {_reagent}, Reaction State: {CurrentState}, Visual State: {CurrentVisualState}");
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
                if (HasUnmixedCopperSulfate)
                    return false;

                AddCopperSulfateToSequence();
                break;

            case PowderType.SodiumCitrate:
                if (HasUnmixedSodiumCitrate)
                    return false;

                AddSodiumCitrateToSequence();
                break;

            case PowderType.SodiumCarbonate:
                if (HasUnmixedSodiumCarbonate)
                    return false;

                AddSodiumCarbonateToSequence();
                break;

            default:
                return false;
        }

        RefreshVisuals();
        AddPowderToProcedureTracker(_powder);

        Debug.Log($"[BenedictBeakerLiquid] Powder Added: {_powder.PowderType}, Reaction State: {CurrentState}, Visual State: {CurrentVisualState}");
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

        Debug.Log($"[BenedictBeakerLiquid] Mixing Applied, Reaction State: {CurrentState}, Visual State: {CurrentVisualState}");
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

    private void AddCopperSulfateToSequence()
    {
        CopperSulfateCount++;
        reactionSequence.Add(BenedictAction.CopperSulfate);
    }

    private void AddSodiumCitrateToSequence()
    {
        SodiumCitrateCount++;
        reactionSequence.Add(BenedictAction.SodiumCitrate);
    }

    private void AddSodiumCarbonateToSequence()
    {
        SodiumCarbonateCount++;
        reactionSequence.Add(BenedictAction.SodiumCarbonate);
    }

    private void AddMixingToSequence()
    {
        MixCount++;

        MixedCopperSulfateCount = CopperSulfateCount;
        MixedSodiumCitrateCount = SodiumCitrateCount;
        MixedSodiumCarbonateCount = SodiumCarbonateCount;

        reactionSequence.Add(BenedictAction.Mix);
    }

    private void UpdateFillStep()
    {
        CurrentFillStep++;
        FillBeaker(CurrentFillStep);
    }

    private int PowderCombinationKey()
    {
        int key = 0;

        if (HasCopperSulfate)
            key |= 1;

        if (HasSodiumCitrate)
            key |= 2;

        if (HasSodiumCarbonate)
            key |= 4;

        return key;
    }

    private int MixedPowderCombinationKey()
    {
        int key = 0;

        if (HasMixedCopperSulfate)
            key |= 1;

        if (HasMixedSodiumCitrate)
            key |= 2;

        if (HasMixedSodiumCarbonate)
            key |= 4;

        return key;
    }

    private int UnmixedPowderCombinationKey()
    {
        int key = 0;

        if (HasUnmixedCopperSulfate)
            key |= 1;

        if (HasUnmixedSodiumCitrate)
            key |= 2;

        if (HasUnmixedSodiumCarbonate)
            key |= 4;

        return key;
    }

    private int ReagentCombinationKey()
    {
        int key = 0;

        if (HasDistilledWater)
            key |= 1;

        if (HasUrine)
            key |= 2;

        return key;
    }

    private bool HasBenedictSequence()
    {
        return reactionSequence.Count == 5 &&
               reactionSequence[0] == BenedictAction.CopperSulfate &&
               reactionSequence[1] == BenedictAction.SodiumCitrate &&
               reactionSequence[2] == BenedictAction.SodiumCarbonate &&
               reactionSequence[3] == BenedictAction.DistilledWater &&
               reactionSequence[4] == BenedictAction.Mix;
    }

    private void UpdateState()
    {
        int liquidKey = ReagentCombinationKey();

        if (HasBenedictSequence())
        {
            CurrentState = BenedictState.BenedictSolution;
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
                CurrentState = BenedictState.Empty;
                return;

            case 1:
                CurrentState = BenedictState.DistilledWaterOnly;
                return;

            case 2:
                CurrentState = BenedictState.UrineOnly;
                return;

            case 3:
                CurrentState = BenedictState.DistilledWaterAndUrine;
                return;

            default:
                CurrentState = BenedictState.Empty;
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
                    CurrentState = BenedictState.CopperSulfateOnly;
                    return;

                case 2:
                    CurrentState = BenedictState.SodiumCitrateOnly;
                    return;

                case 4:
                    CurrentState = BenedictState.SodiumCarbonateOnly;
                    return;

                case 3:
                    CurrentState = BenedictState.CopperSulfateAndSodiumCitrateOnly;
                    return;

                case 5:
                    CurrentState = BenedictState.CopperSulfateAndSodiumCarbonateOnly;
                    return;

                case 6:
                    CurrentState = BenedictState.SodiumCitrateAndSodiumCarbonateOnly;
                    return;

                case 7:
                    CurrentState = BenedictState.PowdersOnly;
                    return;
            }
        }

        if (liquidKey == 1)
        {
            switch (powderKey)
            {
                case 1:
                    CurrentState = BenedictState.CopperSulfateAndDistilledWater;
                    return;

                case 2:
                    CurrentState = BenedictState.SodiumCitrateAndDistilledWater;
                    return;

                case 4:
                    CurrentState = BenedictState.SodiumCarbonateAndDistilledWater;
                    return;

                case 3:
                    CurrentState = BenedictState.CopperSulfateAndSodiumCitrateAndDistilledWater;
                    return;

                case 5:
                    CurrentState = BenedictState.CopperSulfateAndSodiumCarbonateAndDistilledWater;
                    return;

                case 6:
                    CurrentState = BenedictState.SodiumCitrateAndSodiumCarbonateAndDistilledWater;
                    return;

                case 7:
                    CurrentState = BenedictState.PowdersAndDistilledWater;
                    return;
            }
        }

        if (liquidKey == 2)
        {
            switch (powderKey)
            {
                case 1:
                    CurrentState = BenedictState.CopperSulfateAndUrine;
                    return;

                case 2:
                    CurrentState = BenedictState.SodiumCitrateAndUrine;
                    return;

                case 4:
                    CurrentState = BenedictState.SodiumCarbonateAndUrine;
                    return;

                case 3:
                    CurrentState = BenedictState.CopperSulfateAndSodiumCitrateAndUrine;
                    return;

                case 5:
                    CurrentState = BenedictState.CopperSulfateAndSodiumCarbonateAndUrine;
                    return;

                case 6:
                    CurrentState = BenedictState.SodiumCitrateAndSodiumCarbonateAndUrine;
                    return;

                case 7:
                    CurrentState = BenedictState.PowdersAndUrine;
                    return;
            }
        }

        if (liquidKey == 3)
        {
            switch (powderKey)
            {
                case 1:
                    CurrentState = BenedictState.CopperSulfateAndDistilledWaterAndUrine;
                    return;

                case 2:
                    CurrentState = BenedictState.SodiumCitrateAndDistilledWaterAndUrine;
                    return;

                case 4:
                    CurrentState = BenedictState.SodiumCarbonateAndDistilledWaterAndUrine;
                    return;

                case 3:
                    CurrentState = BenedictState.CopperSulfateAndSodiumCitrateAndDistilledWaterAndUrine;
                    return;

                case 5:
                    CurrentState = BenedictState.CopperSulfateAndSodiumCarbonateAndDistilledWaterAndUrine;
                    return;

                case 6:
                    CurrentState = BenedictState.SodiumCitrateAndSodiumCarbonateAndDistilledWaterAndUrine;
                    return;

                case 7:
                    CurrentState = BenedictState.PowdersAndDistilledWaterAndUrine;
                    return;
            }
        }

        CurrentState = BenedictState.Empty;
    }

    private void UpdateMixedPowderState(int powderKey, int liquidKey)
    {
        if (liquidKey == 1)
        {
            switch (powderKey)
            {
                case 1:
                    CurrentState = BenedictState.CopperSulfateAndDistilledWaterMixed;
                    return;

                case 2:
                    CurrentState = BenedictState.SodiumCitrateAndDistilledWaterMixed;
                    return;

                case 4:
                    CurrentState = BenedictState.SodiumCarbonateAndDistilledWaterMixed;
                    return;

                case 3:
                    CurrentState = BenedictState.CopperSulfateAndSodiumCitrateAndDistilledWaterMixed;
                    return;

                case 5:
                    CurrentState = BenedictState.CopperSulfateAndSodiumCarbonateAndDistilledWaterMixed;
                    return;

                case 6:
                    CurrentState = BenedictState.SodiumCitrateAndSodiumCarbonateAndDistilledWaterMixed;
                    return;

                case 7:
                    CurrentState = BenedictState.PowdersAndDistilledWaterMixed;
                    return;
            }
        }

        if (liquidKey == 2)
        {
            switch (powderKey)
            {
                case 1:
                    CurrentState = BenedictState.CopperSulfateAndUrineMixed;
                    return;

                case 2:
                    CurrentState = BenedictState.SodiumCitrateAndUrineMixed;
                    return;

                case 4:
                    CurrentState = BenedictState.SodiumCarbonateAndUrineMixed;
                    return;

                case 3:
                    CurrentState = BenedictState.CopperSulfateAndSodiumCitrateAndUrineMixed;
                    return;

                case 5:
                    CurrentState = BenedictState.CopperSulfateAndSodiumCarbonateAndUrineMixed;
                    return;

                case 6:
                    CurrentState = BenedictState.SodiumCitrateAndSodiumCarbonateAndUrineMixed;
                    return;

                case 7:
                    CurrentState = BenedictState.PowdersAndUrineMixed;
                    return;
            }
        }

        if (liquidKey == 3)
        {
            switch (powderKey)
            {
                case 1:
                    CurrentState = BenedictState.CopperSulfateAndDistilledWaterAndUrineMixed;
                    return;

                case 2:
                    CurrentState = BenedictState.SodiumCitrateAndDistilledWaterAndUrineMixed;
                    return;

                case 4:
                    CurrentState = BenedictState.SodiumCarbonateAndDistilledWaterAndUrineMixed;
                    return;

                case 3:
                    CurrentState = BenedictState.CopperSulfateAndSodiumCitrateAndDistilledWaterAndUrineMixed;
                    return;

                case 5:
                    CurrentState = BenedictState.CopperSulfateAndSodiumCarbonateAndDistilledWaterAndUrineMixed;
                    return;

                case 6:
                    CurrentState = BenedictState.SodiumCitrateAndSodiumCarbonateAndDistilledWaterAndUrineMixed;
                    return;

                case 7:
                    CurrentState = BenedictState.PowdersAndDistilledWaterAndUrineMixed;
                    return;
            }
        }

        UpdateNoPowderState(liquidKey);
    }

    private void UpdateVisualState()
    {
        int liquidKey = ReagentCombinationKey();

        if (!HasPowder && liquidKey == 0)
        {
            CurrentVisualState = BenedictVisualState.Empty;
            return;
        }

        if (liquidKey == 0)
        {
            CurrentVisualState = BenedictVisualState.PowderOnly;
            return;
        }

        if (!HasMixedPowder)
        {
            CurrentVisualState = HasUrine
                ? BenedictVisualState.Yellow
                : BenedictVisualState.Clear;
            return;
        }

        if (HasBenedictSequence())
        {
            CurrentVisualState = BenedictVisualState.BlueSolution;
            return;
        }

        if (HasMixedCopperSulfate && (HasMixedSodiumCitrate || HasMixedSodiumCarbonate))
        {
            CurrentVisualState = HasUrine
                ? BenedictVisualState.Turquoise
                : BenedictVisualState.PaleBlue;
            return;
        }

        if (HasMixedCopperSulfate)
        {
            CurrentVisualState = HasUrine
                ? BenedictVisualState.Turquoise
                : BenedictVisualState.Blue;
            return;
        }

        if (HasMixedSodiumCitrate || HasMixedSodiumCarbonate)
        {
            CurrentVisualState = HasUrine
                ? BenedictVisualState.PaleYellow
                : BenedictVisualState.White;
            return;
        }

        CurrentVisualState = HasUrine
            ? BenedictVisualState.Yellow
            : BenedictVisualState.Clear;
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
        }
    }

    private void UpdatePowderVisuals()
    {
        int unmixedPowderKey = UnmixedPowderCombinationKey();

        bool showCopperSulfate =
            unmixedPowderKey == 1 || unmixedPowderKey == 3 || unmixedPowderKey == 5 || unmixedPowderKey == 7;

        bool showSodiumCitrate =
            unmixedPowderKey == 2 || unmixedPowderKey == 3 || unmixedPowderKey == 6 || unmixedPowderKey == 7;

        bool showSodiumCarbonate =
            unmixedPowderKey == 4 || unmixedPowderKey == 5 || unmixedPowderKey == 6 || unmixedPowderKey == 7;

        if (copperSulfatePowder != null)
            copperSulfatePowder.SetActive(showCopperSulfate);

        if (sodiumCitratePowder != null)
            sodiumCitratePowder.SetActive(showSodiumCitrate);

        if (sodiumCarbonatePowder != null)
            sodiumCarbonatePowder.SetActive(showSodiumCarbonate);

        int visiblePowderCount = 0;
        if (showCopperSulfate) visiblePowderCount++;
        if (showSodiumCitrate) visiblePowderCount++;
        if (showSodiumCarbonate) visiblePowderCount++;

        List<GameObject> visiblePowders = new List<GameObject>();

        if (showCopperSulfate && copperSulfatePowder != null)
            visiblePowders.Add(copperSulfatePowder);

        if (showSodiumCitrate && sodiumCitratePowder != null)
            visiblePowders.Add(sodiumCitratePowder);

        if (showSodiumCarbonate && sodiumCarbonatePowder != null)
            visiblePowders.Add(sodiumCarbonatePowder);

        if (visiblePowderCount == 1)
        {
            Vector3 position = visiblePowders[0].transform.localPosition;
            position.x = 0f;
            position.y = 0.01f;
            position.z = 0f;
            visiblePowders[0].transform.localPosition = position;
        }
        else if (visiblePowderCount == 2)
        {
            Vector3 leftPosition = visiblePowders[0].transform.localPosition;
            leftPosition.x = -0.01f;
            leftPosition.y = 0.01f;
            leftPosition.z = 0f;
            visiblePowders[0].transform.localPosition = leftPosition;

            Vector3 rightPosition = visiblePowders[1].transform.localPosition;
            rightPosition.x = 0.01f;
            rightPosition.y = 0.01f;
            rightPosition.z = 0f;
            visiblePowders[1].transform.localPosition = rightPosition;
        }
        else if (visiblePowderCount == 3)
        {
            if (copperSulfatePowder != null && showCopperSulfate)
            {
                Vector3 position = copperSulfatePowder.transform.localPosition;
                position.x = -0.01f;
                position.y = 0.01f;
                position.z = 0.005f;
                copperSulfatePowder.transform.localPosition = position;
            }

            if (sodiumCitratePowder != null && showSodiumCitrate)
            {
                Vector3 position = sodiumCitratePowder.transform.localPosition;
                position.x = 0.01f;
                position.y = 0.01f;
                position.z = 0.005f;
                sodiumCitratePowder.transform.localPosition = position;
            }

            if (sodiumCarbonatePowder != null && showSodiumCarbonate)
            {
                Vector3 position = sodiumCarbonatePowder.transform.localPosition;
                position.x = 0.004f;
                position.y = 0.01f;
                position.z = -0.01f;
                sodiumCarbonatePowder.transform.localPosition = position;
            }
        }
    }

    protected override void UpdateBeakerVisual()
    {
        base.UpdateBeakerVisual();

        bool showLiquid =
            CurrentVisualState == BenedictVisualState.Clear ||
            CurrentVisualState == BenedictVisualState.Yellow ||
            CurrentVisualState == BenedictVisualState.PaleYellow ||
            CurrentVisualState == BenedictVisualState.White ||
            CurrentVisualState == BenedictVisualState.PaleBlue ||
            CurrentVisualState == BenedictVisualState.Blue ||
            CurrentVisualState == BenedictVisualState.Turquoise ||
            CurrentVisualState == BenedictVisualState.BlueSolution;

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
        CurrentState = BenedictState.Empty;
        CurrentVisualState = BenedictVisualState.Empty;

        DistilledWaterCount = 0;
        UrineCount = 0;

        CopperSulfateCount = 0;
        SodiumCitrateCount = 0;
        SodiumCarbonateCount = 0;

        MixedCopperSulfateCount = 0;
        MixedSodiumCitrateCount = 0;
        MixedSodiumCarbonateCount = 0;

        MixCount = 0;

        reactionSequence.Clear();
    }

    private int GetLiquidCount()
    {
        int count = 0;

        if (HasDistilledWater) count++;
        if (HasUrine) count++;

        return count;
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

    private BenedictActionType GetActionFromPowder(PowderType _powder)
    {
        switch (_powder)
        {
            case PowderType.CopperSulfate:
                return BenedictActionType.AddCopperSulfate;

            case PowderType.SodiumCitrate:
                return BenedictActionType.AddSodiumCitrate;

            case PowderType.SodiumCarbonate:
                return BenedictActionType.AddSodiumCarbonate;

            default:
                return BenedictActionType.None;
        }
    }

    private void AddReagentToProcedureTracker(ReagentType _reagent)
    {
        if (procedureTracker != null)
            procedureTracker.RegisterAction(
                BenedictContainerType.Beaker,
                GetActionFromReagent(_reagent),
                validationOutline);
    }

    private void AddPowderToProcedureTracker(Powder _powder)
    {
        if (procedureTracker != null)
            procedureTracker.RegisterAction(
                BenedictContainerType.Beaker,
                GetActionFromPowder(_powder.PowderType),
                validationOutline);
    }

    private void AddMixingToProcedureTracker()
    {
        if (procedureTracker != null)
            procedureTracker.RegisterAction(
                BenedictContainerType.Beaker,
                BenedictActionType.ApplyMixing,
                validationOutline);
    }
}