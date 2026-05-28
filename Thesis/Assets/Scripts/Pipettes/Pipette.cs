using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Pipette : MonoBehaviour
{
    [Header("State")]
    public PipetteStateController pipetteStateController;

    [Header("References")]
    public Rigidbody rigidBody;
    public Collider pipetteCollider;
    public XRGrabInteractable grabInteractable;

    [Header("Liquid Contents")]
    public PipetteLiquid pipetteLiquid;

    [Header("Points")]
    public Transform originalPoint;

    [Header("Inserters")]
    public PipetteInserterBottle pipetteInserterBottle;
    public PipetteInserterReactionTube pipetteInserterReactionTube;
    public PipetteDispenserBeaker pipetteDispenserBeaker;
    public PipetteAspiratorBeaker pipetteAspiratorBeaker;

    public PipetteReturnOriginalPoint pipetteReturnOriginalPoint;

    private void Awake()
    {
        if (rigidBody == null)
            rigidBody = GetComponent<Rigidbody>();

        if (pipetteCollider == null)
            pipetteCollider = GetComponent<Collider>();

        if (grabInteractable == null)
            grabInteractable = GetComponent<XRGrabInteractable>();

        if (pipetteLiquid == null)
            pipetteLiquid = GetComponentInChildren<PipetteLiquid>();

        if (pipetteStateController == null)
            pipetteStateController = GetComponent<PipetteStateController>();

        if (pipetteInserterBottle == null)
            pipetteInserterBottle = GetComponent<PipetteInserterBottle>();

        if (pipetteInserterReactionTube == null)
            pipetteInserterReactionTube = GetComponent<PipetteInserterReactionTube>();

        if (pipetteDispenserBeaker == null)
            pipetteDispenserBeaker = GetComponent<PipetteDispenserBeaker>();

        if (pipetteAspiratorBeaker == null)
            pipetteAspiratorBeaker = GetComponent<PipetteAspiratorBeaker>();

        if (pipetteReturnOriginalPoint == null)
            pipetteReturnOriginalPoint = GetComponent<PipetteReturnOriginalPoint>();
    }
}