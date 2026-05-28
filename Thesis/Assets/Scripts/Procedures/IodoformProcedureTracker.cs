using System.Collections;
using UnityEngine;

public class IodoformProcedureTracker : ProcedureTracker
{
    private const int ProcedureActionCount = 5;

    public IodoformActionType CurrentAction { get; private set; } = IodoformActionType.AddAcetone;

    public int ActionCount { get; private set; }
    public int CorrectActionCount { get; private set; }
    public int IncorrectActionCount { get; private set; }

    private readonly ActionResult[] iodoformActionResults = new ActionResult[5];

    private bool isRegisteringAction;
    private Coroutine actionRoutine;

    protected override void Awake()
    {
        base.Awake();
        ResetProcedureTracker();
    }

    public void RegisterAction(IodoformActionType performedAction, ValidationOutline validationOutline = null)
    {
        if (IsProcedureCompleted || isRegisteringAction)
            return;

        actionRoutine = StartCoroutine(RegisterActionRoutine(performedAction, validationOutline));
    }

    private IEnumerator RegisterActionRoutine(IodoformActionType performedAction, ValidationOutline validationOutline = null)
    {
        isRegisteringAction = true;

        IodoformActionType expectedAction = CurrentAction;
        bool isCorrectAction = EvaluateAndRecordAction(expectedAction, performedAction);

        yield return ValidateAction(isCorrectAction, validationOutline);

        NextAction();
        EvaluateProcedureCompletion();

        actionRoutine = null;
        isRegisteringAction = false;
    }

    private bool EvaluateAndRecordAction(IodoformActionType expectedAction, IodoformActionType performedAction)
    {
        bool isCorrectAction = performedAction == expectedAction;

        RecordAction(expectedAction.ToString(), performedAction.ToString(), isCorrectAction);

        SaveActionResult(performedAction, isCorrectAction);
        LogActionResult(expectedAction, performedAction, isCorrectAction);

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

    private void NextAction()
    {
        switch (CurrentAction)
        {
            case IodoformActionType.AddAcetone:
                CurrentAction = IodoformActionType.AddDistilledWater;
                break;

            case IodoformActionType.AddDistilledWater:
                CurrentAction = IodoformActionType.AddNaOH;
                break;

            case IodoformActionType.AddNaOH:
                CurrentAction = IodoformActionType.AddIodine;
                break;

            case IodoformActionType.AddIodine:
                CurrentAction = IodoformActionType.ApplyHeating;
                break;

            case IodoformActionType.ApplyHeating:
                CurrentAction = IodoformActionType.None;
                break;
        }

        NotifyProcedureActionChanged();
    }

    public override GuidanceTargetType[] GetCurrentGuidanceTargets()
    {
        if (!guidanceEnabled || IsProcedureCompleted)
            return System.Array.Empty<GuidanceTargetType>();

        switch (CurrentAction)
        {
            case IodoformActionType.AddAcetone:
                return new GuidanceTargetType[]
                {
                    GuidanceTargetType.AcetoneBottle,
                    GuidanceTargetType.BluePipette
                };

            case IodoformActionType.AddDistilledWater:
                return new GuidanceTargetType[]
                {
                    GuidanceTargetType.DistilledWaterBottle,
                    GuidanceTargetType.IodoformReactionTube
                };

            case IodoformActionType.AddNaOH:
                return new GuidanceTargetType[]
                {
                    GuidanceTargetType.NaOHBottle,
                    GuidanceTargetType.GreenPipette
                };

            case IodoformActionType.AddIodine:
                return new GuidanceTargetType[]
                {
                    GuidanceTargetType.IodineBottle,
                    GuidanceTargetType.YellowPipette
                };

            case IodoformActionType.ApplyHeating:
                return new GuidanceTargetType[]
                {
                    GuidanceTargetType.IodoformReactionTube,
                    GuidanceTargetType.ElectricHeaterBeaker,
                    GuidanceTargetType.ElectricHeater
                };

            default:
                return System.Array.Empty<GuidanceTargetType>();
        }
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

    private void SaveActionResult(IodoformActionType action, bool isCorrectAction)
    {
        int actionIndex = GetActionIndex(action);

        if (actionIndex < 0 || actionIndex >= iodoformActionResults.Length)
            return;

        iodoformActionResults[actionIndex] = isCorrectAction
            ? ActionResult.Correct
            : ActionResult.Incorrect;
    }

    private void LogActionResult(IodoformActionType expectedAction, IodoformActionType performedAction, bool isCorrectAction)
    {
        Debug.Log(
            $"[IodoformProcedureTracker] Expected: {expectedAction}; " +
            $"Performed: {performedAction}; " +
            $"Result: {(isCorrectAction ? "Correct" : "Incorrect")}; " +
            $"Action Count: {ActionCount} / {ProcedureActionCount}; " +
            $"Correct Actions: {CorrectActionCount}; " +
            $"Incorrect Actions: {IncorrectActionCount}; " +
            $"Next Action: {(IsProcedureCompleted ? "Completed" : CurrentAction.ToString())}");
    }

    private int GetActionIndex(IodoformActionType action)
    {
        switch (action)
        {
            case IodoformActionType.AddAcetone:
                return 0;

            case IodoformActionType.AddDistilledWater:
                return 1;

            case IodoformActionType.AddNaOH:
                return 2;

            case IodoformActionType.AddIodine:
                return 3;

            case IodoformActionType.ApplyHeating:
                return 4;

            default:
                return -1;
        }
    }

    public ActionResult GetActionResult(IodoformActionType action)
    {
        int actionIndex = GetActionIndex(action);

        if (actionIndex < 0 || actionIndex >= iodoformActionResults.Length)
            return ActionResult.NotAttempted;

        return iodoformActionResults[actionIndex];
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

        CurrentAction = IodoformActionType.AddAcetone;
        IsProcedureCompleted = false;

        ActionCount = 0;
        CorrectActionCount = 0;
        IncorrectActionCount = 0;

        ResetProcedureTimer();

        for (int i = 0; i < iodoformActionResults.Length; i++)
            iodoformActionResults[i] = ActionResult.NotAttempted;

        NotifyProcedureActionChanged();
    }
}