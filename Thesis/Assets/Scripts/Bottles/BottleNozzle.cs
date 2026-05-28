using UnityEngine;

public class BottleNozzle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Bottle bottle;

    [Header("Removal Distance")]
    public float removalDistance = 0.1f;

    [Header("Outline")]
    public Behaviour outline;

    [Header("Audio")]
    [SerializeField] private AudioClip pourLiquidAudio;
    [SerializeField] private float pourLiquidVolume = 2f;

    private ReactionTubeFillZone currentReactionTubeFillZone;
    private BeakerFillZone currentBeakerFillZone;

    private bool fillLocked = false;

    private void Awake()
    {
        if (bottle == null)
            bottle = GetComponentInParent<Bottle>();

        if (outline != null)
            outline.enabled = false;
    }

    private void Update()
    {
        if (!fillLocked)
            return;

        HandleZoneRemoval();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (fillLocked)
            return;

        if (!CanPour())
            return;

        HandleReactionTubeFillZone(other);
        HandleBeakerFillZone(other);
    }

    private void OnTriggerExit(Collider other)
    {
        ReactionTubeFillZone reactionTubeZone =
            other.GetComponent<ReactionTubeFillZone>()
            ?? other.GetComponentInParent<ReactionTubeFillZone>();

        BeakerFillZone beakerZone =
            other.GetComponent<BeakerFillZone>()
            ?? other.GetComponentInParent<BeakerFillZone>();

        if (reactionTubeZone == null && beakerZone == null)
            return;

        if (outline != null)
            outline.enabled = false;
    }

    private bool CanPour()
    {
        if (bottle == null || bottle.bottleLiquid == null)
            return false;

        if (bottle.bottleLiquid.IsEmpty())
            return false;

        if (bottle.bottleLiquid.reagentType == ReagentType.None)
            return false;

        return true;
    }

    private void HandleReactionTubeFillZone(Collider reactionTubeCollider)
    {
        ReactionTubeFillZone reactionTubeFillZone =
            reactionTubeCollider.GetComponent<ReactionTubeFillZone>()
            ?? reactionTubeCollider.GetComponentInParent<ReactionTubeFillZone>();

        if (reactionTubeFillZone == null)
            return;

        AudioManager.Instance?.PlayAudio(pourLiquidAudio, pourLiquidVolume);

        FillReactionTube(reactionTubeFillZone);
    }

    private void HandleBeakerFillZone(Collider beakerCollider)
    {
        BeakerFillZone beakerFillZone =
            beakerCollider.GetComponent<BeakerFillZone>()
            ?? beakerCollider.GetComponentInParent<BeakerFillZone>();

        if (beakerFillZone == null)
            return;

        AudioManager.Instance?.PlayAudio(pourLiquidAudio, pourLiquidVolume);

        FillBeaker(beakerFillZone);
    }

    private void FillReactionTube(ReactionTubeFillZone reactionTubeFillZone)
    {
        ReactionTube reactionTube = reactionTubeFillZone.GetComponentInParent<ReactionTube>();

        if (reactionTube == null || reactionTube.reactionTubeLiquid == null)
            return;

        ReactionTubeLiquid reactionTubeLiquid = reactionTube.reactionTubeLiquid;

        if (reactionTubeLiquid.IsFull())
            return;

        bool addReagent = reactionTubeLiquid.AddReagent(bottle.bottleLiquid.reagentType);
        if (!addReagent)
            return;

        if (!bottle.bottleLiquid.RemoveOneStep())
            return;

        LockToReactionTubeZone(reactionTubeFillZone);

        if (reactionTubeLiquid.IsFull() && outline != null)
            outline.enabled = false;
    }

    private void FillBeaker(BeakerFillZone fillZone)
    {
        Beaker beaker = fillZone.GetComponentInParent<Beaker>();

        if (beaker == null || beaker.beakerLiquid == null)
            return;

        BeakerLiquid beakerLiquid = beaker.beakerLiquid;

        if (beakerLiquid.IsFull())
            return;

        bool addReagent = beakerLiquid.AddReagent(bottle.bottleLiquid.reagentType);
        if (!addReagent)
            return;

        if (!bottle.bottleLiquid.RemoveOneStep())
            return;

        LockToBeakerZone(fillZone);

        if (beakerLiquid.IsFull() && outline != null)
            outline.enabled = false;
    }

    private void LockToReactionTubeZone(ReactionTubeFillZone fillZone)
    {
        currentReactionTubeFillZone = fillZone;
        currentBeakerFillZone = null;
        fillLocked = true;

        if (outline != null)
            outline.enabled = true;
    }

    private void LockToBeakerZone(BeakerFillZone fillZone)
    {
        currentBeakerFillZone = fillZone;
        currentReactionTubeFillZone = null;
        fillLocked = true;

        if (outline != null)
            outline.enabled = true;
    }

    private void HandleZoneRemoval()
    {
        Transform fillZoneTransform = null;

        if (currentReactionTubeFillZone != null)
            fillZoneTransform = currentReactionTubeFillZone.transform;
        else if (currentBeakerFillZone != null)
            fillZoneTransform = currentBeakerFillZone.transform;

        if (fillZoneTransform == null)
            return;

        float distance = Vector3.Distance(fillZoneTransform.position, transform.position);

        if (distance > removalDistance)
            UnlockFill();
    }

    private void UnlockFill()
    {
        fillLocked = false;
        currentReactionTubeFillZone = null;
        currentBeakerFillZone = null;
    }
}