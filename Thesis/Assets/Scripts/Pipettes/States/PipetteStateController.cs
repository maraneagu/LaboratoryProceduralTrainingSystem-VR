using UnityEngine;

public class PipetteStateController : MonoBehaviour
{
    public PipetteState CurrentState { get; private set; } = PipetteState.Free;

    public bool IsFree => CurrentState == PipetteState.Free;
    public bool IsBusy => CurrentState != PipetteState.Free;

    public bool IsInBottle => CurrentState == PipetteState.InBottle;
    public bool IsInReactionTube => CurrentState == PipetteState.InReactionTube;
    public bool IsInBeaker => CurrentState == PipetteState.InBeaker;

    public bool IsInserted =>
        CurrentState == PipetteState.InBottle ||
        CurrentState == PipetteState.InReactionTube ||
        CurrentState == PipetteState.InBeaker;

    public void SetState(PipetteState newState)
    {
        if (CurrentState == newState)
            return;

        CurrentState = newState;
        Debug.Log($"[PipetteStateController] Pipette State Changed To: {CurrentState}!");
    }
}