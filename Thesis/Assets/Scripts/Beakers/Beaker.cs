using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Beaker : MonoBehaviour
{
    [Header("State")]
    [HideInInspector] public bool isPipetteInserted = false;
    [HideInInspector] public bool isSpatulaInserted = false;
    [HideInInspector] public bool isBeakerInserted = false;

    [Header("References")]
    public Rigidbody rigidBody;
    public XRGrabInteractable grabInteractable;
    public Collider beakerCollider;
    public BeakerFillZone beakerFillZone;
    public Collider beakerFillZoneCollider;

    [Header("Points")]
    public Transform originalPoint;
    public Transform pipetteAspiratorPoint;
    public Transform pipetteDispenserPoint;
    public Transform spatulaInsertPoint;
    public Transform stirringRodInsertPoint;

    [Header("Liquid Contents")]
    public BeakerLiquid beakerLiquid;

    public BeakerReturnOriginalPoint beakerReturnOriginalPoint;

    private void Awake()
    {
        if (rigidBody == null)
            rigidBody = GetComponent<Rigidbody>();

        if (grabInteractable == null)
            grabInteractable = GetComponent<XRGrabInteractable>();

        if (beakerCollider == null)
            beakerCollider = GetComponent<Collider>();

        if (beakerFillZone == null)
            beakerFillZone = GetComponentInChildren<BeakerFillZone>();

        if (beakerFillZoneCollider == null && beakerFillZone != null)
            beakerFillZoneCollider = beakerFillZone.GetComponent<Collider>();

        if (beakerLiquid == null)
            beakerLiquid = GetComponentInChildren<BeakerLiquid>();

        if (beakerReturnOriginalPoint == null)
            beakerReturnOriginalPoint = GetComponent<BeakerReturnOriginalPoint>();
    }
}