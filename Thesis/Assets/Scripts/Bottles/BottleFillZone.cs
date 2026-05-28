using UnityEngine;

public class BottleFillZone : MonoBehaviour
{
    [Header("References")]
    public Bottle bottle;

    private void Awake()
    {
        if (bottle == null)
            bottle = GetComponentInParent<Bottle>();
    }

    private void OnTriggerEnter(Collider pipetteCollider)
    {
        if (pipetteCollider.GetComponent<PipetteTip>() == null)
            return;

        PipetteInserterBottle pipette =
            pipetteCollider.GetComponentInParent<PipetteInserterBottle>();

        if (pipette == null || bottle == null)
            return;

        if (!bottle.isLidOpen)
        {
            Debug.Log("[BottleFillZone] Bottle Lid Closed! Insertion Not Allowed!");
            return;
        }

        if (bottle.pipetteInsertPoint == null)
        {
            Debug.LogWarning("[BottleFillZone] Insertion Point Is Missing!");
            return;
        }

        if (!pipette.CanInsert())
        {
            Debug.Log("[BottleFillZone] Pipette Entered Zone, Insertion Not Allowed!");
            return;
        }

        pipette.InsertIntoBottle(bottle);
    }

    public void DisableZone()
    {
        if (bottle != null)
        {
            if (bottle.bottleFillZoneCollider != null)
                bottle.bottleFillZoneCollider.enabled = false;

            if (bottle.bottleCollider != null)
                bottle.bottleCollider.enabled = false;
        }
    }

    public void EnableZone()
    {
        if (bottle != null)
        {
            if (bottle.bottleFillZoneCollider != null)
                bottle.bottleFillZoneCollider.enabled = true;

            if (bottle.bottleCollider != null)
                bottle.bottleCollider.enabled = true;
        }
    }
}