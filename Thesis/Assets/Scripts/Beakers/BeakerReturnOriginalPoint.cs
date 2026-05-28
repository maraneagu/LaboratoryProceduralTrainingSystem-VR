using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BeakerReturnOriginalPoint : MonoBehaviour
{
    private Beaker beaker;
    private Transform beakerParent;

    [Header("Animation")]
    [SerializeField] private float returnDuration = 0.35f;

    [Header("References")]
    [SerializeField] private Transform experiment;

    private Coroutine returnRoutine;

    private Collider[] beakerColliders;
    private bool disableBeakerColliders = true;

    private bool isReturning;
    private bool returnLocked;

    public bool IsReturning => isReturning;

    private void Awake()
    {
        beaker = GetComponent<Beaker>();
        beakerParent = transform.parent;
        beakerColliders = GetComponentsInChildren<Collider>(true);
    }

    private void OnEnable()
    {
        if (beaker?.grabInteractable != null)
        {
            beaker.grabInteractable.selectExited.RemoveListener(OnReleased);
            beaker.grabInteractable.selectEntered.RemoveListener(OnGrabbed);

            beaker.grabInteractable.selectExited.AddListener(OnReleased);
            beaker.grabInteractable.selectEntered.AddListener(OnGrabbed);
        }
    }

    private void OnDisable()
    {
        if (beaker?.grabInteractable != null)
        {
            beaker.grabInteractable.selectExited.RemoveListener(OnReleased);
            beaker.grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        }

        StopAllCoroutines();
        RestoreAfterReturn();
    }

    private void Start()
    {
        if (beaker == null || beaker.originalPoint == null || beaker.rigidBody == null)
            return;

        SnapToPoint(beaker.originalPoint);
        SetKinematicState();
        beaker.rigidBody.detectCollisions = true;
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        CancelReturn();

        SetBeakerColliders(true);

        if (beaker?.rigidBody == null)
            return;

        beaker.rigidBody.useGravity = true;
        beaker.rigidBody.isKinematic = false;
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

        if (beaker == null || beaker.originalPoint == null)
            return false;

        return true;
    }

    private IEnumerator DelayedReturn()
    {
        isReturning = true;

        yield return null;

        if (beaker == null)
        {
            isReturning = false;
            yield break;
        }

        if (beaker.grabInteractable != null && beaker.grabInteractable.isSelected)
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
        if (beaker?.rigidBody == null)
            return;

        SetKinematicState();
        beaker.rigidBody.detectCollisions = false;

        if (beaker.grabInteractable != null)
            beaker.grabInteractable.enabled = false;

        transform.SetParent(null, true);

        if (disableBeakerColliders)
            SetBeakerColliders(false);

        if (experiment != null && ExperimentCollidersManager.Instance != null)
        {
            ExperimentCollidersManager.Instance.DisableExperimentColliders(experiment, transform);
        }
    }

    private void RestoreAfterReturn()
    {
        if (beaker?.rigidBody != null)
        {
            SetKinematicState();
            beaker.rigidBody.detectCollisions = true;
        }

        if (beaker?.grabInteractable != null)
            beaker.grabInteractable.enabled = true;

        if (beakerParent != null)
            transform.SetParent(beakerParent, true);

        if (disableBeakerColliders)
            SetBeakerColliders(true);

        if (ExperimentCollidersManager.Instance != null)
        {
            ExperimentCollidersManager.Instance.EnableExperimentColliders();
        }
    }

    private IEnumerator SmoothReturnToPoint()
    {
        if (beaker == null || beaker.originalPoint == null)
            yield break;

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        Vector3 endPosition = beaker.originalPoint.position;
        Quaternion endRotation = beaker.originalPoint.rotation;

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

        Debug.Log("[BeakerReturnOriginalPoint] Beaker Returned!");
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
        if (beaker?.rigidBody == null)
            return;

        beaker.rigidBody.linearVelocity = Vector3.zero;
        beaker.rigidBody.angularVelocity = Vector3.zero;
        beaker.rigidBody.useGravity = false;
        beaker.rigidBody.isKinematic = true;
    }

    private void SetBeakerColliders(bool enabled)
    {
        if (beakerColliders == null)
            return;

        foreach (Collider collider in beakerColliders)
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