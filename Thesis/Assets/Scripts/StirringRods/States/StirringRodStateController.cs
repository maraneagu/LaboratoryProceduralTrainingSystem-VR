using UnityEngine;

public class StirringRodStateController : MonoBehaviour
{
    public StirringRodState CurrentState { get; private set; } = StirringRodState.Free;

    public bool IsFree => CurrentState == StirringRodState.Free;
    public bool IsBusy => CurrentState != StirringRodState.Free;

    public bool IsInBeaker => CurrentState == StirringRodState.InBeaker;

    public bool IsInserted =>
        CurrentState == StirringRodState.InBeaker;

    public void SetState(StirringRodState newState)
    {
        if (CurrentState == newState)
            return;

        CurrentState = newState;
        Debug.Log($"[StirringRodStateController] Stirring Rod State Changed To: {CurrentState}!");
    }
}
