using System.Collections;
using UnityEngine;

public class PipetteInserterReactionTube : MonoBehaviour
{
    private Pipette pipette;
    private ReactionTube reactionTube;

    public bool isInserted =>
        pipette != null &&
        pipette.pipetteStateController != null &&
        pipette.pipetteStateController.IsInReactionTube;

    [Header("Insertion Animation")]
    public float insertAnimationDuration = 0.25f;
    private Coroutine insertAnimationRoutine;

    [Header("Insertion Duration")]
    public float insertDuration = 0.75f;

    [Header("Reinsertion Unlock Distance")]
    public float reinsertionUnlockDistance = 0.08f;

    [Header("Audio")]
    [SerializeField] private AudioClip pourLiquidAudio;
    [SerializeField] private float pourLiquidVolume = 2f;

    private bool reinsertionLocked = false;

    private ReactionTubeFillZone lockedReactionTubeFillZone = null;
    private Transform lockedReactionTubeInsertPoint = null;

    private void Awake()
    {
        pipette = GetComponent<Pipette>();
    }

    private void Update()
    {
        HandleReinsertionUnlock();

        if (!isInserted || reactionTube == null || reactionTube.pipetteInsertPoint == null)
            return;

        if (pipette == null || pipette.grabInteractable == null || !pipette.grabInteractable.isSelected)
        {
            transform.position = reactionTube.pipetteInsertPoint.position;
            transform.rotation = reactionTube.pipetteInsertPoint.rotation;
            return;
        }
    }

    private void HandleReinsertionUnlock()
    {
        if (!reinsertionLocked || lockedReactionTubeInsertPoint == null)
            return;

        float unlockDistance = Vector3.Distance(transform.position, lockedReactionTubeInsertPoint.position);

        if (unlockDistance <= reinsertionUnlockDistance)
            return;

        reinsertionLocked = false;
        lockedReactionTubeFillZone = null;
        lockedReactionTubeInsertPoint = null;

        Debug.Log("[PipetteInserterReactionTube] Reaction Tube Reinsertion Allowed!");
    }

    public bool CanInsert()
    {
        if (isInserted || reinsertionLocked)
            return false;

        if (pipette == null || pipette.pipetteLiquid == null || !pipette.pipetteLiquid.HasLiquid)
        {
            Debug.Log("[PipetteInserterReactionTube] Pipette Is Empty!");
            return false;
        }

        bool hasReagent = pipette.pipetteLiquid.CurrentReagent != ReagentType.None;
        bool hasSolution = pipette.pipetteLiquid.CurrentSolution != null;

        if (!hasReagent && !hasSolution)
        {
            Debug.Log("[PipetteInserterReactionTube] Pipette Has No Reagent Or Solution Assigned!");
            return false;
        }
   
        return true;
    }

    public void InsertIntoReactionTube(ReactionTube _reactionTube)
    {
        if (!CanInsert() || _reactionTube == null || _reactionTube.pipetteInsertPoint == null)
            return;

        reactionTube = _reactionTube;

        if (pipette != null && pipette.pipetteStateController != null)
            pipette.pipetteStateController.SetState(PipetteState.InReactionTube);

        reactionTube.isPipetteInserted = true;
        reactionTube.reactionTubeFillZone?.DisableZone();

        if (pipette != null && pipette.pipetteCollider != null && reactionTube.reactionTubeCollider != null)
            Physics.IgnoreCollision(pipette.pipetteCollider, reactionTube.reactionTubeCollider, true);

        ForceReleaseIfHeld();
        LockRigidBody();

        if (insertAnimationRoutine != null)
            StopCoroutine(insertAnimationRoutine);

        insertAnimationRoutine = StartCoroutine(SmoothInsertIntoReactionTube());
    }

    private IEnumerator SmoothInsertIntoReactionTube()
    {
        if (reactionTube == null || reactionTube.pipetteInsertPoint == null)
            yield break;

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        Vector3 endPosition = reactionTube.pipetteInsertPoint.position;
        Quaternion endRotation = reactionTube.pipetteInsertPoint.rotation;

        float elapsed = 0f;

        while (elapsed < insertAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float time = Mathf.Clamp01(elapsed / insertAnimationDuration);
            time = time * time * (3f - 2f * time);

            transform.position = Vector3.Lerp(startPosition, endPosition, time);
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, time);

            yield return null;
        }

        transform.position = endPosition;
        transform.rotation = endRotation;

        Debug.Log("[PipetteInserterReactionTube] Pipette Inserted Into Reaction Tube!");

        yield return new WaitForSeconds(insertDuration);

        AudioManager.Instance?.PlayAudio(pourLiquidAudio, pourLiquidVolume);

        EmptyPipetteIntoReactionTube();

        yield return new WaitForSeconds(insertDuration);

        RemoveFromReactionTube();

        if (pipette != null && pipette.pipetteReturnOriginalPoint != null)
        {
            pipette.pipetteReturnOriginalPoint.Return();
        }

        Debug.Log("[PipetteInserterReactionTube] Pipette Can Be Picked Up Again!");
    }

    private void EmptyPipetteIntoReactionTube()
    {
        if (pipette == null || pipette.pipetteLiquid == null || !pipette.pipetteLiquid.HasLiquid)
            return;

        if (reactionTube == null || reactionTube.reactionTubeLiquid == null)
            return;

        if (reactionTube.reactionTubeLiquid.IsFull())
            return;

        BeakerLiquid solution = pipette.pipetteLiquid.CurrentSolution;
        ReagentType reagent = pipette.pipetteLiquid.CurrentReagent;

        if (solution != null)
        {
            bool addSolution = reactionTube.reactionTubeLiquid.AddSolution(solution);
            if (!addSolution)
                return;

            pipette.pipetteLiquid.EmptyPipette();

            Debug.Log($"[PipetteInserterReactionTube] Pipette Emptied {solution.name} Into Reaction Tube!");
            return;
        }

        if (reagent != ReagentType.None)
        {
            bool addReagent = reactionTube.reactionTubeLiquid.AddReagent(reagent);
            if (!addReagent)
                return;

            pipette.pipetteLiquid.EmptyPipette();

            Debug.Log($"[PipetteInserterReactionTube] Pipette Emptied {reagent} Into Reaction Tube!");
        }
    }

    private void RemoveFromReactionTube()
    {
        if (pipette != null && pipette.pipetteStateController != null)
            pipette.pipetteStateController.SetState(PipetteState.Free);

        if (pipette != null && pipette.rigidBody != null)
        {
            pipette.rigidBody.isKinematic = false;
            pipette.rigidBody.useGravity = false;
            pipette.rigidBody.linearVelocity = Vector3.zero;
            pipette.rigidBody.angularVelocity = Vector3.zero;
        }

        if (reactionTube != null && pipette != null && pipette.pipetteCollider != null && reactionTube.reactionTubeCollider != null)
            Physics.IgnoreCollision(pipette.pipetteCollider, reactionTube.reactionTubeCollider, false);

        if (reactionTube != null)
        {
            reactionTube.isPipetteInserted = false;
            reactionTube.reactionTubeFillZone?.EnableZone();

            reinsertionLocked = true;
            lockedReactionTubeFillZone = reactionTube.reactionTubeFillZone;
            lockedReactionTubeInsertPoint = reactionTube.pipetteInsertPoint;
            reactionTube = null;
        }

        Debug.Log("[PipetteInserterReactionTube] Pipette Removed From Reaction Tube!");
    }

    private void ForceReleaseIfHeld()
    {
        if (pipette == null || pipette.grabInteractable == null || !pipette.grabInteractable.isSelected)
            return;

        var interactor = pipette.grabInteractable.firstInteractorSelecting;

        if (interactor != null && pipette.grabInteractable.interactionManager != null)
            pipette.grabInteractable.interactionManager.SelectExit(interactor, pipette.grabInteractable);
    }

    private void LockRigidBody()
    {
        if (pipette == null || pipette.rigidBody == null)
            return;

        pipette.rigidBody.linearVelocity = Vector3.zero;
        pipette.rigidBody.angularVelocity = Vector3.zero;
        pipette.rigidBody.isKinematic = true;
        pipette.rigidBody.useGravity = false;
    }
}