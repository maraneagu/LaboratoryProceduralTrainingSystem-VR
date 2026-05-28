using System.Collections;
using UnityEngine;

public class SpatulaInserterBeaker : MonoBehaviour
{
    private Spatula spatula;
    private Beaker beaker;

    public bool isInserted =>
        spatula != null &&
        spatula.spatulaStateController != null &&
        spatula.spatulaStateController.IsInBeaker;

    [Header("Insert Animation")]
    public float insertAnimationDuration = 0.25f;
    private Coroutine insertAnimationRoutine;

    [Header("Insert Duration")]
    public float insertDuration = 0.75f;

    [Header("Removal Distance")]
    public float removalDistance = 0.05f;

    [Header("Reinsertion Unlock Distance")]
    public float reinsertionUnlockDistance = 0.08f;

    [Header("Audio")]
    [SerializeField] private AudioClip dropPowderAudio;
    [SerializeField] private float dropPowderVolume = 1f;

    private bool canBeRemoved = false;
    private bool reinsertionLocked = false;
    private Transform lockedBeakerInsertPoint = null;

    private void Awake()
    {
        spatula = GetComponent<Spatula>();
    }

    private void Update()
    {
        HandleReinsertionUnlock();

        if (!isInserted || beaker == null || beaker.spatulaInsertPoint == null)
            return;

        if (spatula == null || spatula.grabInteractable == null || !spatula.grabInteractable.isSelected)
        {
            transform.position = beaker.spatulaInsertPoint.position;
            transform.rotation = beaker.spatulaInsertPoint.rotation;
            return;
        }

        if (!canBeRemoved)
            return;

        float distance = Vector3.Distance(transform.position, beaker.spatulaInsertPoint.position);
        if (distance > removalDistance)
            RemoveFromBeaker();
    }

    private void HandleReinsertionUnlock()
    {
        if (!reinsertionLocked || lockedBeakerInsertPoint == null)
            return;

        float unlockDistance = Vector3.Distance(transform.position, lockedBeakerInsertPoint.position);

        if (unlockDistance <= reinsertionUnlockDistance)
            return;

        reinsertionLocked = false;
        lockedBeakerInsertPoint = null;

        Debug.Log("[SpatulaInserterBeaker] Beaker Reinsertion Allowed!");
    }

    public bool CanInsert()
    {
        if (isInserted || reinsertionLocked)
            return false;

        if (spatula == null || spatula.spatulaStateController == null || spatula.spatulaPowder == null)
            return false;

        if (!spatula.spatulaPowder.HasPowder)
        {
            Debug.Log("[SpatulaInserterBeaker] Spatula Has No Powder!");
            return false;
        }

        return true;
    }

    public void InsertIntoBeaker(Beaker _beaker)
    {
        if (!CanInsert() || _beaker == null || _beaker.spatulaInsertPoint == null)
            return;

        beaker = _beaker;
        canBeRemoved = false;

        if (spatula != null && spatula.spatulaStateController != null)
            spatula.spatulaStateController.SetState(SpatulaState.InBeaker);

        beaker.isSpatulaInserted = true;
        beaker.beakerFillZone?.DisableZone();

        ForceReleaseIfHeld();
        LockRigidBody();

        if (insertAnimationRoutine != null)
            StopCoroutine(insertAnimationRoutine);

        insertAnimationRoutine = StartCoroutine(SmoothInsertIntoBeaker());
    }

    private IEnumerator SmoothInsertIntoBeaker()
    {
        if (beaker == null || beaker.spatulaInsertPoint == null)
            yield break;

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        Vector3 endPosition = beaker.spatulaInsertPoint.position;
        Quaternion endRotation = beaker.spatulaInsertPoint.rotation;

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

        Debug.Log("[SpatulaInserterBeaker] Spatula Inserted Into Beaker!");

        yield return new WaitForSeconds(insertDuration);

        AudioManager.Instance?.PlayAudio(dropPowderAudio, dropPowderVolume);

        EmptySpatulaIntoBeaker();

        yield return new WaitForSeconds(insertDuration);

        RemoveFromBeaker();

        if (spatula != null && spatula.spatulaReturnOriginalPoint != null)
        {
            spatula.spatulaReturnOriginalPoint.Return();
        }

        Debug.Log("[SpatulaInserterBeaker] Spatula Can Be Picked Up Again!");
    }

    private void EmptySpatulaIntoBeaker()
    {
        if (spatula == null || spatula.spatulaPowder == null || !spatula.spatulaPowder.HasPowder)
            return;

        if (beaker == null || beaker.beakerLiquid == null)
        {
            Debug.Log("[SpatulaInserterBeaker] BeakerLiquid Is Missing!");
            return;
        }

        Powder powder = spatula.spatulaPowder.powder;

        if (powder == null)
        {
            Debug.Log("[SpatulaInserterBeaker] Spatula Has No Powder Assigned!");
            return;
        }

        bool accepted = beaker.beakerLiquid.AddPowder(powder);

        if (!accepted)
        {
            Debug.LogWarning("[SpatulaInserterBeaker] Beaker Rejected Powder!");
            return;
        }

        spatula.spatulaPowder.EmptySpatula();

        Debug.Log("[SpatulaInserterBeaker] Spatula Emptied Powder Into Beaker!");
    }

    private void RemoveFromBeaker()
    {
        canBeRemoved = false;

        if (spatula != null && spatula.spatulaStateController != null)
            spatula.spatulaStateController.SetState(SpatulaState.Free);

        if (spatula != null && spatula.rigidBody != null)
        {
            spatula.rigidBody.isKinematic = false;
            spatula.rigidBody.useGravity = false;
            spatula.rigidBody.linearVelocity = Vector3.zero;
            spatula.rigidBody.angularVelocity = Vector3.zero;
        }

        if (beaker != null)
        {
            beaker.isSpatulaInserted = false;
            beaker.beakerFillZone?.EnableZone();

            reinsertionLocked = true;
            lockedBeakerInsertPoint = beaker.spatulaInsertPoint;
            beaker = null;
        }

        Debug.Log("[SpatulaInserterBeaker] Spatula Removed From Beaker!");
    }

    private void ForceReleaseIfHeld()
    {
        if (spatula == null || spatula.grabInteractable == null || !spatula.grabInteractable.isSelected)
            return;

        var interactor = spatula.grabInteractable.firstInteractorSelecting;
        if (interactor != null && spatula.grabInteractable.interactionManager != null)
            spatula.grabInteractable.interactionManager.SelectExit(interactor, spatula.grabInteractable);
    }

    private void LockRigidBody()
    {
        if (spatula == null || spatula.rigidBody == null)
            return;

        spatula.rigidBody.linearVelocity = Vector3.zero;
        spatula.rigidBody.angularVelocity = Vector3.zero;
        spatula.rigidBody.isKinematic = true;
        spatula.rigidBody.useGravity = false;
    }
}