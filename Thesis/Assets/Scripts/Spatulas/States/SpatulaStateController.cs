using UnityEngine;

public class SpatulaStateController : MonoBehaviour
{
    public SpatulaState CurrentState { get; private set; } = SpatulaState.Free;

    public bool IsFree => CurrentState == SpatulaState.Free;
    public bool IsBusy => CurrentState != SpatulaState.Free;

    public bool IsInPowder => CurrentState == SpatulaState.InPowder;
    public bool IsInBeaker => CurrentState == SpatulaState.InBeaker;

    public bool IsInserted =>
        CurrentState == SpatulaState.InPowder ||
        CurrentState == SpatulaState.InBeaker;

    public void SetState(SpatulaState newState)
    {
        if (CurrentState == newState)
            return;

        CurrentState = newState;
        Debug.Log($"[SpatulaStateController] Spatula State Changed To: {CurrentState}!");
    }
}