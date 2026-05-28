using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ClipboardStandFillZone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform clipboardInsertPoint;
    [SerializeField] private ExperimentSetupSwitcher experimentSetupSwitcher;

    [Header("Insertion Animation")]
    [SerializeField] private float insertAnimationDuration = 0.25f;

    private Clipboard clipboard;
    private Coroutine insertRoutine;

    private void OnTriggerEnter(Collider clipboardCollider)
    {
        Clipboard clipboard = clipboardCollider.GetComponentInParent<Clipboard>();

        if (clipboard == null)
            return;

        InsertClipboard(clipboard);
    }

    private void InsertClipboard(Clipboard _clipboard)
    {
        clipboard = _clipboard;

        DisableReturnToOriginalPoint();
        ForceReleaseIfHeld();

        if (insertRoutine != null)
            StopCoroutine(insertRoutine);

        insertRoutine = StartCoroutine(SmoothInsertClipboard());
    }

    private IEnumerator SmoothInsertClipboard()
    {
        Vector3 startPosition = clipboard.transform.position;
        Quaternion startRotation = clipboard.transform.rotation;

        Vector3 endPosition = clipboardInsertPoint.position;
        Quaternion endRotation = clipboardInsertPoint.rotation;

        float elapsed = 0f;

        while (elapsed < insertAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float time = Mathf.Clamp01(elapsed / insertAnimationDuration);
            time = time * time * (3f - 2f * time);

            clipboard.transform.position = Vector3.Lerp(startPosition, endPosition, time);
            clipboard.transform.rotation = Quaternion.Slerp(startRotation, endRotation, time);

            yield return null;
        }

        clipboard.transform.position = endPosition;
        clipboard.transform.rotation = endRotation;
        clipboard.transform.SetParent(clipboardInsertPoint);

        experimentSetupSwitcher?.StartExperiment();
    }

    private void DisableReturnToOriginalPoint()
    {
        if (clipboard == null || clipboard.clipboardReturnOriginalPoint == null)
            return;

        clipboard.clipboardReturnOriginalPoint.enabled = false;
    }

    private void ForceReleaseIfHeld()
    {
        if (clipboard == null || clipboard.grabInteractable == null || !clipboard.grabInteractable.isSelected)
            return;

        IXRSelectInteractor interactor = clipboard.grabInteractable.firstInteractorSelecting;

        if (interactor != null && clipboard.grabInteractable.interactionManager != null)
            clipboard.grabInteractable.interactionManager.SelectExit(interactor, clipboard.grabInteractable);
    }
}