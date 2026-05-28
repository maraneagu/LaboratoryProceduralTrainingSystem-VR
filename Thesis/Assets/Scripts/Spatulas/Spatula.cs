using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Spatula : MonoBehaviour
{
    [Header("State")]
    public SpatulaStateController spatulaStateController;

    [Header("References")]
    public Rigidbody rigidBody;
    public Collider spatulaCollider;
    public XRGrabInteractable grabInteractable;

    [Header("Powder Contents")]
    public SpatulaPowder spatulaPowder;

    [Header("Points")]
    public Transform originalPoint;

    [Header("Inserters")]
    public SpatulaInserterPowder spatulaInserterPowder;
    public SpatulaInserterBeaker spatulaInserterBeaker;

    public SpatulaReturnOriginalPoint spatulaReturnOriginalPoint;

    private void Awake()
    {
        if (grabInteractable == null)
            grabInteractable = GetComponent<XRGrabInteractable>();

        if (rigidBody == null)
            rigidBody = GetComponent<Rigidbody>();

        if (spatulaCollider == null)
            spatulaCollider = GetComponent<Collider>();

        if (spatulaStateController == null)
            spatulaStateController = GetComponent<SpatulaStateController>();

        if (spatulaPowder == null)
            spatulaPowder = GetComponentInChildren<SpatulaPowder>(true);

        if (spatulaInserterPowder == null)
            spatulaInserterPowder = GetComponent<SpatulaInserterPowder>();

        if (spatulaInserterBeaker == null)
            spatulaInserterBeaker = GetComponent<SpatulaInserterBeaker>();

        if (spatulaReturnOriginalPoint == null)
            spatulaReturnOriginalPoint = GetComponent<SpatulaReturnOriginalPoint>();
    }
}