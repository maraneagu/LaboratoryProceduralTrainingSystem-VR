using UnityEngine;

public class Powder : MonoBehaviour
{
    [Header("State")]
    [HideInInspector] public bool isSpatulaInserted = false;

    [Header("References")]
    public PowderFillZone powderFillZone;
    public Collider powderFillZoneCollider;
    public Transform spatulaInsertPoint;

    public PowderType powderType = PowderType.None;
    public MeshRenderer powderRenderer;

    public PowderType PowderType => powderType;

    private void Awake()
    {
        if (powderFillZone == null)
            powderFillZone = GetComponentInChildren<PowderFillZone>();

        if (powderFillZoneCollider == null && powderFillZone != null)
            powderFillZoneCollider = powderFillZone.GetComponent<Collider>();

        if (powderRenderer == null)
            powderRenderer = GetComponent<MeshRenderer>();
    }
}