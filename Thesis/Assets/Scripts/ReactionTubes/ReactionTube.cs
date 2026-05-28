using System.Diagnostics;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ReactionTube : MonoBehaviour
{
    [Header("State")]
    [HideInInspector] public bool isPipetteInserted = false;

    [Header("References")]
    public Rigidbody rigidBody;
    public XRGrabInteractable grabInteractable;
    public Collider reactionTubeCollider;
    public ReactionTubeFillZone reactionTubeFillZone;
    public Collider reactionTubeFillZoneCollider;

    [Header("Points")]
    public Transform originalPoint;
    public Transform pipetteInsertPoint;
    public Transform beakerInsertPoint;

    [Header("Liquid Contents")]
    public ReactionTubeLiquid reactionTubeLiquid;

    private void Awake()
    {
        if (rigidBody == null)
            rigidBody = GetComponent<Rigidbody>();

        if (grabInteractable == null)
            grabInteractable = GetComponent<XRGrabInteractable>();

        if (reactionTubeCollider == null)
            reactionTubeCollider = GetComponent<Collider>();

        if (reactionTubeFillZone == null)
            reactionTubeFillZone = GetComponentInChildren<ReactionTubeFillZone>();

        if (reactionTubeFillZoneCollider == null && reactionTubeFillZone != null)
            reactionTubeFillZoneCollider = reactionTubeFillZone.GetComponent<Collider>();

        if (reactionTubeLiquid == null)
            reactionTubeLiquid = GetComponentInChildren<ReactionTubeLiquid>();
    }
}