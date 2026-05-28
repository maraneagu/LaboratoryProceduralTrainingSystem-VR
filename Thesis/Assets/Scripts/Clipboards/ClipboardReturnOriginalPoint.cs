using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ClipboardReturnOriginalPoint : MonoBehaviour
{
    private Clipboard clipboard;

    [Header("Animation")]
    [SerializeField] private float returnDuration = 0.35f;

    private Coroutine returnRoutine;

    private void Awake()
    {
        clipboard = GetComponent<Clipboard>();
    }

    private void OnEnable()
    {
        if (clipboard?.grabInteractable != null)
        {
            clipboard.grabInteractable.selectExited.RemoveListener(OnReleased);
            clipboard.grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    private void OnDisable()
    {
        if (clipboard?.grabInteractable != null)
            clipboard.grabInteractable.selectExited.RemoveListener(OnReleased);

        StopAllCoroutines();
    }

    private void Start()
    {
        if (clipboard == null || clipboard.originalPoint == null || clipboard.rigidBody == null)
            return;

        SnapToPoint(clipboard.originalPoint);
        SetKinematicState();
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        ReturnToOriginalPoint();
    }

    public void ReturnToOriginalPoint()
    {
        if (returnRoutine != null)
            StopCoroutine(returnRoutine);

        returnRoutine = StartCoroutine(DelayedReturn());
    }

    private IEnumerator DelayedReturn()
    {
        yield return null;

        if (clipboard == null)
        {
            yield break;
        }

        if (clipboard.grabInteractable != null &&
            clipboard.grabInteractable.isSelected)
        {
            yield break;
        }

        yield return SmoothReturnToPoint();

        returnRoutine = null;
    }

    private IEnumerator SmoothReturnToPoint()
    {
        if (clipboard == null || clipboard.originalPoint == null)
            yield break;

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        Vector3 endPosition = clipboard.originalPoint.position;
        Quaternion endRotation = clipboard.originalPoint.rotation;

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

        SetKinematicState();
    }

    private void SetKinematicState()
    {
        if (clipboard?.rigidBody == null)
            return;

        clipboard.rigidBody.linearVelocity = Vector3.zero;
        clipboard.rigidBody.angularVelocity = Vector3.zero;
        clipboard.rigidBody.useGravity = false;
        clipboard.rigidBody.isKinematic = true;
    }

    private void SnapToPoint(Transform point)
    {
        if (point == null)
            return;

        transform.position = point.position;
        transform.rotation = point.rotation;
    }
}