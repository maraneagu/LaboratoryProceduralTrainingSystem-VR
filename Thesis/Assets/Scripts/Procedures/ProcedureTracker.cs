using System;
using UnityEngine;

public abstract class ProcedureTracker : MonoBehaviour
{
    [Header("Validation Settings")]
    [SerializeField] protected bool validationEnabled = true;

    [Header("Guidance Settings")]
    [SerializeField] protected bool guidanceEnabled = true;

    [Header("Experiment Completion")]
    [SerializeField] private ExperimentCompletionManager experimentCompletionManager;

    protected ProcedureTimer procedureTimer;

    public bool IsValidationEnabled => validationEnabled;
    public bool IsGuidanceEnabled => guidanceEnabled;

    public bool IsProcedureCompleted { get; protected set; }
    public bool IsWaitingForNextAction { get; protected set; }

    public event Action OnProcedureActionChanged;

    protected virtual void Awake()
    {
        procedureTimer = new ProcedureTimer(GetProcedureName());
    }

    protected virtual string GetProcedureName()
    {
        return GetType().Name.Replace("ProcedureTracker", "");
    }

    protected virtual string GetParticipantId()
    {
        if (Participant.Instance != null)
        {
            string participantId = Participant.Instance.ParticipantId;

            if (!string.IsNullOrWhiteSpace(participantId))
                return participantId;
        }

        return "UnknownParticipant";
    }

    protected virtual string GetParticipantDominantHand()
    {
        if (Participant.Instance != null)
            return Participant.Instance.ParticipantDominantHand.ToString();

        return "UnknownDominantHand";
    }

    protected void NotifyProcedureActionChanged()
    {
        OnProcedureActionChanged?.Invoke();
    }

    protected void RecordAction(string expectedAction, string performedAction, bool isCorrect)
    {
        procedureTimer.RecordAction(expectedAction, performedAction, isCorrect);
    }

    protected void CompleteProcedure(int correctActionCount, int incorrectActionCount)
    {
        if (experimentCompletionManager != null)
            experimentCompletionManager.CompleteExperiment();

        procedureTimer.CompleteProcedure(
            GetParticipantId(),
            GetParticipantDominantHand(),
            guidanceEnabled,
            validationEnabled,
            correctActionCount,
            incorrectActionCount);
    }

    protected void ResetProcedureTimer()
    {
        procedureTimer.ResetProcedureTimer();
    }

    public abstract GuidanceTargetType[] GetCurrentGuidanceTargets();
}