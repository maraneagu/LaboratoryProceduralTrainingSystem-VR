using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class StirringRodReturnOriginalPoint : MonoBehaviour
{
    private StirringRod stirringRod;
    private Transform stirringRodParent;

    [Header("Animation")]
    [SerializeField] private float returnDuration = 0.35f;

    [Header("References")]
    [SerializeField] private Transform experiment;

    private Coroutine returnRoutine;
    private bool isReturning;

    public bool IsReturning => isReturning;

    private void Awake()
    {
        stirringRod = GetComponent<StirringRod>();
        stirringRodParent = transform.parent;
    }

    private void OnEnable()
    {
        if (stirringRod?.grabInteractable != null)
        {
            stirringRod.grabInteractable.selectExited.RemoveListener(OnReleased);
            stirringRod.grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    private void OnDisable()
    {
        if (stirringRod?.grabInteractable != null)
            stirringRod.grabInteractable.selectExited.RemoveListener(OnReleased);

        StopAllCoroutines();
        RestoreAfterReturn();
    }

    private void Start()
    {
        if (stirringRod == null || stirringRod.originalPoint == null || stirringRod.rigidBody == null)
            return;

        SnapToPoint(stirringRod.originalPoint);
        SetKinematicState();
        stirringRod.rigidBody.detectCollisions = true;
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
        if (stirringRod == null)
            return false;

        if (isReturning)
            return false;

        if (stirringRod.stirringRodStateController != null &&
            !stirringRod.stirringRodStateController.IsFree)
            return false;

        return true;
    }

    private IEnumerator DelayedReturn()
    {
        isReturning = true;

        yield return null;

        if (stirringRod == null)
        {
            isReturning = false;
            yield break;
        }

        if (stirringRod.grabInteractable != null &&
            stirringRod.grabInteractable.isSelected)
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
        if (stirringRod?.rigidBody == null)
            return;

        SetKinematicState();
        stirringRod.rigidBody.detectCollisions = false;

        if (stirringRod.grabInteractable != null)
            stirringRod.grabInteractable.enabled = false;

        transform.SetParent(null, true);

        if (experiment != null && ExperimentCollidersManager.Instance != null)
        {
            ExperimentCollidersManager.Instance.DisableExperimentColliders(experiment, transform);
        }
    }

    private void RestoreAfterReturn()
    {
        if (stirringRod?.rigidBody != null)
        {
            SetKinematicState();
            stirringRod.rigidBody.detectCollisions = true;
        }

        if (stirringRod?.grabInteractable != null)
            stirringRod.grabInteractable.enabled = true;

        if (stirringRodParent != null)
            transform.SetParent(stirringRodParent, true);

        if (ExperimentCollidersManager.Instance != null)
        {
            ExperimentCollidersManager.Instance.EnableExperimentColliders();
        }

        if (stirringRod != null && stirringRod.stirringRodInserterBeaker != null)
            stirringRod.stirringRodInserterBeaker.EnableLockedBeakerZone();
    }

    private IEnumerator SmoothReturnToPoint()
    {
        if (stirringRod == null || stirringRod.originalPoint == null)
            yield break;

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        Vector3 endPosition = stirringRod.originalPoint.position;
        Quaternion endRotation = stirringRod.originalPoint.rotation;

        float elapsed = 0f;

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float time = Mathf.Clamp01(elapsed / returnDuration);
            float smoothStep = Mathf.SmoothStep(0f, 1f, time);

            transform.position = Vector3.Lerp(startPosition, endPosition, smoothStep);
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, smoothStep);

            yield return null;
        }

        transform.position = endPosition;
        transform.rotation = endRotation;

        yield return null;

        Debug.Log("[StirringRodReturnOriginalPoint] Stirring Rod Returned!");
    }

    private void SetKinematicState()
    {
        if (stirringRod?.rigidBody == null)
            return;

        stirringRod.rigidBody.linearVelocity = Vector3.zero;
        stirringRod.rigidBody.angularVelocity = Vector3.zero;
        stirringRod.rigidBody.useGravity = false;
        stirringRod.rigidBody.isKinematic = true;
    }

    private void SnapToPoint(Transform point)
    {
        if (point == null)
            return;

        transform.position = point.position;
        transform.rotation = point.rotation;
    }
}