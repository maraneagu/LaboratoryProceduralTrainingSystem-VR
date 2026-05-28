using System.Collections;
using UnityEngine;

public class PipetteAspiratorBeaker : MonoBehaviour
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

    [Header("Aspirating Duration")]
    public float aspirateDuration = 0.75f;

    [Header("Removal Distance")]
    public float removalDistance = 0.05f;

    [Header("Reinsertion Unlock Distance")]
    public float reinsertionUnlockDistance = 0.08f;

    [Header("Audio")]
    [SerializeField] private AudioClip fillWithLiquidAudio;
    [SerializeField] private float fillWithLiquidVolume = 0.15f;

    private bool canBeRemoved = false;
    private bool reinsertionLocked = false;
    private Transform lockedBeakerAspiratorPoint = null;

    private void Awake()
    {
        pipette = GetComponent<Pipette>();
    }

    private void Update()
    {
        HandleReinsertionUnlock();

        if (!isInserted || beaker == null || beaker.pipetteAspiratorPoint == null)
            return;

        if (pipette == null || pipette.grabInteractable == null || !pipette.grabInteractable.isSelected)
        {
            transform.position = beaker.pipetteAspiratorPoint.position;
            transform.rotation = beaker.pipetteAspiratorPoint.rotation;
            return;
        }

        if (!canBeRemoved)
            return;

        float distance = Vector3.Distance(transform.position, beaker.pipetteAspiratorPoint.position);

        if (distance > removalDistance)
            RemoveFromBeaker();
    }

    private void HandleReinsertionUnlock()
    {
        if (!reinsertionLocked || lockedBeakerAspiratorPoint == null)
            return;

        float unlockDistance = Vector3.Distance(transform.position, lockedBeakerAspiratorPoint.position);

        if (unlockDistance <= reinsertionUnlockDistance)
            return;

        reinsertionLocked = false;
        lockedBeakerAspiratorPoint = null;

        Debug.Log("[PipetteAspiratorBeaker] Beaker Reinsertion Allowed!");
    }

    public bool CanInsert()
    {
        if (isInserted || reinsertionLocked)
            return false;

        if (pipette == null || pipette.pipetteLiquid == null)
            return false;

        if (pipette.pipetteLiquid.HasLiquid)
        {
            Debug.Log("[PipetteAspiratorBeaker] Pipette Is Already Filled!");
            return false;
        }

        return true;
    }

    public void InsertIntoBeaker(Beaker _beaker)
    {
        if (!CanInsert() || _beaker == null || _beaker.pipetteAspiratorPoint == null)
            return;

        if (_beaker.beakerLiquid == null || !_beaker.beakerLiquid.HasLiquid)
        {
            Debug.Log("[PipetteAspiratorBeaker] Beaker Has No Liquid!");
            return;
        }

        beaker = _beaker;
        canBeRemoved = false;

        pipette.pipetteStateController?.SetState(PipetteState.InBeaker);

        beaker.isPipetteInserted = true;
        beaker.beakerFillZone?.DisableZone();

        if (pipette.pipetteCollider != null && beaker.beakerCollider != null)
            Physics.IgnoreCollision(pipette.pipetteCollider, beaker.beakerCollider, true);

        ForceReleaseIfHeld();
        LockRigidBody();

        if (insertAnimationRoutine != null)
            StopCoroutine(insertAnimationRoutine);

        insertAnimationRoutine = StartCoroutine(SmoothInsert());
    }

    private IEnumerator SmoothInsert()
    {
        if (beaker == null || beaker.pipetteAspiratorPoint == null)
            yield break;

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        Vector3 endPosition = beaker.pipetteAspiratorPoint.position;
        Quaternion endRotation = beaker.pipetteAspiratorPoint.rotation;

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

        Debug.Log("[PipetteAspiratorBeaker] Pipette Inserted Into Beaker!");

        yield return new WaitForSeconds(aspirateDuration);

        AudioManager.Instance?.PlayAudio(fillWithLiquidAudio, fillWithLiquidVolume);

        FillFromBeaker();

        yield return new WaitForSeconds(aspirateDuration);

        canBeRemoved = true;
        LockRigidBody();

        Debug.Log("[PipetteAspiratorBeaker] Pipette Can Be Picked Up Again!");
    }

    private void FillFromBeaker()
    {
        if (pipette?.pipetteLiquid == null || beaker?.beakerLiquid == null)
            return;

        if (pipette.pipetteLiquid.HasLiquid)
            return;

        if (!beaker.beakerLiquid.RemoveFromBeaker())
        {
            Debug.Log("[PipetteAspiratorBeaker] Beaker Has No Liquid!");
            return;
        }

        pipette.pipetteLiquid.FillFromBeaker(beaker);
        Debug.Log("[PipetteAspiratorBeaker] Pipette Is Getting Filled!");
    }

    private void RemoveFromBeaker()
    {
        canBeRemoved = false;

        if (pipette != null && pipette.pipetteStateController != null)
            pipette.pipetteStateController.SetState(PipetteState.Free);

        if (pipette.rigidBody != null)
        {
            pipette.rigidBody.isKinematic = false;
            pipette.rigidBody.useGravity = false;
            pipette.rigidBody.linearVelocity = Vector3.zero;
            pipette.rigidBody.angularVelocity = Vector3.zero;
        }

        if (beaker != null && pipette != null && pipette.pipetteCollider != null && beaker?.beakerCollider != null)
            Physics.IgnoreCollision(pipette.pipetteCollider, beaker.beakerCollider, false);

        if (beaker != null)
        {
            beaker.isPipetteInserted = false;
            beaker.beakerFillZone?.EnableZone();

            reinsertionLocked = true;
            lockedBeakerAspiratorPoint = beaker.pipetteAspiratorPoint;
            beaker = null;
        }

        Debug.Log("[PipetteAspiratorBeaker] Pipette Removed From Beaker!");
    }

    private void ForceReleaseIfHeld()
    {
        if (pipette.grabInteractable == null || !pipette.grabInteractable.isSelected)
            return;

        var interactor = pipette.grabInteractable.firstInteractorSelecting;

        if (interactor != null && pipette.grabInteractable.interactionManager != null)
            pipette.grabInteractable.interactionManager.SelectExit(interactor, pipette.grabInteractable);
    }

    private void LockRigidBody()
    {
        if (pipette.rigidBody == null)
            return;

        pipette.rigidBody.linearVelocity = Vector3.zero;
        pipette.rigidBody.angularVelocity = Vector3.zero;
        pipette.rigidBody.isKinematic = true;
        pipette.rigidBody.useGravity = false;
    }
}