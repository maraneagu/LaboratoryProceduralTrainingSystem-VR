using UnityEngine;

public class SpatulaPowder : MonoBehaviour
{
    private MeshRenderer spatulaPowderRenderer;
    private Material spatulaPowderMaterial;

    public Powder powder { get; private set; }

    public bool HasPowder =>
        powder != null &&
        spatulaPowderRenderer != null &&
        spatulaPowderRenderer.enabled;

    private void Awake()
    {
        spatulaPowderRenderer = GetComponent<MeshRenderer>();

        if (spatulaPowderRenderer != null)
            spatulaPowderMaterial = spatulaPowderRenderer.material;

        EmptySpatula();
    }

    public void FillFromPowder(Powder _powder)
    {
        if (_powder == null || _powder.powderRenderer == null)
        {
            Debug.Log("[SpatulaPowder] Powder Or PowderRenderer Is Null!");
            return;
        }

        powder = _powder;

        spatulaPowderRenderer.enabled = true;
        spatulaPowderRenderer.material = _powder.powderRenderer.material;

        Debug.Log($"[SpatulaPowder] Spatula Filled With {_powder.PowderType}!");
    }

    public void EmptySpatula()
    {
        if (spatulaPowderRenderer == null || spatulaPowderMaterial == null)
            return;

        spatulaPowderRenderer.enabled = false;
        spatulaPowderRenderer.material = spatulaPowderMaterial;

        powder = null;

        Debug.Log("[SpatulaPowder] Spatula Emptied!");
    }
}