using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class StirringRod : MonoBehaviour
{
    [Header("State")]
    public StirringRodStateController stirringRodStateController;

    [Header("References")]
    public XRGrabInteractable grabInteractable;
    public Rigidbody rigidBody;
    public Collider stirringRodCollider;

    [Header("Points")]
    public Transform originalPoint;

    [Header("Inserters")]
    public StirringRodInserterBeaker stirringRodInserterBeaker;
    public StirringRodReturnOriginalPoint stirringRodReturnOriginalPoint;

    private void Awake()
    {
        if (stirringRodStateController == null)
            stirringRodStateController = GetComponent<StirringRodStateController>();

        if (grabInteractable == null)
            grabInteractable = GetComponent<XRGrabInteractable>();

        if (rigidBody == null)
            rigidBody = GetComponent<Rigidbody>();

        if (stirringRodCollider == null)
            stirringRodCollider = GetComponent<Collider>();

        if (stirringRodInserterBeaker == null)
            stirringRodInserterBeaker = GetComponent<StirringRodInserterBeaker>();

        if (stirringRodReturnOriginalPoint == null)
            stirringRodReturnOriginalPoint = GetComponent<StirringRodReturnOriginalPoint>();
    }
}