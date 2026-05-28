using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ReactionTubeReturnOriginalPoint : MonoBehaviour
{
    private ReactionTube reactionTube;
    private Transform reactionTubeParent;

    [Header("Animation")]
    [SerializeField] private float returnDuration = 0.35f;

    [Header("References")]
    [SerializeField] private Transform experiment;

    private Coroutine returnRoutine;

    private Collider[] reactionTubeColliders;
    private bool disableReactionTubeColliders = true;

    private bool isReturning;
    private bool returnLocked;

    public bool IsReturning => isReturning;

    private void Awake()
    {
        reactionTube = GetComponent<ReactionTube>();
        reactionTubeParent = transform.parent;
        reactionTubeColliders = GetComponentsInChildren<Collider>(true);
    }

    private void OnEnable()
    {
        if (reactionTube?.grabInteractable != null)
        {
            reactionTube.grabInteractable.selectExited.RemoveListener(OnReleased);
            reactionTube.grabInteractable.selectEntered.RemoveListener(OnGrabbed);

            reactionTube.grabInteractable.selectExited.AddListener(OnReleased);
            reactionTube.grabInteractable.selectEntered.AddListener(OnGrabbed);
        }
    }

    private void OnDisable()
    {
        if (reactionTube?.grabInteractable != null)
        {
            reactionTube.grabInteractable.selectExited.RemoveListener(OnReleased);
            reactionTube.grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        }

        StopAllCoroutines();
        RestoreAfterReturn();
    }

    private void Start()
    {
        if (reactionTube == null || reactionTube.originalPoint == null || reactionTube.rigidBody == null)
            return;

        SnapToPoint(reactionTube.originalPoint);
        SetKinematicState();
        reactionTube.rigidBody.detectCollisions = true;
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        CancelReturn();

        SetReactionTubeColliders(true);

        if (reactionTube?.rigidBody == null)
            return;

        reactionTube.rigidBody.useGravity = true;
        reactionTube.rigidBody.isKinematic = false;
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        ReturnToOriginalPoint();
    }

    public void ReturnToOriginalPoint()
    {
        if (!CanStartReturn())
            return;

        if (returnRoutine != null)
            StopCoroutine(returnRoutine);

        returnRoutine = StartCoroutine(DelayedReturn());
    }

    private bool CanStartReturn()
    {
        if (returnLocked)
            return false;

        if (isReturning)
            return false;

        if (reactionTube == null || reactionTube.originalPoint == null)
            return false;

        return true;
    }

    private IEnumerator DelayedReturn()
    {
        isReturning = true;

        yield return null;

        if (reactionTube == null)
        {
            isReturning = false;
            yield break;
        }

        if (reactionTube.grabInteractable != null &&
            reactionTube.grabInteractable.isSelected)
        {
            isReturning = false;
            yield break;
        }

        PrepareForReturn();

        yield return SmoothReturnToPoint();

        RestoreAfterReturn();
        isReturning = false;
        returnRoutine = null;
    }

    private void PrepareForReturn()
    {
        if (reactionTube?.rigidBody == null)
            return;

        SetKinematicState();
        reactionTube.rigidBody.detectCollisions = false;

        if (reactionTube.grabInteractable != null)
            reactionTube.grabInteractable.enabled = false;

        transform.SetParent(null, true);

        if (disableReactionTubeColliders)
            SetReactionTubeColliders(false);

        if (experiment != null && ExperimentCollidersManager.Instance != null)
        {
            ExperimentCollidersManager.Instance.DisableExperimentColliders(experiment, transform);
        }
    }

    private void RestoreAfterReturn()
    {
        if (reactionTube?.rigidBody != null)
        {
            SetKinematicState();
            reactionTube.rigidBody.detectCollisions = true;
        }

        if (reactionTube?.grabInteractable != null)
            reactionTube.grabInteractable.enabled = true;

        if (reactionTubeParent != null)
            transform.SetParent(reactionTubeParent, true);

        if (disableReactionTubeColliders)
            SetReactionTubeColliders(true);

        if (ExperimentCollidersManager.Instance != null)
        {
            ExperimentCollidersManager.Instance.EnableExperimentColliders();
        }
    }

    private IEnumerator SmoothReturnToPoint()
    {
        if (reactionTube == null || reactionTube.originalPoint == null)
            yield break;

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        Vector3 endPosition = reactionTube.originalPoint.position;
        Quaternion endRotation = reactionTube.originalPoint.rotation;

        float elapsed = 0f;

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float time = Mathf.Clamp01(elapsed / returnDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, time);

            transform.position = Vector3.Lerp(startPosition, endPosition, smoothT);
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, smoothT);

            yield return null;
        }

        transform.position = endPosition;
        transform.rotation = endRotation;

        yield return null;
    }

    private void CancelReturn()
    {
        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }

        isReturning = false;
    }

    private void SetKinematicState()
    {
        if (reactionTube?.rigidBody == null)
            return;

        reactionTube.rigidBody.linearVelocity = Vector3.zero;
        reactionTube.rigidBody.angularVelocity = Vector3.zero;
        reactionTube.rigidBody.useGravity = false;
        reactionTube.rigidBody.isKinematic = true;
    }

    private void SetReactionTubeColliders(bool enabled)
    {
        if (reactionTubeColliders == null)
            return;

        foreach (Collider collider in reactionTubeColliders)
        {
            if (collider != null)
                collider.enabled = enabled;
        }
    }

    private void SnapToPoint(Transform point)
    {
        if (point == null)
            return;

        transform.position = point.position;
        transform.rotation = point.rotation;
    }

    public void LockReturn()
    {
        returnLocked = true;
        CancelReturn();
    }

    public void UnlockReturn()
    {
        returnLocked = false;
    }
}