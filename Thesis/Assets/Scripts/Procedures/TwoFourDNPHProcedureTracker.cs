using System.Collections;
using UnityEngine;

public class TwoFourDNPHProcedureTracker : ProcedureTracker
{
    private const int ProcedureActionCount = 6;

    public enum TwoFourDNPHBeakerAction
    {
        AddTwoFourDNPH,
        AddPhosphoricAcid,
        ApplyMixing,
        AddEthanol,
        Completed
    }

    public enum TwoFourDNPHReactionTubeAction
    {
        AddTwoFourDNPHSolution,
        AddPMethoxybenzaldehyde,
        Completed
    }

    public TwoFourDNPHBeakerAction CurrentBeakerAction { get; private set; }
    public TwoFourDNPHReactionTubeAction CurrentReactionTubeAction { get; private set; }

    public bool IsBeakerCompleted { get; private set; }
    public bool IsReactionTubeCompleted { get; private set; }

    public int ActionCount { get; private set; }
    public int CorrectActionCount { get; private set; }
    public int IncorrectActionCount { get; private set; }

    private readonly ActionResult[] actionResults = new ActionResult[6];

    private bool isRegisteringAction;
    private Coroutine actionRoutine;

    protected override void Awake()
    {
        base.Awake();
        ResetProcedureTracker();
    }

    public void RegisterAction(
        TwoFourDNPHContainerType container,
        TwoFourDNPHActionType performedAction,
        ValidationOutline validationOutline = null)
    {
        if (IsProcedureCompleted || isRegisteringAction)
            return;

        actionRoutine = StartCoroutine(RegisterActionRoutine(container, performedAction, validationOutline));
    }

    private IEnumerator RegisterActionRoutine(
        TwoFourDNPHContainerType container,
        TwoFourDNPHActionType performedAction,
        ValidationOutline validationOutline = null)
    {
        isRegisteringAction = true;

        TwoFourDNPHActionType expectedAction = GetExpectedAction();
        bool isCorrectAction = EvaluateAndRecordAction(container, expectedAction, performedAction);

        yield return ValidateAction(isCorrectAction, validationOutline);

        NextAction();
        EvaluateProcedureCompletion();

        actionRoutine = null;
        isRegisteringAction = false;
    }

    private bool EvaluateAndRecordAction(
        TwoFourDNPHContainerType container,
        TwoFourDNPHActionType expectedAction,
        TwoFourDNPHActionType performedAction)
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

    private TwoFourDNPHActionType GetExpectedAction()
    {
        if (!IsBeakerCompleted)
            return GetExpectedBeakerAction();

        if (!IsReactionTubeCompleted)
            return GetExpectedReactionTubeAction();

        return TwoFourDNPHActionType.None;
    }

    private TwoFourDNPHActionType GetExpectedBeakerAction()
    {
        switch (CurrentBeakerAction)
        {
            case TwoFourDNPHBeakerAction.AddTwoFourDNPH:
                return TwoFourDNPHActionType.AddTwoFourDNPH;

            case TwoFourDNPHBeakerAction.AddPhosphoricAcid:
                return TwoFourDNPHActionType.AddPhosphoricAcid;

            case TwoFourDNPHBeakerAction.ApplyMixing:
                return TwoFourDNPHActionType.ApplyMixing;

            case TwoFourDNPHBeakerAction.AddEthanol:
                return TwoFourDNPHActionType.AddEthanol;

            case TwoFourDNPHBeakerAction.Completed:
            default:
                return TwoFourDNPHActionType.None;
        }
    }

    private TwoFourDNPHActionType GetExpectedReactionTubeAction()
    {
        switch (CurrentReactionTubeAction)
        {
            case TwoFourDNPHReactionTubeAction.AddTwoFourDNPHSolution:
                return TwoFourDNPHActionType.AddTwoFourDNPHSolutionToReactionTube;

            case TwoFourDNPHReactionTubeAction.AddPMethoxybenzaldehyde:
                return TwoFourDNPHActionType.AddPMethoxybenzaldehyde;

            case TwoFourDNPHReactionTubeAction.Completed:
            default:
                return TwoFourDNPHActionType.None;
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
            case TwoFourDNPHBeakerAction.AddTwoFourDNPH:
                CurrentBeakerAction = TwoFourDNPHBeakerAction.AddPhosphoricAcid;
                break;

            case TwoFourDNPHBeakerAction.AddPhosphoricAcid:
                CurrentBeakerAction = TwoFourDNPHBeakerAction.ApplyMixing;
                break;

            case TwoFourDNPHBeakerAction.ApplyMixing:
                CurrentBeakerAction = TwoFourDNPHBeakerAction.AddEthanol;
                break;

            case TwoFourDNPHBeakerAction.AddEthanol:
                CurrentBeakerAction = TwoFourDNPHBeakerAction.Completed;
                IsBeakerCompleted = true;
                break;
        }

        NotifyProcedureActionChanged();
    }

    private void NextReactionTubeAction()
    {
        switch (CurrentReactionTubeAction)
        {
            case TwoFourDNPHReactionTubeAction.AddTwoFourDNPHSolution:
                CurrentReactionTubeAction = TwoFourDNPHReactionTubeAction.AddPMethoxybenzaldehyde;
                break;

            case TwoFourDNPHReactionTubeAction.AddPMethoxybenzaldehyde:
                CurrentReactionTubeAction = TwoFourDNPHReactionTubeAction.Completed;
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
                case TwoFourDNPHBeakerAction.AddTwoFourDNPH:
                    return new GuidanceTargetType[]
                    {
                        GuidanceTargetType.TwoFourDNPHPowder,
                        GuidanceTargetType.Spatula
                    };

                case TwoFourDNPHBeakerAction.AddPhosphoricAcid:
                    return new GuidanceTargetType[]
                    {
                        GuidanceTargetType.PhosphoricAcidBottle,
                        GuidanceTargetType.BluePipette
                    };

                case TwoFourDNPHBeakerAction.ApplyMixing:
                    return new GuidanceTargetType[]
                    {
                        GuidanceTargetType.TwoFourDNPHBeaker,
                        GuidanceTargetType.StirringRod
                    };

                case TwoFourDNPHBeakerAction.AddEthanol:
                    return new GuidanceTargetType[]
                    {
                        GuidanceTargetType.EthanolBottle,
                        GuidanceTargetType.TwoFourDNPHBeaker
                    };

                default:
                    return System.Array.Empty<GuidanceTargetType>();
            }
        }

        if (!IsReactionTubeCompleted)
        {
            switch (CurrentReactionTubeAction)
            {
                case TwoFourDNPHReactionTubeAction.AddTwoFourDNPHSolution:
                    return new GuidanceTargetType[]
                    {
                        GuidanceTargetType.TwoFourDNPHBeaker,
                        GuidanceTargetType.BluePipette
                    };

                case TwoFourDNPHReactionTubeAction.AddPMethoxybenzaldehyde:
                    return new GuidanceTargetType[]
                    {
                        GuidanceTargetType.PMethoxybenzaldehydeBottle,
                        GuidanceTargetType.YellowPipette
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

    private void SaveActionResult(TwoFourDNPHActionType action, bool isCorrectAction)
    {
        int actionIndex = GetActionIndex(action);

        if (actionIndex < 0 || actionIndex >= actionResults.Length)
            return;

        actionResults[actionIndex] = isCorrectAction
            ? ActionResult.Correct
            : ActionResult.Incorrect;
    }

    private void LogActionResult(
        TwoFourDNPHContainerType container,
        TwoFourDNPHActionType expectedAction,
        TwoFourDNPHActionType performedAction,
        bool isCorrectAction)
    {
        Debug.Log(
            $"[TwoFourDNPHProcedureTracker] Container: {container}; " +
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

    private int GetActionIndex(TwoFourDNPHActionType action)
    {
        switch (action)
        {
            case TwoFourDNPHActionType.AddTwoFourDNPH:
                return 0;

            case TwoFourDNPHActionType.AddPhosphoricAcid:
                return 1;

            case TwoFourDNPHActionType.ApplyMixing:
                return 2;

            case TwoFourDNPHActionType.AddEthanol:
                return 3;

            case TwoFourDNPHActionType.AddTwoFourDNPHSolutionToReactionTube:
                return 4;

            case TwoFourDNPHActionType.AddPMethoxybenzaldehyde:
                return 5;

            default:
                return -1;
        }
    }

    public ActionResult GetActionResult(TwoFourDNPHActionType action)
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

        CurrentBeakerAction = TwoFourDNPHBeakerAction.AddTwoFourDNPH;
        CurrentReactionTubeAction = TwoFourDNPHReactionTubeAction.AddTwoFourDNPHSolution;

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