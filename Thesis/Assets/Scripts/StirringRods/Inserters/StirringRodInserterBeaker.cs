using System.Collections;
using UnityEngine;

public class StirringRodInserterBeaker : MonoBehaviour
{
    private StirringRod stirringRod;
    private Beaker beaker;

    public bool isInserted =>
        stirringRod != null &&
        stirringRod.stirringRodStateController != null &&
        stirringRod.stirringRodStateController.IsInBeaker;

    [Header("Insertion Animation")]
    [SerializeField] private float insertAnimationDuration = 0.25f;
    private Coroutine insertRoutine;

    [Header("Stirring Duration")]
    [SerializeField] private float stirDuration = 2f;

    [Header("Swirling Settings")]
    [SerializeField] private float swirlRadius = 0.02f;
    [SerializeField] private float swirlSpeed = 360f;

    [Header("Removal Distance")]
    [SerializeField] private float removalDistance = 0.05f;

    [Header("Reinsertion Unlock Distance")]
    [SerializeField] private float reinsertionUnlockDistance = 0.08f;

    [Header("Audio")]
    [SerializeField] private AudioClip stirLiquidAudio;

    private bool canBeRemoved = false;
    private bool reinsertionLocked = false;

    private BeakerFillZone lockedBeakerFillZone = null;
    private Transform lockedInsertPoint = null;
    private Transform currentLockedPoint = null;

    private void Awake()
    {
        stirringRod = GetComponent<StirringRod>();
    }

    private void Update()
    {
        HandleReinsertionUnlock();

        if (!isInserted || beaker == null || currentLockedPoint == null)
            return;

        if (stirringRod == null || stirringRod.grabInteractable == null || !stirringRod.grabInteractable.isSelected)
        {
            transform.position = currentLockedPoint.position;
            transform.rotation = currentLockedPoint.rotation;
            return;
        }

        if (!canBeRemoved)
            return;

        float distance = Vector3.Distance(transform.position, currentLockedPoint.position);
        if (distance > removalDistance)
            RemoveFromBeaker();
    }

    private void HandleReinsertionUnlock()
    {
        if (!reinsertionLocked || lockedInsertPoint == null)
            return;

        float unlockDistance = Vector3.Distance(transform.position, lockedInsertPoint.position);

        if (unlockDistance <= reinsertionUnlockDistance)
            return;

        reinsertionLocked = false;
        lockedInsertPoint = null;

        Debug.Log("[StirringRodInserterBeaker] Beaker Reinsertion Allowed!");
    }

    public bool CanInsert(Beaker targetBeaker)
    {
        if (stirringRod == null || stirringRod.stirringRodStateController == null)
            return false;

        if (!stirringRod.stirringRodStateController.IsFree)
            return false;

        if (isInserted || reinsertionLocked)
            return false;

        if (targetBeaker == null || targetBeaker.beakerLiquid == null)
            return false;

        if (targetBeaker.beakerLiquid.IsEmpty())
        {
            Debug.Log("[StirringRodInserterBeaker] Beaker Has No Liquid To Stir!");
            return false;
        }

        return true;
    }

    public void InsertIntoBeaker(Beaker targetBeaker)
    {
        if (!CanInsert(targetBeaker) || targetBeaker == null || targetBeaker.stirringRodInsertPoint == null)
            return;

        beaker = targetBeaker;
        currentLockedPoint = targetBeaker.stirringRodInsertPoint;
        canBeRemoved = false;

        if (stirringRod != null && stirringRod.stirringRodStateController != null)
            stirringRod.stirringRodStateController.SetState(StirringRodState.InBeaker);

        beaker.beakerFillZone?.DisableZone();

        ForceReleaseIfHeld();
        LockRigidBody();

        if (insertRoutine != null)
            StopCoroutine(insertRoutine);

        insertRoutine = StartCoroutine(StirIntoBeaker());
    }

    private IEnumerator StirIntoBeaker()
    {
        if (beaker == null || beaker.stirringRodInsertPoint == null)
            yield break;

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        Vector3 endPosition = beaker.stirringRodInsertPoint.position;
        Quaternion endRotation = beaker.stirringRodInsertPoint.rotation;

        float elapsed = 0f;

        while (elapsed < insertAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / insertAnimationDuration);
            t = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);

            yield return null;
        }

        transform.position = endPosition;
        transform.rotation = endRotation;

        Debug.Log("[StirringRodInserterBeaker] Stirring Rod Inserted Into Beaker!");

        AudioManager.Instance?.PlayAudio(stirLiquidAudio);

        float timer = 0f;
        while (timer < stirDuration)
        {
            timer += Time.deltaTime;
            float angle = timer * swirlSpeed * Mathf.Deg2Rad;

            Vector3 localOffset = new Vector3(
                Mathf.Cos(angle) * swirlRadius,
                0f,
                Mathf.Sin(angle) * swirlRadius
            );

            transform.position = beaker.stirringRodInsertPoint.position +
                                 beaker.stirringRodInsertPoint.TransformDirection(localOffset);

            transform.rotation = beaker.stirringRodInsertPoint.rotation *
                                 Quaternion.Euler(0f, timer * swirlSpeed, 0f);

            yield return null;
        }

        transform.position = beaker.stirringRodInsertPoint.position;
        transform.rotation = beaker.stirringRodInsertPoint.rotation;

        if (beaker != null && beaker.beakerLiquid != null)
            beaker.beakerLiquid.ApplyMixing();

        RemoveFromBeaker();

        if (stirringRod != null && stirringRod.stirringRodReturnOriginalPoint != null)
            stirringRod.stirringRodReturnOriginalPoint.ReturnToOriginalPoint();
    }

    private void RemoveFromBeaker()
    {
        canBeRemoved = false;

        if (stirringRod != null && stirringRod.stirringRodStateController != null)
            stirringRod.stirringRodStateController.SetState(StirringRodState.Free);

        if (stirringRod != null && stirringRod.rigidBody != null)
        {
            stirringRod.rigidBody.isKinematic = false;
            stirringRod.rigidBody.useGravity = false;
            stirringRod.rigidBody.linearVelocity = Vector3.zero;
            stirringRod.rigidBody.angularVelocity = Vector3.zero;
        }

        if (beaker != null)
        {
            reinsertionLocked = true;
            lockedBeakerFillZone = beaker.beakerFillZone;
            lockedInsertPoint = beaker.stirringRodInsertPoint;
        }

        currentLockedPoint = null;
        beaker = null;

        Debug.Log("[StirringRodInserterBeaker] Stirring Rod Removed From Beaker!");
    }

    public void EnableLockedBeakerZone()
    {
        if (lockedBeakerFillZone == null)
            return;

        lockedBeakerFillZone.EnableZone();
        lockedBeakerFillZone = null;
    }

    private void ForceReleaseIfHeld()
    {
        if (stirringRod == null || stirringRod.grabInteractable == null || !stirringRod.grabInteractable.isSelected)
            return;

        var interactor = stirringRod.grabInteractable.firstInteractorSelecting;

        if (interactor != null && stirringRod.grabInteractable.interactionManager != null)
            stirringRod.grabInteractable.interactionManager.SelectExit(interactor, stirringRod.grabInteractable);
    }

    private void LockRigidBody()
    {
        if (stirringRod == null || stirringRod.rigidBody == null)
            return;

        stirringRod.rigidBody.linearVelocity = Vector3.zero;
        stirringRod.rigidBody.angularVelocity = Vector3.zero;
        stirringRod.rigidBody.isKinematic = true;
        stirringRod.rigidBody.useGravity = false;
    }
}