using System.Collections;
using UnityEngine;

public class BenedictProcedureTracker : ProcedureTracker
{
    private const int ProcedureActionCount = 8;

    public enum BenedictBeakerAction
    {
        AddCopperSulfate,
        AddSodiumCitrate,
        AddSodiumCarbonate,
        AddDistilledWater,
        ApplyMixing,
        Completed
    }

    public enum BenedictReactionTubeAction
    {
        AddBenedictSolution,
        AddUrine,
        ApplyHeating,
        Completed
    }

    public BenedictBeakerAction CurrentBeakerAction { get; private set; }
    public BenedictReactionTubeAction CurrentReactionTubeAction { get; private set; }

    public bool IsBeakerCompleted { get; private set; }
    public bool IsReactionTubeCompleted { get; private set; }

    public int ActionCount { get; private set; }
    public int CorrectActionCount { get; private set; }
    public int IncorrectActionCount { get; private set; }

    private readonly ActionResult[] actionResults = new ActionResult[8];

    private bool isRegisteringAction;
    private Coroutine actionRoutine;

    protected override void Awake()
    {
        base.Awake();
        ResetProcedureTracker();
    }

    public void RegisterAction(
        BenedictContainerType container,
        BenedictActionType performedAction,
        ValidationOutline validationOutline = null)
    {
        if (IsProcedureCompleted || isRegisteringAction)
            return;

        actionRoutine = StartCoroutine(RegisterActionRoutine(container, performedAction, validationOutline));
    }

    private IEnumerator RegisterActionRoutine(
        BenedictContainerType container,
        BenedictActionType performedAction,
        ValidationOutline validationOutline = null)
    {
        isRegisteringAction = true;

        BenedictActionType expectedAction = GetExpectedAction();
        bool isCorrectAction = EvaluateAndRecordAction(container, expectedAction, performedAction);

        yield return ValidateAction(isCorrectAction, validationOutline);

        NextAction();
        EvaluateProcedureCompletion();

        actionRoutine = null;
        isRegisteringAction = false;
    }

    private bool EvaluateAndRecordAction(
        BenedictContainerType container,
        BenedictActionType expectedAction,
        BenedictActionType performedAction)
    {
        bool isCorrectAction = performedAction == expectedAction;

        RecordAction(expectedAction.ToString(), performedAction.ToString(), isCorrectAction);

        SaveActionResult(expectedAction, isCorrectAction);
        LogActionResult(container, expectedAction, performedAction, isCorrectAction);

        UpdateActionCounts(isCorrectAction);

        return isCorrectAction;
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

    private BenedictActionType GetExpectedAction()
    {
        if (!IsBeakerCompleted)
            return GetExpectedBeakerAction();

        if (!IsReactionTubeCompleted)
            return GetExpectedReactionTubeAction();

        return BenedictActionType.None;
    }

    private BenedictActionType GetExpectedBeakerAction()
    {
        switch (CurrentBeakerAction)
        {
            case BenedictBeakerAction.AddCopperSulfate:
                return BenedictActionType.AddCopperSulfate;

            case BenedictBeakerAction.AddSodiumCitrate:
                return BenedictActionType.AddSodiumCitrate;

            case BenedictBeakerAction.AddSodiumCarbonate:
                return BenedictActionType.AddSodiumCarbonate;

            case BenedictBeakerAction.AddDistilledWater:
                return BenedictActionType.AddDistilledWater;

            case BenedictBeakerAction.ApplyMixing:
                return BenedictActionType.ApplyMixing;

            case BenedictBeakerAction.Completed:
            default:
                return BenedictActionType.None;
        }
    }

    private BenedictActionType GetExpectedReactionTubeAction()
    {
        switch (CurrentReactionTubeAction)
        {
            case BenedictReactionTubeAction.AddBenedictSolution:
                return BenedictActionType.AddBenedictSolution;

            case BenedictReactionTubeAction.AddUrine:
                return BenedictActionType.AddUrine;

            case BenedictReactionTubeAction.ApplyHeating:
                return BenedictActionType.ApplyHeating;

            case BenedictReactionTubeAction.Completed:
            default:
                return BenedictActionType.None;
        }
    }

    private void NextAction()
    {
        if (!IsBeakerCompleted)
        {
            NextBeakerAction();
            return;
        }

        if (!IsReactionTubeCompleted)
        {
            NextReactionTubeAction();
            return;
        }

        NotifyProcedureActionChanged();
    }

    private void NextBeakerAction()
    {
        switch (CurrentBeakerAction)
        {
            case BenedictBeakerAction.AddCopperSulfate:
                CurrentBeakerAction = BenedictBeakerAction.AddSodiumCitrate;
                break;

            case BenedictBeakerAction.AddSodiumCitrate:
                CurrentBeakerAction = BenedictBeakerAction.AddSodiumCarbonate;
                break;

            case BenedictBeakerAction.AddSodiumCarbonate:
                CurrentBeakerAction = BenedictBeakerAction.AddDistilledWater;
                break;

            case BenedictBeakerAction.AddDistilledWater:
                CurrentBeakerAction = BenedictBeakerAction.ApplyMixing;
                break;

            case BenedictBeakerAction.ApplyMixing:
                CurrentBeakerAction = BenedictBeakerAction.Completed;
                IsBeakerCompleted = true;
                break;
        }

        NotifyProcedureActionChanged();
    }

    private void NextReactionTubeAction()
    {
        switch (CurrentReactionTubeAction)
        {
            case BenedictReactionTubeAction.AddBenedictSolution:
                CurrentReactionTubeAction = BenedictReactionTubeAction.AddUrine;
                break;

            case BenedictReactionTubeAction.AddUrine:
                CurrentReactionTubeAction = BenedictReactionTubeAction.ApplyHeating;
                break;

            case BenedictReactionTubeAction.ApplyHeating:
                CurrentReactionTubeAction = BenedictReactionTubeAction.Completed;
                IsReactionTubeCompleted = true;
                break;
        }

        NotifyProcedureActionChanged();
    }

    public override GuidanceTargetType[] GetCurrentGuidanceTargets()
    {
        if (!guidanceEnabled || IsProcedureCompleted)
            return System.Array.Empty<GuidanceTargetType>();

        if (!IsBeakerCompleted)
        {
            switch (CurrentBeakerAction)
            {
                case BenedictBeakerAction.AddCopperSulfate:
                    return new GuidanceTargetType[]
                    {
                        GuidanceTargetType.CopperSulfatePowder,
                        GuidanceTargetType.Spatula
                    };

                case BenedictBeakerAction.AddSodiumCitrate:
                    return new GuidanceTargetType[]
                    {
                        GuidanceTargetType.SodiumCitratePowder,
                        GuidanceTargetType.Spatula
                    };

                case BenedictBeakerAction.AddSodiumCarbonate:
                    return new GuidanceTargetType[]
                    {
                        GuidanceTargetType.SodiumCarbonatePowder,
                        GuidanceTargetType.Spatula
                    };

                case BenedictBeakerAction.AddDistilledWater:
                    return new GuidanceTargetType[]
                    {
                        GuidanceTargetType.DistilledWaterBottle,
                        GuidanceTargetType.BenedictBeaker
                    };

                case BenedictBeakerAction.ApplyMixing:
                    return new GuidanceTargetType[]
                    {
                        GuidanceTargetType.BenedictBeaker,
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
                case BenedictReactionTubeAction.AddBenedictSolution:
                    return new GuidanceTargetType[]
                    {
                        GuidanceTargetType.BenedictBeaker,
                        GuidanceTargetType.YellowPipette
                    };

                case BenedictReactionTubeAction.AddUrine:
                    return new GuidanceTargetType[]
                    {
                        GuidanceTargetType.UrineBottle,
                        GuidanceTargetType.YellowPipette
                    };

                case BenedictReactionTubeAction.ApplyHeating:
                    return new GuidanceTargetType[]
                    {
                        GuidanceTargetType.BenedictReactionTube,
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

    private void SaveActionResult(BenedictActionType action, bool isCorrectAction)
    {
        int actionIndex = GetActionIndex(action);

        if (actionIndex < 0 || actionIndex >= actionResults.Length)
            return;

        actionResults[actionIndex] = isCorrectAction
            ? ActionResult.Correct
            : ActionResult.Incorrect;
    }

    private void LogActionResult(
        BenedictContainerType container,
        BenedictActionType expectedAction,
        BenedictActionType performedAction,
        bool isCorrectAction)
    {
        Debug.Log(
            $"[BenedictProcedureTracker] Container: {container}; " +
            $"Expected: {expectedAction}; " +
            $"Performed: {performedAction}; " +
            $"Result: {(isCorrectAction ? "Correct" : "Incorrect")}; " +
            $"Action Count: {ActionCount} / {ProcedureActionCount}; " +
            $"Correct Actions: {CorrectActionCount}; " +
            $"Incorrect Actions: {IncorrectActionCount}; " +
            $"Beaker Action: {CurrentBeakerAction}; " +
            $"Reaction Tube Action: {CurrentReactionTubeAction}; " +
            $"Completed: {IsProcedureCompleted}");
    }

    private int GetActionIndex(BenedictActionType action)
    {
        switch (action)
        {
            case BenedictActionType.AddCopperSulfate:
                return 0;

            case BenedictActionType.AddSodiumCitrate:
                return 1;

            case BenedictActionType.AddSodiumCarbonate:
                return 2;

            case BenedictActionType.AddDistilledWater:
                return 3;

            case BenedictActionType.ApplyMixing:
                return 4;

            case BenedictActionType.AddBenedictSolution:
                return 5;

            case BenedictActionType.AddUrine:
                return 6;

            case BenedictActionType.ApplyHeating:
                return 7;

            default:
                return -1;
        }
    }

    public ActionResult GetActionResult(BenedictActionType action)
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

        CurrentBeakerAction = BenedictBeakerAction.AddCopperSulfate;
        CurrentReactionTubeAction = BenedictReactionTubeAction.AddBenedictSolution;

        IsBeakerCompleted = false;
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