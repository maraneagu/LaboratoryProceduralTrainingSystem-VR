using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SpatulaReturnOriginalPoint : MonoBehaviour
{
    private Spatula spatula;
    private Transform spatulaParent;

    [Header("References")]
    [SerializeField] private Transform experiment;

    [Header("Animation")]
    [SerializeField] private float returnDuration = 0.35f;

    private Coroutine returnRoutine;
    private bool isReturning;

    public bool IsReturning => isReturning;

    private void Awake()
    {
        spatula = GetComponent<Spatula>();
        spatulaParent = transform.parent;
    }

    private void OnEnable()
    {
        if (spatula?.grabInteractable != null)
        {
            spatula.grabInteractable.selectExited.RemoveListener(OnReleased);
            spatula.grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    private void OnDisable()
    {
        if (spatula?.grabInteractable != null)
            spatula.grabInteractable.selectExited.RemoveListener(OnReleased);

        StopAllCoroutines();
        RestoreAfterReturn();
    }

    private void Start()
    {
        if (spatula == null || spatula.originalPoint == null || spatula.rigidBody == null)
            return;

        SnapToPoint(spatula.originalPoint);
        SetKinematicState();
        spatula.rigidBody.detectCollisions = true;

        Debug.Log("[SpatulaReturnOriginalPoint] Spatula Initialized At Original Point!");
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        Return();
    }

    private bool CanStartReturn()
    {
        if (spatula == null)
            return false;

        if (isReturning)
            return false;

        if (spatula.spatulaStateController != null && !spatula.spatulaStateController.IsFree)
            return false;

        return true;
    }

    private Transform GetReturnPoint()
    {
        if (spatula == null)
            return null;

        if (spatula.originalPoint == null)
        {
            Debug.Log("[SpatulaReturnOriginalPoint] Spatula Original Point Is Missing!");
            return null;
        }

        return spatula.originalPoint;
    }

    public void Return()
    {
        if (!CanStartReturn())
            return;

        Transform returnPoint = GetReturnPoint();
        if (returnPoint == null)
            return;

        if (returnRoutine != null)
            StopCoroutine(returnRoutine);

        returnRoutine = StartCoroutine(ReturnToPoint(returnPoint));
    }

    private IEnumerator ReturnToPoint(Transform returnPoint)
    {
        isReturning = true;

        if (spatula == null)
        {
            isReturning = false;
            yield break;
        }

        if (spatula.grabInteractable != null && spatula.grabInteractable.isSelected)
        {
            isReturning = false;
            yield break;
        }

        PrepareForReturn();

        yield return SmoothReturnToPoint(returnPoint);

        RestoreAfterReturn();
        isReturning = false;
        returnRoutine = null;
    }

    private IEnumerator SmoothReturnToPoint(Transform returnPoint)
    {
        if (spatula == null || spatula.rigidBody == null || returnPoint == null)
            yield break;

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        Vector3 endPosition = returnPoint.position;
        Quaternion endRotation = returnPoint.rotation;

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

        if (spatula.spatulaPowder != null &&
            spatula.spatulaPowder.HasPowder)
        {
            spatula.spatulaPowder.EmptySpatula();

            if (spatula.spatulaStateController != null)
                spatula.spatulaStateController.SetState(SpatulaState.Free);
        }

        yield return null;

        Debug.Log("[SpatulaReturnOriginalPoint] Spatula Returned !");
    }

    private void PrepareForReturn()
    {
        if (spatula?.rigidBody == null)
            return;

        SetKinematicState();
        spatula.rigidBody.detectCollisions = false;

        if (spatula.grabInteractable != null)
            spatula.grabInteractable.enabled = false;

        transform.SetParent(null, true);

        if (experiment != null && ExperimentCollidersManager.Instance != null)
        {
            ExperimentCollidersManager.Instance.DisableExperimentColliders(experiment, transform);
        }
    }

    private void RestoreAfterReturn()
    {
        if (spatula?.rigidBody != null)
        {
            SetKinematicState();
            spatula.rigidBody.detectCollisions = true;
        }

        if (spatula?.grabInteractable != null)
            spatula.grabInteractable.enabled = true;

        if (spatulaParent != null)
            transform.SetParent(spatulaParent, true);

        if (ExperimentCollidersManager.Instance != null)
        {
            ExperimentCollidersManager.Instance.EnableExperimentColliders();
        }
    }

    private void SetKinematicState()
    {
        if (spatula?.rigidBody == null)
            return;

        spatula.rigidBody.linearVelocity = Vector3.zero;
        spatula.rigidBody.angularVelocity = Vector3.zero;
        spatula.rigidBody.useGravity = false;
        spatula.rigidBody.isKinematic = true;
    }

    private void SnapToPoint(Transform returnPoint)
    {
        if (returnPoint == null)
            return;

        transform.position = returnPoint.position;
        transform.rotation = returnPoint.rotation;
    }
}