using UnityEngine;

public class ReactionTubeFillZone : MonoBehaviour
{
    [Header("References")]
    public ReactionTube reactionTube;

    private void Awake()
    {
        if (reactionTube == null)
            reactionTube = GetComponentInParent<ReactionTube>();
    }

    private void OnTriggerEnter(Collider pipetteCollider)
    {
        if (pipetteCollider.GetComponent<PipetteTip>() == null)
            return;

        PipetteInserterReactionTube pipette = 
            pipetteCollider.GetComponentInParent<PipetteInserterReactionTube>();
        
        if (pipette == null || reactionTube == null)
            return;

        if (reactionTube.isPipetteInserted)
            return;

        if (reactionTube.pipetteInsertPoint == null)
        {
            Debug.LogWarning("[ReactionTubeFillZone] Insertion Point Missing!");
            return;
        }

        if (!pipette.CanInsert())
        {
            Debug.Log("[ReactionTubeFillZone] Pipette Entered Zone, Insertion Not Allowed!");
            return;
        }

        pipette.InsertIntoReactionTube(reactionTube);
    }

    public void DisableZone()
    {
        if (reactionTube != null)
        {
            if (reactionTube.reactionTubeFillZoneCollider != null)
                reactionTube.reactionTubeFillZoneCollider.enabled = false;

            if (reactionTube.reactionTubeCollider != null)
                reactionTube.reactionTubeCollider.enabled = false;
        }
    }

    public void EnableZone()
    {
        if (reactionTube != null)
        {
            if (reactionTube.reactionTubeFillZoneCollider != null)
                reactionTube.reactionTubeFillZoneCollider.enabled = true;

            if (reactionTube.reactionTubeCollider != null)
                reactionTube.reactionTubeCollider.enabled = true;
        }
    }
}