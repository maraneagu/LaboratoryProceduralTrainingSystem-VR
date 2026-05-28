using System.Collections;
using UnityEngine;

public class SpatulaInserterPowder : MonoBehaviour
{
    private Spatula spatula;
    private Powder powder;

    public bool isInserted => 
        spatula != null &&
        spatula.spatulaStateController != null &&
        spatula.spatulaStateController.IsInPowder;

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
    [SerializeField] private AudioClip scoopPowderAudio;
    [SerializeField] private float scoopPowderVolume = 1f;

    private bool canBeRemoved = false;
    private bool reinsertionLocked = false;
    private Transform lockedPowderInsertPoint = null;

    private void Awake()
    {
        spatula = GetComponent<Spatula>();
    }

    private void Update()
    {
        HandleReinsertionUnlock();

        if (!isInserted || powder == null || powder.spatulaInsertPoint == null)
            return;

        if (spatula == null || spatula.grabInteractable == null || !spatula.grabInteractable.isSelected)
        {
            transform.position = powder.spatulaInsertPoint.position;
            transform.rotation = powder.spatulaInsertPoint.rotation;
            return;
        }

        if (!canBeRemoved)
            return;

        float distance = Vector3.Distance(transform.position, powder.spatulaInsertPoint.position);
        if (distance > removalDistance)
            RemoveFromPowder();
    }

    private void HandleReinsertionUnlock()
    {
        if (!reinsertionLocked || lockedPowderInsertPoint == null)
            return;

        float unlockDistance = Vector3.Distance(transform.position, lockedPowderInsertPoint.position);

        if (unlockDistance <= reinsertionUnlockDistance)
            return;

        reinsertionLocked = false;
        lockedPowderInsertPoint = null;

        Debug.Log("[SpatulaInserterPowder] Powder Reinsertion Allowed!");
    }

    public bool CanInsert()
    {
        if (isInserted || reinsertionLocked)
            return false;

        if (spatula == null || spatula.spatulaStateController == null || spatula.spatulaPowder == null)
            return false;

        if (spatula.spatulaPowder.HasPowder)
        {
            Debug.Log("[SpatulaInserterPowder] Spatula Is Already Filled!");
            return false;
        }

        return true;
    }

    public void InsertIntoPowder(Powder _powder)
    {
        if (!CanInsert() || _powder == null || _powder.spatulaInsertPoint == null)
            return;

        powder = _powder;
        canBeRemoved = false;

        if (spatula != null && spatula.spatulaStateController != null)
            spatula.spatulaStateController.SetState(SpatulaState.InPowder);

        powder.isSpatulaInserted = true;
        powder.powderFillZone?.DisableZone();

        ForceReleaseIfHeld();
        LockRigidBody();

        if (insertAnimationRoutine != null)
            StopCoroutine(insertAnimationRoutine);

        insertAnimationRoutine = StartCoroutine(SmoothInsertIntoPowder());
    }

    private IEnumerator SmoothInsertIntoPowder()
    {
        if (powder == null || powder.spatulaInsertPoint == null)
            yield break;

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        Vector3 endPosition = powder.spatulaInsertPoint.position;
        Quaternion endRotation = powder.spatulaInsertPoint.rotation;

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

        Debug.Log("[SpatulaInserterPowder] Spatula Inserted Into Powder!");

        yield return new WaitForSeconds(insertDuration);

        AudioManager.Instance?.PlayAudio(scoopPowderAudio, scoopPowderVolume);

        FillSpatulaFromPowder();

        canBeRemoved = true;
        LockRigidBody();

        Debug.Log("[SpatulaInserterPowder] Spatula Can Be Picked Up Again!");
    }

    private void FillSpatulaFromPowder()
    {
        if (powder == null)
            return;

        if (spatula == null || spatula.spatulaPowder == null || spatula.spatulaStateController == null)
            return;

        if (spatula.spatulaPowder.HasPowder)
            return;

        spatula.spatulaPowder.FillFromPowder(powder);
        Debug.Log("[SpatulaInserterPowder] Spatula Is Getting Filled!");
    }

    private void RemoveFromPowder()
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

        if (powder != null)
        {
            powder.isSpatulaInserted = false;
            powder.powderFillZone?.EnableZone();

            reinsertionLocked = true;
            lockedPowderInsertPoint = powder.spatulaInsertPoint;
            powder = null;
        }

        Debug.Log("[SpatulaInserterPowder] Spatula Removed From Powder!");
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