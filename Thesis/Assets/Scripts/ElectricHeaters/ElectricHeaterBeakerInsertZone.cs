using System.Collections;
using UnityEngine;

public class ElectricHeaterBeakerInsertZone : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float insertAnimationDuration = 0.3f;

    [Header("Removal Distance")]
    [SerializeField] private float removalDistance = 0.08f;

    [Header("Outline")]
    [SerializeField] private Behaviour outline;

    private ReactionTube lockedReactionTube;
    private Transform lockedBeakerInsertPoint;
    private Coroutine insertRoutine;
    private bool isReactionTubeInserted = false;

    public bool IsReactionTubeInserted => isReactionTubeInserted;
    public ReactionTube LockedReactionTube => lockedReactionTube;

    private void Awake()
    {
        if (outline != null)
            outline.enabled = false;
    }

    private void Update()
    {
        if (!isReactionTubeInserted || lockedReactionTube == null || lockedBeakerInsertPoint == null)
            return;

        Transform tubeTransform = lockedReactionTube.transform;

        float distance = Vector3.Distance(tubeTransform.position, lockedBeakerInsertPoint.position);

        if (distance > removalDistance)
        {
            ReactionTubeReturnOriginalPoint returnScript = lockedReactionTube.GetComponent<ReactionTubeReturnOriginalPoint>();
            if (returnScript != null)
                returnScript.UnlockReturn();

            isReactionTubeInserted = false;
            lockedReactionTube = null;
            lockedBeakerInsertPoint = null;

            if (outline != null)
                outline.enabled = false;

            return;
        }

        tubeTransform.position = lockedBeakerInsertPoint.position;
        tubeTransform.rotation = lockedBeakerInsertPoint.rotation;
    }

    private void OnTriggerEnter(Collider reactionTubeCollider)
    {
        if (isReactionTubeInserted)
            return;

        ReactionTube reactionTube = reactionTubeCollider.GetComponent<ReactionTube>()
            ?? reactionTubeCollider.GetComponentInParent<ReactionTube>();

        if (reactionTube == null || reactionTube.beakerInsertPoint == null)
            return;

        if (reactionTube.reactionTubeLiquid == null || reactionTube.reactionTubeLiquid.IsEmpty())
            return;

        if (outline != null)
            outline.enabled = true;

        ReactionTubeReturnOriginalPoint returnScript = reactionTube.GetComponent<ReactionTubeReturnOriginalPoint>();
        if (returnScript != null)
            returnScript.LockReturn();

        UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable =
            reactionTube.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        if (grabInteractable != null && grabInteractable.isSelected)
        {
            var interactor = grabInteractable.firstInteractorSelecting;
            if (interactor != null && grabInteractable.interactionManager != null)
                grabInteractable.interactionManager.SelectExit(interactor, grabInteractable);
        }

        Rigidbody rigidBody = reactionTube.GetComponent<Rigidbody>();
        if (rigidBody != null)
        {
            rigidBody.linearVelocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;
        }

        if (insertRoutine != null)
            StopCoroutine(insertRoutine);

        insertRoutine = StartCoroutine(AnimateInsertion(reactionTube));
    }

    private void OnTriggerExit(Collider reactionTubeCollider)
    {
        if (isReactionTubeInserted)
            return;

        ReactionTube reactionTube = reactionTubeCollider.GetComponent<ReactionTube>()
            ?? reactionTubeCollider.GetComponentInParent<ReactionTube>();

        if (reactionTube == null)
            return;

        if (outline != null)
            outline.enabled = false;
    }

    private IEnumerator AnimateInsertion(ReactionTube reactionTube)
    {
        Transform reactionTubeTransform = reactionTube.transform;
        Transform beakerInsertPoint = reactionTube.beakerInsertPoint;

        Vector3 startPosition = reactionTubeTransform.position;
        Quaternion startRotation = reactionTubeTransform.rotation;

        Vector3 endPosition = beakerInsertPoint.position;
        Quaternion endRotation = beakerInsertPoint.rotation;

        float elapsed = 0f;

        while (elapsed < insertAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / insertAnimationDuration);

            reactionTubeTransform.position = Vector3.Lerp(startPosition, endPosition, t);
            reactionTubeTransform.rotation = Quaternion.Slerp(startRotation, endRotation, t);

            yield return null;
        }

        reactionTubeTransform.position = endPosition;
        reactionTubeTransform.rotation = endRotation;

        if (outline != null)
            outline.enabled = false;

        lockedReactionTube = reactionTube;
        lockedBeakerInsertPoint = beakerInsertPoint;
        isReactionTubeInserted = true;
        insertRoutine = null;
    }
}