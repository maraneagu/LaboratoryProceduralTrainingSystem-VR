using UnityEngine;

public class ExperimentGuidanceManager : MonoBehaviour
{
    public GuidanceManager CurrentGuidanceManager { get; private set; }

    public void SetActiveGuidanceManager(GuidanceManager guidanceManager)
    {
        CurrentGuidanceManager = guidanceManager;
    }

    public void ClearActiveGuidanceManager(GuidanceManager guidanceManager)
    {
        if (CurrentGuidanceManager == guidanceManager)
            CurrentGuidanceManager = null;
    }

    public void NotifyInteractionStarted()
    {
        if (CurrentGuidanceManager != null)
            CurrentGuidanceManager.NotifyInteractionStarted();
    }
}