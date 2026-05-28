using System.Collections.Generic;
using UnityEngine;

public class ExperimentCollidersManager : MonoBehaviour
{
    public static ExperimentCollidersManager Instance { get; private set; }

    private readonly Dictionary<Collider, bool> originalColliderStates = new Dictionary<Collider, bool>();
    private int disableRequests = 0;

    public bool AreExperimentCollidersDisabled => disableRequests > 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void DisableExperimentColliders(Transform experimentRoot, Transform returningObjectRoot)
    {
        if (experimentRoot == null)
            return;

        disableRequests++;

        if (disableRequests != 1)
            return;

        originalColliderStates.Clear();

        Collider[] colliders = experimentRoot.GetComponentsInChildren<Collider>(true);

        foreach (Collider collider in colliders)
        {
            if (collider == null)
                continue;

            if (returningObjectRoot != null && collider.transform.IsChildOf(returningObjectRoot))
                continue;

            originalColliderStates[collider] = collider.enabled;
            collider.enabled = false;
        }
    }

    public void EnableExperimentColliders()
    {
        if (disableRequests <= 0)
        {
            disableRequests = 0;
            return;
        }

        disableRequests--;

        if (disableRequests != 0)
            return;

        foreach (KeyValuePair<Collider, bool> pair in originalColliderStates)
        {
            Collider collider = pair.Key;

            if (collider == null)
                continue;

            collider.enabled = pair.Value;
        }

        originalColliderStates.Clear();
    }
}