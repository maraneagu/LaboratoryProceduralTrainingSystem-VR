using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Clipboard : MonoBehaviour
{
    [Header("References")]
    public XRGrabInteractable grabInteractable;
    public Rigidbody rigidBody;
    public Collider clipboardCollider;

    [Header("Points")]
    public Transform originalPoint;

    public ClipboardReturnOriginalPoint clipboardReturnOriginalPoint;

    private void Awake()
    {
        if (grabInteractable == null)
            grabInteractable = GetComponent<XRGrabInteractable>();

        if (rigidBody == null)
            rigidBody = GetComponent<Rigidbody>();

        if (clipboardCollider == null)
            clipboardCollider = GetComponent<Collider>();

        if (clipboardReturnOriginalPoint == null)
            clipboardReturnOriginalPoint = GetComponent<ClipboardReturnOriginalPoint>();
    }
}