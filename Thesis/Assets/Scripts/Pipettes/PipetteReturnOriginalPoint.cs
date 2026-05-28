using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PipetteReturnOriginalPoint : MonoBehaviour
{
    private Pipette pipette;
    private Transform pipetteParent;

    [Header("References")]
    [SerializeField] private Transform experiment;

    [Header("Animation")]
    [SerializeField] private float returnDuration = 0.35f;

    private Coroutine returnRoutine;
    private bool isReturning;

    public bool IsReturning => isReturning;

    private void Awake()
    {
        pipette = GetComponent<Pipette>();
        pipetteParent = transform.parent;
    }

    private void OnEnable()
    {
        if (pipette?.grabInteractable != null)
        {
            pipette.grabInteractable.selectExited.RemoveListener(OnReleased);
            pipette.grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    private void OnDisable()
    {
        if (pipette?.grabInteractable != null)
            pipette.grabInteractable.selectExited.RemoveListener(OnReleased);

        StopAllCoroutines();
        RestoreAfterReturn();
    }

    private void Start()
    {
        if (pipette == null || pipette.originalPoint == null || pipette.rigidBody == null)
            return;

        SnapToPoint(pipette.originalPoint);
        SetKinematicState();
        pipette.rigidBody.detectCollisions = true;

        Debug.Log("[PipetteReturnOriginalPoint] Pipette Initialized At Original Point!");
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        Return();
    }

    private bool CanStartReturn()
    {
        if (pipette == null)
            return false;

        if (isReturning)
            return false;

        if (pipette.pipetteStateController != null && !pipette.pipetteStateController.IsFree)
            return false;

        return true;
    }

    private Transform GetReturnPoint()
    {
        if (pipette == null)
            return null;

        if (pipette.originalPoint == null)
        {
            Debug.Log("[PipetteReturnOriginalPoint] Pipette Original Point Is Missing!");
            return null;
        }

        return pipette.originalPoint;
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

        if (pipette == null)
        {
            isReturning = false;
            yield break;
        }

        if (pipette.grabInteractable != null && pipette.grabInteractable.isSelected)
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
        if (pipette == null || pipette.rigidBody == null || returnPoint == null)
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

        if (pipette.pipetteLiquid != null &&
            pipette.pipetteLiquid.HasLiquid)
        {
            pipette.pipetteLiquid.EmptyPipette();

            if (pipette.pipetteStateController != null)
                pipette.pipetteStateController.SetState(PipetteState.Free);
        }

        yield return null;

        Debug.Log("[PipetteReturnOriginalPoint] Pipette Returned !");
    }

    private void PrepareForReturn()
    {
        if (pipette?.rigidBody == null)
            return;

        SetKinematicState();
        pipette.rigidBody.detectCollisions = false;

        if (pipette.grabInteractable != null)
            pipette.grabInteractable.enabled = false;

        transform.SetParent(null, true);

        if (experiment != null && ExperimentCollidersManager.Instance != null)
        {
            ExperimentCollidersManager.Instance.DisableExperimentColliders(experiment, transform);
        }
    }

    private void RestoreAfterReturn()
    {
        if (pipette?.rigidBody != null)
        {
            SetKinematicState();
            pipette.rigidBody.detectCollisions = true;
        }

        if (pipette?.grabInteractable != null)
            pipette.grabInteractable.enabled = true;

        if (pipetteParent != null)
            transform.SetParent(pipetteParent, true);

        if (ExperimentCollidersManager.Instance != null)
        {
            ExperimentCollidersManager.Instance.EnableExperimentColliders();
        }
    }

    private void SetKinematicState()
    {
        if (pipette?.rigidBody == null)
            return;

        pipette.rigidBody.linearVelocity = Vector3.zero;
        pipette.rigidBody.angularVelocity = Vector3.zero;
        pipette.rigidBody.useGravity = false;
        pipette.rigidBody.isKinematic = true;
    }

    private void SnapToPoint(Transform point)
    {
        if (point == null)
            return;

        transform.position = point.position;
        transform.rotation = point.rotation;
    }
}