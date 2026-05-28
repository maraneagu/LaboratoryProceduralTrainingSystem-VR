using System.Diagnostics;
using UnityEngine;

public class Bottle : MonoBehaviour
{
    [Header("State")]
    [HideInInspector] public bool isPipetteInserted = false;
    [HideInInspector] public bool isLidOpen = false;

    [Header("References")]
    public Collider bottleCollider;
    public BottleFillZone bottleFillZone;
    public Collider bottleFillZoneCollider;
    public Transform pipetteInsertPoint;

    [Header("Lid References")]
    public Transform lidTransform;
    public Transform lidClosedPoint;
    public Transform lidOpenPoint;

    [Header("Liquid Contents")]
    public BottleLiquid bottleLiquid;

    private void Awake()
    {
        if (bottleCollider == null)
            bottleCollider = GetComponent<Collider>();

        if (bottleFillZone == null)
            bottleFillZone = GetComponentInChildren<BottleFillZone>();

        if (bottleFillZoneCollider == null && bottleFillZone != null)
            bottleFillZoneCollider = bottleFillZone.GetComponent<Collider>();

        if (bottleLiquid == null)
            bottleLiquid = GetComponentInChildren<BottleLiquid>();
    }
}