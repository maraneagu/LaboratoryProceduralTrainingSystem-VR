using System.Collections;
using UnityEngine;

public class PipetteInserterBottle : MonoBehaviour
{
    private Pipette pipette;
    private Bottle bottle;

    public bool isInserted =>
        pipette != null &&
        pipette.pipetteStateController != null &&
        pipette.pipetteStateController.IsInBottle;

    [Header("Insertion Animation")]
    public float insertAnimationDuration = 0.25f;
    private Coroutine insertAnimationRoutine;

    [Header("Insertion Duration")]
    public float insertDuration = 0.75f;

    [Header("Removal Distance")]
    public float removalDistance = 0.05f;

    [Header("Reinsertion Unlock Distance")]
    public float reinsertionUnlockDistance = 0.08f;

    [Header("Audio")]
    [SerializeField] private AudioClip fillWithLiquidAudio;
    [SerializeField] private float fillWithLiquidVolume = 0.15f;

    private bool canBeRemoved = false;
    private bool reinsertionLocked = false;
    private Transform lockedBottleInsertPoint = null;

    private void Awake()
    {
        pipette = GetComponent<Pipette>();
    }

    private void Update()
    {
        HandleReinsertionUnlock();

        if (!isInserted || bottle == null || bottle.pipetteInsertPoint == null)
            return;

        if (pipette == null || pipette.grabInteractable == null || !pipette.grabInteractable.isSelected)
        {
            transform.position = bottle.pipetteInsertPoint.position;
            transform.rotation = bottle.pipetteInsertPoint.rotation;
            return;
        }

        if (!canBeRemoved)
            return;

        float distance = Vector3.Distance(transform.position, bottle.pipetteInsertPoint.position);
        if (distance > removalDistance)
            RemoveFromBottle();
    }

    private void HandleReinsertionUnlock()
    {
        if (!reinsertionLocked || lockedBottleInsertPoint == null)
            return;

        float unlockDistance = Vector3.Distance(transform.position, lockedBottleInsertPoint.position);

        if (unlockDistance <= reinsertionUnlockDistance)
            return;

        reinsertionLocked = false;
        lockedBottleInsertPoint = null;

        Debug.Log("[PipetteInserterBottle] Bottle Reinsertion Allowed!");
    }

    public bool CanInsert()
    {
        if (isInserted || reinsertionLocked)
            return false;

        if (pipette == null || pipette.pipetteLiquid == null)
            return false;

        if (pipette.pipetteLiquid.HasLiquid)
        {
            Debug.Log("[PipetteInserterBottle] Pipette Is Already Filled!");
            return false;
        }

        return true;
    }

    public void InsertIntoBottle(Bottle _bottle)
    {
        if (!CanInsert() || _bottle == null || _bottle.pipetteInsertPoint == null)
            return;

        bottle = _bottle;
        canBeRemoved = false;

        if (pipette != null && pipette.pipetteStateController != null)
            pipette.pipetteStateController.SetState(PipetteState.InBottle);

        bottle.isPipetteInserted = true;
        bottle.bottleFillZone?.DisableZone();

        if (pipette != null && pipette.pipetteCollider != null && bottle.bottleCollider != null)
            Physics.IgnoreCollision(pipette.pipetteCollider, bottle.bottleCollider, true);

        ForceReleaseIfHeld();
        LockRigidBody();

        if (insertAnimationRoutine != null)
            StopCoroutine(insertAnimationRoutine);

        insertAnimationRoutine = StartCoroutine(SmoothInsertIntoBottle());
    }

    private IEnumerator SmoothInsertIntoBottle()
    {
        if (bottle == null || bottle.pipetteInsertPoint == null)
            yield break;

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        Vector3 endPosition = bottle.pipetteInsertPoint.position;
        Quaternion endRotation = bottle.pipetteInsertPoint.rotation;

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

        Debug.Log("[PipetteInserterBottle] Pipette Inserted Into Bottle!");

        yield return new WaitForSeconds(insertDuration);

        AudioManager.Instance?.PlayAudio(fillWithLiquidAudio, fillWithLiquidVolume);

        FillFromBottle();

        yield return new WaitForSeconds(insertDuration);

        canBeRemoved = true;
        LockRigidBody();

        Debug.Log("[PipetteInserterBottle] Pipette Filled And Ready To Be Removed!");
    }

    private void FillFromBottle()
    {
        if (pipette == null || pipette.pipetteLiquid == null || bottle == null || bottle.bottleLiquid == null)
            return;

        if (pipette.pipetteLiquid.HasLiquid)
            return;

        if (!bottle.bottleLiquid.RemoveOneStep())
        {
            Debug.Log("[PipetteInserterBottle] Bottle Has No Liquid!");
            return;
        }

        pipette.pipetteLiquid.FillFromBottle(bottle);
        Debug.Log("[PipetteInserterBottle] Pipette Is Getting Filled!");
    }

    private void RemoveFromBottle()
    {
        canBeRemoved = false;

        if (pipette != null && pipette.pipetteStateController != null)
            pipette.pipetteStateController.SetState(PipetteState.Free);

        if (pipette != null && pipette.rigidBody != null)
        {
            pipette.rigidBody.isKinematic = false;
            pipette.rigidBody.useGravity = false;
            pipette.rigidBody.linearVelocity = Vector3.zero;
            pipette.rigidBody.angularVelocity = Vector3.zero;
        }

        if (bottle != null && pipette != null && pipette.pipetteCollider != null && bottle.bottleCollider != null)
            Physics.IgnoreCollision(pipette.pipetteCollider, bottle.bottleCollider, false);

        if (bottle != null)
        {
            bottle.isPipetteInserted = false;
            bottle.bottleFillZone?.EnableZone();

            reinsertionLocked = true;
            lockedBottleInsertPoint = bottle.pipetteInsertPoint;
            bottle = null;
        }

        Debug.Log("[PipetteInserterBottle] Pipette Removed From Bottle!");
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