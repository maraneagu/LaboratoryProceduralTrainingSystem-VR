using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuidanceManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ProcedureTracker procedureTracker;
    [SerializeField] private GuidanceTarget[] guidanceTargets;
    [SerializeField] private ExperimentGuidanceManager experimentGuidanceManager;

    [Header("Timing Settings")]
    [SerializeField] private float initialGuidanceDelay = 5f;
    [SerializeField] private float guidanceDelay = 3f;

    [Header("Audio")]
    [SerializeField] private AudioClip guidanceAudio;
    [SerializeField] private float guidanceVolume = 0.2f;

    private readonly List<GuidanceTarget> currentGuidanceTargets = new List<GuidanceTarget>();
    private Coroutine delayedGuidanceRoutine;

    private bool hasShownInitialGuidance;

    private int currentActionIndex = -1;
    private int dismissToActionIndex = -1;

    private void OnEnable()
    {
        if (experimentGuidanceManager != null)
            experimentGuidanceManager.SetActiveGuidanceManager(this);

        if (procedureTracker != null)
            procedureTracker.OnProcedureActionChanged += ProcedureActionChanged;

        ResetGuidance();
    }

    private void OnDisable()
    {
        if (procedureTracker != null)
            procedureTracker.OnProcedureActionChanged -= ProcedureActionChanged;

        if (experimentGuidanceManager != null && experimentGuidanceManager.CurrentGuidanceManager == this)
            experimentGuidanceManager.ClearActiveGuidanceManager(this);

        StopDelayedGuidanceRoutine();
        DismissGuidance();
        ResetGuidance();
    }

    private void Start()
    {
        ProcedureActionChanged();
    }

    public void NotifyInteractionStarted()
    {
        if (currentActionIndex < 0)
            return;

        int index = currentActionIndex;

        if (procedureTracker != null && procedureTracker.IsWaitingForNextAction)
            index = currentActionIndex + 1;

        if (index > dismissToActionIndex)
            dismissToActionIndex = index;

        StopDelayedGuidanceRoutine();
        DismissGuidance();
    }

    private void ProcedureActionChanged()
    {
        currentActionIndex++;

        StopDelayedGuidanceRoutine();
        DismissGuidance();

        delayedGuidanceRoutine = StartCoroutine(ShowGuidance());
    }

    private IEnumerator ShowGuidance()
    {
        float delay = hasShownInitialGuidance ? guidanceDelay : initialGuidanceDelay;

        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        delayedGuidanceRoutine = null;
        ShowGuidanceAfterDelay();
    }

    private void ShowGuidanceAfterDelay()
    {
        List<GuidanceTarget> guidanceTargets = GetGuidanceTargets();

        if (AreCurrentGuidanceTargets(currentGuidanceTargets, guidanceTargets))
            return;

        ShowGuidanceTargets(guidanceTargets);
    }

    private List<GuidanceTarget> GetGuidanceTargets()
    {
        if (procedureTracker == null)
            return new List<GuidanceTarget>();

        if (IsGuidanceDismissed())
            return new List<GuidanceTarget>();

        GuidanceTargetType[] guidanceTargetTypes = procedureTracker.GetCurrentGuidanceTargets();
        List<GuidanceTarget> guidanceTargets = FindGuidanceTargets(guidanceTargetTypes);

        return guidanceTargets;
    }

    private void ShowGuidanceTargets(List<GuidanceTarget> guidanceTargets)
    {
        DismissGuidance();

        if (guidanceTargets == null || guidanceTargets.Count == 0)
            return;

        for (int i = 0; i < guidanceTargets.Count; i++)
        {
            GuidanceTarget guidanceTarget = guidanceTargets[i];

            if (guidanceTarget == null)
                continue;

            guidanceTarget.ShowGuidance();
            currentGuidanceTargets.Add(guidanceTarget);
        }

        if (currentGuidanceTargets.Count > 0)
        {
            AudioManager.Instance?.PlayAudio(guidanceAudio, guidanceVolume);
            hasShownInitialGuidance = true;
        }
    }

    private void DismissGuidance()
    {
        for (int i = 0; i < currentGuidanceTargets.Count; i++)
        {
            if (currentGuidanceTargets[i] != null)
                currentGuidanceTargets[i].HideGuidance();
        }

        currentGuidanceTargets.Clear();
    }

    private List<GuidanceTarget> FindGuidanceTargets(GuidanceTargetType[] guidanceTargetTypes)
    {
        List<GuidanceTarget> _guidanceTargets = new List<GuidanceTarget>();

        if (guidanceTargetTypes == null || guidanceTargetTypes.Length == 0)
            return _guidanceTargets;

        for (int i = 0; i < guidanceTargetTypes.Length; i++)
        {
            GuidanceTargetType guidanceTargetType = guidanceTargetTypes[i];

            for (int j = 0; j < guidanceTargets.Length; j++)
            {
                GuidanceTarget guidanceTarget = guidanceTargets[j];

                if (guidanceTarget != null && guidanceTarget.TargetType == guidanceTargetType)
                {
                    _guidanceTargets.Add(guidanceTarget);
                    break;
                }
            }
        }

        return _guidanceTargets;
    }

    private bool IsGuidanceDismissed()
    {
        return currentActionIndex <= dismissToActionIndex;
    }

    private bool AreCurrentGuidanceTargets(List<GuidanceTarget> currentGuidanceTargets, List<GuidanceTarget> nextGuidanceTargets)
    {
        if (currentGuidanceTargets.Count != nextGuidanceTargets.Count)
            return false;

        for (int i = 0; i < currentGuidanceTargets.Count; i++)
        {
            if (!nextGuidanceTargets.Contains(currentGuidanceTargets[i]))
                return false;
        }

        return true;
    }

    private void StopDelayedGuidanceRoutine()
    {
        if (delayedGuidanceRoutine == null)
            return;

        StopCoroutine(delayedGuidanceRoutine);
        delayedGuidanceRoutine = null;
    }

    private void ResetGuidance()
    {
        hasShownInitialGuidance = false;

        currentActionIndex = -1;
        dismissToActionIndex = -1;
    }
}