using UnityEngine;

public class BeakerFillZone : MonoBehaviour
{
    [Header("References")]
    public Beaker beaker;

    private void Awake()
    {
        if (beaker == null)
            beaker = GetComponentInParent<Beaker>();
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (beaker == null)
            return;

        HandlePipetteDispenser(collider);
        HandlePipetteAspirator(collider);
        HandleSpatula(collider);
        HandleStirringRod(collider);
    }

    private void HandlePipetteDispenser(Collider pipetteCollider)
    {
        if (pipetteCollider.GetComponent<PipetteTip>() == null)
            return;

        PipetteDispenserBeaker pipette =
            pipetteCollider.GetComponentInParent<PipetteDispenserBeaker>();

        if (pipette == null)
            return;

        if (beaker.isPipetteInserted)
            return;

        if (beaker.pipetteDispenserPoint == null)
        {
            Debug.LogWarning("[BeakerFillZone] Pipette Dispenser Point Missing!");
            return;
        }

        if (!pipette.CanInsert())
        {
            Debug.Log("[BeakerFillZone] Pipette Dispenser Entered Zone, Insertion Not Allowed!");
            return;
        }

        pipette.InsertIntoBeaker(beaker);
    }

    private void HandlePipetteAspirator(Collider pipetteCollider)
    {
        if (pipetteCollider.GetComponent<PipetteTip>() == null)
            return;

        PipetteAspiratorBeaker pipette =
            pipetteCollider.GetComponentInParent<PipetteAspiratorBeaker>();

        if (pipette == null)
            return;

        if (beaker.isPipetteInserted)
            return;

        if (beaker.pipetteAspiratorPoint == null)
        {
            Debug.LogWarning("[BeakerFillZone] Pipette Aspirator Point Missing!");
            return;
        }

        if (!pipette.CanInsert())
        {
            Debug.Log("[BeakerFillZone] Pipette Aspirator Entered Zone, Insertion Not Allowed!");
            return;
        }

        pipette.InsertIntoBeaker(beaker);
    }

    private void HandleSpatula(Collider spatulaCollider)
    {
        if (spatulaCollider.GetComponent<SpatulaSpoon>() == null)
            return;

        SpatulaInserterBeaker spatula =
            spatulaCollider.GetComponentInParent<SpatulaInserterBeaker>();

        if (spatula == null)
            return;

        if (beaker.isSpatulaInserted)
            return;

        if (beaker.spatulaInsertPoint == null)
        {
            Debug.LogWarning("[BeakerFillZone] Spatula Insert Point Missing!");
            return;
        }

        if (!spatula.CanInsert())
        {
            Debug.Log("[BeakerFillZone] Spatula Entered Zone, Insertion Not Allowed!");
            return;
        }

        spatula.InsertIntoBeaker(beaker);
    }

    private void HandleStirringRod(Collider stirringRodCollider)
    {
        StirringRod stirringRod = stirringRodCollider.GetComponentInParent<StirringRod>();

        if (stirringRod == null || stirringRod.stirringRodInserterBeaker == null)
            return;

        if (!stirringRod.stirringRodInserterBeaker.CanInsert(beaker))
            return;

        stirringRod.stirringRodInserterBeaker.InsertIntoBeaker(beaker);
    }

    public void DisableZone()
    {
        if (beaker == null)
            return;

        if (beaker.beakerFillZoneCollider != null)
            beaker.beakerFillZoneCollider.enabled = false;

        if (beaker.beakerCollider != null)
            beaker.beakerCollider.enabled = false;
    }

    public void EnableZone()
    {
        if (beaker == null)
            return;

        if (beaker.beakerFillZoneCollider != null)
            beaker.beakerFillZoneCollider.enabled = true;

        if (beaker.beakerCollider != null)
            beaker.beakerCollider.enabled = true;
    }
}