using System.Collections;
using UnityEngine;

public class FehlingProcedureTracker : ProcedureTracker
{
    private const int ProcedureActionCount = 10;

    public enum FehlingAAction
    {
        AddCopperSulfate,
        AddSulfuricAcid,
        ApplyMixing,
        Completed
    }

    public enum FehlingBAction
    {
        AddPotassiumTartrate,
        AddNaOH,
        ApplyMixing,
        Completed
    }

    public enum FehlingReactionTubeAction
    {
        AddFehlingA,
        AddFehlingB,
        AddGlucose,
        ApplyHeating,
        Completed
    }

    public FehlingAAction CurrentFehlingAAction { get; private set; }
    public FehlingBAction CurrentFehlingBAction { get; private set; }
    public FehlingReactionTubeAction CurrentReactionTubeAction { get; private set; }

    public bool IsFehlingACompleted { get; private set; }
    public bool IsFehlingBCompleted { get; private set; }
    public bool IsReactionTubeCompleted { get; private set; }

    public int ActionCount { get; private set; }
    public int CorrectActionCount { get; private set; }
    public int IncorrectActionCount { get; private set; }

    private readonly ActionResult[] actionResults = new ActionResult[10];

    private bool isRegisteringAction;
    private Coroutine actionRoutine;

    protected override void Awake()
    {
        base.Awake();
        ResetProcedureTracker();
    }

    public void RegisterAction(
        FehlingContainerType container,
        FehlingActionType performedAction,
        ValidationOutline validationOutline = null)
    {
        if (IsProcedureCompleted || isRegisteringAction)
            return;

        actionRoutine = StartCoroutine(RegisterActionRoutine(container, performedAction, validationOutline));
    }

    private IEnumerator RegisterActionRoutine(
        FehlingContainerType container,
        FehlingActionType performedAction,
        ValidationOutline validationOutline = null)
    {
        isRegisteringAction = true;

        FehlingActionType expectedAction = GetExpectedAction(container);
        bool isCorrectAction = EvaluateAndRecordAction(container, expectedAction, performedAction);

        yield return ValidateAction(isCorrectAction, validationOutline);

        NextAction(container, isCorrectAction);
        EvaluateProcedureCompletion();

        actionRoutine = null;
        isRegisteringAction = false;
    }

    private bool EvaluateAndRecordAction(
        FehlingContainerType container,
        FehlingActionType expectedAction,
        FehlingActionType performedAction)
    {
        bool isCorrectAction = IsCorrectAction(container, performedAction);

        RecordAction(expectedAction.ToString(), performedAction.ToString(), isCorrectAction);

        SaveActionResult(expectedAction, isCorrectAction);
        LogActionResult(container, expectedAction, performedAction, isCorrectAction);

        UpdateActionCounts(isCorrectAction);

        return isCorrectAction;
    }

    private bool IsCorrectAction(FehlingContainerType container, FehlingActionType performedAction)
    {
        if (!IsFehlingACompleted || !IsFehlingBCompleted)
        {
            switch (container)
            {
                case FehlingContainerType.FehlingABeaker:
                    return !IsFehlingACompleted &&
                           performedAction == GetExpectedFehlingAAction();

                case FehlingContainerType.FehlingBBeaker:
                    return !IsFehlingBCompleted &&
                           performedAction == GetExpectedFehlingBAction();

                case FehlingContainerType.ReactionTube:
                default:
                    return false;
            }
        }

        if (!IsReactionTubeCompleted)
        {
            return container == FehlingContainerType.ReactionTube &&
                   performedAction == GetExpectedReactionTubeAction();
        }

        return false;
    }

    private IEnumerator ValidateAction(bool isCorrectAction, ValidationOutline validationOutline)
    {
        ShowValidation(isCorrectAction, validationOutline);

        float validationDuration = GetValidationDuration(validationOutline);

        if (validationDuration <= 0f)
            yield break;

        IsWaitingForNextAction = true;
        yield return new WaitForSeconds(validationDuration);
        IsWaitingForNextAction = false;
    }

    private void EvaluateProcedureCompletion()
    {
        if (ActionCount < ProcedureActionCount)
            return;

        IsProcedureCompleted = true;
        CompleteProcedure(CorrectActionCount, IncorrectActionCount);
    }

    private FehlingActionType GetExpectedAction(FehlingContainerType container)
    {
        if (!IsFehlingACompleted || !IsFehlingBCompleted)
        {
            switch (container)
            {
                case FehlingContainerType.FehlingABeaker:
                    return IsFehlingACompleted
                        ? FehlingActionType.None
                        : GetExpectedFehlingAAction();

                case FehlingContainerType.FehlingBBeaker:
                    return IsFehlingBCompleted
                        ? FehlingActionType.None
                        : GetExpectedFehlingBAction();

                case FehlingContainerType.ReactionTube:
                default:
                    return GetExpectedBeakerAction();
            }
        }

        if (!IsReactionTubeCompleted)
            return GetExpectedReactionTubeAction();

        return FehlingActionType.None;
    }

    private FehlingActionType GetExpectedBeakerAction()
    {
        if (!IsFehlingACompleted)
            return GetExpectedFehlingAAction();

        if (!IsFehlingBCompleted)
            return GetExpectedFehlingBAction();

        return FehlingActionType.None;
    }

    private FehlingActionType GetExpectedFehlingAAction()
    {
        switch (CurrentFehlingAAction)
        {
            case FehlingAAction.AddCopperSulfate:
                return FehlingActionType.AddCopperSulfate;

            case FehlingAAction.AddSulfuricAcid:
                return FehlingActionType.AddSulfuricAcid;

            case FehlingAAction.ApplyMixing:
                return FehlingActionType.ApplyMixingToFehlingA;

            case FehlingAAction.Completed:
            default:
                return FehlingActionType.None;
        }
    }

    private FehlingActionType GetExpectedFehlingBAction()
    {
        switch (CurrentFehlingBAction)
        {
            case FehlingBAction.AddPotassiumTartrate:
                return FehlingActionType.AddPotassiumTartrate;

            case FehlingBAction.AddNaOH:
                return FehlingActionType.AddNaOH;

            case FehlingBAction.ApplyMixing:
                return FehlingActionType.ApplyMixingToFehlingB;

            case FehlingBAction.Completed:
            default:
                return FehlingActionType.None;
        }
    }

    private FehlingActionType GetExpectedReactionTubeAction()
    {
        switch (CurrentReactionTubeAction)
        {
            case FehlingReactionTubeAction.AddFehlingA:
                return FehlingActionType.AddFehlingA;

            case FehlingReactionTubeAction.AddFehlingB:
                return FehlingActionType.AddFehlingB;

            case FehlingReactionTubeAction.AddGlucose:
                return FehlingActionType.AddGlucose;

            case FehlingReactionTubeAction.ApplyHeating:
                return FehlingActionType.ApplyHeating;

            case FehlingReactionTubeAction.Completed:
            default:
                return FehlingActionType.None;
        }
    }

    private void NextAction(FehlingContainerType container, bool isCorrectAction)
    {
        if (!IsFehlingACompleted || !IsFehlingBCompleted)
        {
            NextBeakerAction(container, isCorrectAction);
            return;
        }

        if (!IsReactionTubeCompleted)
        {
            NextReactionTubeAction();
            return;
        }

        NotifyProcedureActionChanged();
    }

    private void NextBeakerAction(FehlingContainerType container, bool isCorrectAction)
    {
        if (isCorrectAction)
        {
            switch (container)
            {
                case FehlingContainerType.FehlingABeaker:
                    NextFehlingAAction();
                    return;

                case FehlingContainerType.FehlingBBeaker:
                    NextFehlingBAction();
                    return;
            }
        }

        if (!IsFehlingACompleted)
        {
            NextFehlingAAction();
            return;
        }

        if (!IsFehlingBCompleted)
        {
            NextFehlingBAction();
            return;
        }

        NotifyProcedureActionChanged();
    }

    private void NextFehlingAAction()
    {
        switch (CurrentFehlingAAction)
        {
            case FehlingAAction.AddCopperSulfate:
                CurrentFehlingAAction = FehlingAAction.AddSulfuricAcid;
                break;

            case FehlingAAction.AddSulfuricAcid:
                CurrentFehlingAAction = FehlingAAction.ApplyMixing;
                break;

            case FehlingAAction.ApplyMixing:
                CurrentFehlingAAction = FehlingAAction.Completed;
                IsFehlingACompleted = true;
                break;
        }

        NotifyProcedureActionChanged();
    }

    private void NextFehlingBAction()
    {
        switch (CurrentFehlingBAction)
        {
            case FehlingBAction.AddPotassiumTartrate:
                CurrentFehlingBAction = FehlingBAction.AddNaOH;
                break;

            case FehlingBAction.AddNaOH:
                CurrentFehlingBAction = FehlingBAction.ApplyMixing;
                break;

            case FehlingBAction.ApplyMixing:
                CurrentFehlingBAction = FehlingBAction.Completed;
                IsFehlingBCompleted = true;
                break;
        }

        NotifyProcedureActionChanged();
    }

    private void NextReactionTubeAction()
    {
        switch (CurrentReactionTubeAction)
        {
            case FehlingReactionTubeAction.AddFehlingA:
                CurrentReactionTubeAction = FehlingReactionTubeAction.AddFehlingB;
                break;

            case FehlingReactionTubeAction.AddFehlingB:
                CurrentReactionTubeAction = FehlingReactionTubeAction.AddGlucose;
                break;

            case FehlingReactionTubeAction.AddGlucose:
                CurrentReactionTubeAction = FehlingReactionTubeAction.ApplyHeating;
                break;

            case FehlingReactionTubeAction.ApplyHeating:
                CurrentReactionTubeAction = FehlingReactionTubeAction.Completed;
                IsReactionTubeCompleted = true;
                break;
        }

        NotifyProcedureActionChanged();
    }

    public override GuidanceTargetType[] GetCurrentGuidanceTargets()
    {
        if (!guidanceEnabled || IsProcedureCompleted)
            return System.Array.Empty<GuidanceTargetType>();

        if (!IsFehlingACompleted)
        {
            switch (CurrentFehlingAAction)
            {
                case FehlingAAction.AddCopperSulfate:
                    return new GuidanceTargetType[]
                    {
                        GuidanceTargetType.CopperSulfatePowder,
                        GuidanceTargetType.Spatula
                    };

                case FehlingAAction.AddSulfuricAcid:
                    return new GuidanceTargetType[]
                    {
                        GuidanceTargetType.SulfuricAcidBottle,
                        GuidanceTargetType.YellowPipette
                    };

                case FehlingAAction.ApplyMixing:
                    return new GuidanceTargetType[]
                    {
                        GuidanceTargetType.FehlingABeaker,
                        GuidanceTargetType.StirringRod
                    };

                default:
                    return System.Array.Empty<GuidanceTargetType>();
            }
        }

        if (!IsFehlingBCompleted)
        {
            switch (CurrentFehlingBAction)
            {
                case FehlingBAction.AddPotassiumTartrate:
                    return new GuidanceTargetType[]
                    {
                        GuidanceTargetType.PotassiumTartratePowder,
                        GuidanceTargetType.Spatula
                    };

                case FehlingBAction.AddNaOH:
                    return new GuidanceTargetType[]
                    {
                        GuidanceTargetType.NaOHBottle,
                        GuidanceTargetType.GreenPipette
                    };

                case FehlingBAction.ApplyMixing:
                    return new GuidanceTargetType[]
                    {
                        GuidanceTargetType.FehlingBBeaker,
                        GuidanceTargetType.StirringRod
                    };

                default:
                    return System.Array.Empty<GuidanceTargetType>();
            }
        }

        if (!IsReactionTubeCompleted)
        {
            switch (CurrentReactionTubeAction)
            {
                case FehlingReactionTubeAction.AddFehlingA:
                    return new GuidanceTargetType[]
                    {
                        GuidanceTargetType.FehlingABeaker,
                        GuidanceTargetType.YellowPipette
                    };

                case FehlingReactionTubeAction.AddFehlingB:
                    return new GuidanceTargetType[]
                    {
                        GuidanceTargetType.FehlingBBeaker,
                        GuidanceTargetType.GreenPipette
                    };

                case FehlingReactionTubeAction.AddGlucose:
                    return new GuidanceTargetType[]
                    {
                        GuidanceTargetType.GlucoseBottle,
                        GuidanceTargetType.YellowPipette
                    };

                case FehlingReactionTubeAction.ApplyHeating:
                    return new GuidanceTargetType[]
                    {
                        GuidanceTargetType.FehlingReactionTube,
                        GuidanceTargetType.ElectricHeaterBeaker,
                        GuidanceTargetType.ElectricHeater
                    };

                default:
                    return System.Array.Empty<GuidanceTargetType>();
            }
        }

        return System.Array.Empty<GuidanceTargetType>();
    }

    private void ShowValidation(bool isCorrectAction, ValidationOutline validationOutline)
    {
        if (!validationEnabled || validationOutline == null)
            return;

        validationOutline.ShowValidation(isCorrectAction);
    }

    private float GetValidationDuration(ValidationOutline validationOutline)
    {
        if (!validationEnabled || validationOutline == null)
            return 0f;

        return validationOutline.GetValidationDuration();
    }

    private void SaveActionResult(FehlingActionType action, bool isCorrectAction)
    {
        int actionIndex = GetActionIndex(action);

        if (actionIndex < 0 || actionIndex >= actionResults.Length)
            return;

        actionResults[actionIndex] = isCorrectAction
            ? ActionResult.Correct
            : ActionResult.Incorrect;
    }

    private void LogActionResult(
        FehlingContainerType container,
        FehlingActionType expectedAction,
        FehlingActionType performedAction,
        bool isCorrectAction)
    {
        Debug.Log(
            $"[FehlingProcedureTracker] Container: {container}; " +
            $"Expected: {expectedAction}; " +
            $"Performed: {performedAction}; " +
            $"Result: {(isCorrectAction ? "Correct" : "Incorrect")}; " +
            $"Action Count: {ActionCount} / {ProcedureActionCount}; " +
            $"Correct Actions: {CorrectActionCount}; " +
            $"Incorrect Actions: {IncorrectActionCount}; " +
            $"Fehling A Action: {CurrentFehlingAAction}; " +
            $"Fehling B Action: {CurrentFehlingBAction}; " +
            $"Reaction Tube Action: {CurrentReactionTubeAction}; " +
            $"Completed: {IsProcedureCompleted}");
    }

    private int GetActionIndex(FehlingActionType action)
    {
        switch (action)
        {
            case FehlingActionType.AddCopperSulfate:
                return 0;
            case FehlingActionType.AddSulfuricAcid:
                return 1;
            case FehlingActionType.ApplyMixingToFehlingA:
                return 2;
            case FehlingActionType.AddPotassiumTartrate:
                return 3;
            case FehlingActionType.AddNaOH:
                return 4;
            case FehlingActionType.ApplyMixingToFehlingB:
                return 5;
            case FehlingActionType.AddFehlingA:
                return 6;
            case FehlingActionType.AddFehlingB:
                return 7;
            case FehlingActionType.AddGlucose:
                return 8;
            case FehlingActionType.ApplyHeating:
                return 9;
            default:
                return -1;
        }
    }

    public ActionResult GetActionResult(FehlingActionType action)
    {
        int actionIndex = GetActionIndex(action);

        if (actionIndex < 0 || actionIndex >= actionResults.Length)
            return ActionResult.NotAttempted;

        return actionResults[actionIndex];
    }

    private void UpdateActionCounts(bool isCorrectAction)
    {
        ActionCount++;

        if (isCorrectAction)
            CorrectActionCount++;
        else
            IncorrectActionCount++;
    }

    public void ResetProcedureTracker()
    {
        if (actionRoutine != null)
        {
            StopCoroutine(actionRoutine);
            actionRoutine = null;
        }

        isRegisteringAction = false;
        IsWaitingForNextAction = false;

        CurrentFehlingAAction = FehlingAAction.AddCopperSulfate;
        CurrentFehlingBAction = FehlingBAction.AddPotassiumTartrate;
        CurrentReactionTubeAction = FehlingReactionTubeAction.AddFehlingA;

        IsFehlingACompleted = false;
        IsFehlingBCompleted = false;
        IsReactionTubeCompleted = false;
        IsProcedureCompleted = false;

        ActionCount = 0;
        CorrectActionCount = 0;
        IncorrectActionCount = 0;

        ResetProcedureTimer();

        for (int i = 0; i < actionResults.Length; i++)
            actionResults[i] = ActionResult.NotAttempted;

        NotifyProcedureActionChanged();
    }
}