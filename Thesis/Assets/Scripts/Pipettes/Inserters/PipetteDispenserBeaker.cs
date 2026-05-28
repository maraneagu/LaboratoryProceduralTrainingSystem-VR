using System.Collections;
using UnityEngine;

public class PipetteDispenserBeaker : MonoBehaviour
{
    private Pipette pipette;
    private Beaker beaker;

    public bool isInserted =>
        pipette != null &&
        pipette.pipetteStateController != null &&
        pipette.pipetteStateController.IsInBeaker;

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

    private BeakerFillZone lockedBeakerFillZone = null;
    private Transform lockedBeakerDispenserPoint = null;

    private void Awake()
    {
        pipette = GetComponent<Pipette>();
    }

    private void Update()
    {
        HandleReinsertionUnlock();

        if (!isInserted || beaker == null || beaker.pipetteDispenserPoint == null)
            return;

        if (pipette == null || pipette.grabInteractable == null || !pipette.grabInteractable.isSelected)
        {
            transform.position = beaker.pipetteDispenserPoint.position;
            transform.rotation = beaker.pipetteDispenserPoint.rotation;
            return;
        }
    }

    private void HandleReinsertionUnlock()
    {
        if (!reinsertionLocked || lockedBeakerDispenserPoint == null)
            return;

        float unlockDistance = Vector3.Distance(transform.position, lockedBeakerDispenserPoint.position);

        if (unlockDistance <= reinsertionUnlockDistance)
            return;

        reinsertionLocked = false;
        lockedBeakerFillZone = null;
        lockedBeakerDispenserPoint = null;

        Debug.Log("[PipetteInserterBeaker] Beaker Reinsertion Allowed!");
    }

    public bool CanInsert()
    {
        if (isInserted || reinsertionLocked)
            return false;

        if (pipette == null || pipette.pipetteLiquid == null || !pipette.pipetteLiquid.HasLiquid)
        {
            Debug.Log("[PipetteInserterBeaker] Pipette Is Empty!");
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

    public void InsertIntoBeaker(Beaker _beaker)
    {
        if (!CanInsert() || _beaker == null || _beaker.pipetteDispenserPoint == null)
            return;

        beaker = _beaker;

        if (pipette != null && pipette.pipetteStateController != null)
            pipette.pipetteStateController.SetState(PipetteState.InBeaker);

        beaker.isPipetteInserted = true;
        beaker.beakerFillZone?.DisableZone();

        if (pipette != null && pipette.pipetteCollider != null && beaker.beakerCollider != null)
            Physics.IgnoreCollision(pipette.pipetteCollider, beaker.beakerCollider, true);

        ForceReleaseIfHeld();
        LockRigidBody();

        if (insertAnimationRoutine != null)
            StopCoroutine(insertAnimationRoutine);

        insertAnimationRoutine = StartCoroutine(SmoothInsertIntoBeaker());
    }

    private IEnumerator SmoothInsertIntoBeaker()
    {
        if (beaker == null || beaker.pipetteDispenserPoint == null)
            yield break;

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        Vector3 endPosition = beaker.pipetteDispenserPoint.position;
        Quaternion endRotation = beaker.pipetteDispenserPoint.rotation;

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

        Debug.Log("[PipetteInserterBeaker] Pipette Inserted Into Beaker!");

        yield return new WaitForSeconds(insertDuration);

        AudioManager.Instance?.PlayAudio(pourLiquidAudio, pourLiquidVolume);

        EmptyPipetteIntoBeaker();

        yield return new WaitForSeconds(insertDuration);

        RemoveFromBeaker();

        if (pipette != null && pipette.pipetteReturnOriginalPoint != null)
        {
            pipette.pipetteReturnOriginalPoint.Return();
        }

        Debug.Log("[PipetteInserterBeaker] Pipette Can Be Picked Up Again!");
    }

    private void EmptyPipetteIntoBeaker()
    {
        if (pipette == null || pipette.pipetteLiquid == null || !pipette.pipetteLiquid.HasLiquid)
            return;

        if (beaker == null || beaker.beakerLiquid == null)
            return;

        if (beaker.beakerLiquid.IsFull())
            return;

        ReagentType reagent = pipette.pipetteLiquid.CurrentReagent;

        if (reagent == ReagentType.None)
            return;

        bool addReagent = beaker.beakerLiquid.AddReagent(reagent);
        if (!addReagent)
            return;

        pipette.pipetteLiquid.EmptyPipette();

        Debug.Log($"[PipetteInserterBeaker] Pipette Emptied {reagent} Into Beaker!");
    }

    private void RemoveFromBeaker()
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

        if (beaker != null && pipette != null && pipette.pipetteCollider != null && beaker.beakerCollider != null)
            Physics.IgnoreCollision(pipette.pipetteCollider, beaker.beakerCollider, false);

        if (beaker != null)
        {
            beaker.isPipetteInserted = false;
            beaker.beakerFillZone?.EnableZone();

            reinsertionLocked = true;
            lockedBeakerFillZone = beaker.beakerFillZone;
            lockedBeakerDispenserPoint = beaker.pipetteDispenserPoint;
            beaker = null;
        }

        Debug.Log("[PipetteInserterBeaker] Pipette Removed From Beaker!");
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