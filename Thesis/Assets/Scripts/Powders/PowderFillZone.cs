using UnityEngine;

public class PowderFillZone : MonoBehaviour
{
    [Header("References")]
    public Powder powder;

    private void Awake()
    {
        if (powder == null)
            powder = GetComponentInParent<Powder>();
    }

    private void OnTriggerEnter(Collider spatulaCollider)
    {
        if (spatulaCollider.GetComponent<SpatulaSpoon>() == null)
            return;

        SpatulaInserterPowder spatula =
            spatulaCollider.GetComponentInParent<SpatulaInserterPowder>();

        if (spatula == null || powder == null)
            return;

        if (powder.spatulaInsertPoint == null)
        {
            Debug.LogWarning("[PowderFillZone] Insertion Point Missing!");
            return;
        }

        if (!spatula.CanInsert())
        {
            Debug.Log("[PowderFillZone] Spatula Entered Zone, Insertion Not Allowed!");
            return;
        }

        spatula.InsertIntoPowder(powder);
    }

    public void DisableZone()
    {
        if (powder != null)
        {
            if (powder.powderFillZoneCollider != null)
                powder.powderFillZoneCollider.enabled = false;
        }
    }

    public void EnableZone()
    {
        if (powder != null)
        {
            if (powder.powderFillZoneCollider != null)
                powder.powderFillZoneCollider.enabled = true;
        }
    }
}