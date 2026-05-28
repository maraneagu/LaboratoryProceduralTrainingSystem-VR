using UnityEngine;

public class GuidanceTarget : MonoBehaviour
{
    [SerializeField] private GuidanceTargetType guidanceTargetType;
    [SerializeField] private GuidanceOutline guidanceOutline;

    public GuidanceTargetType TargetType => guidanceTargetType;

    private void Awake()
    {
        if (guidanceOutline == null)
            guidanceOutline = GetComponent<GuidanceOutline>();
    }

    public void ShowGuidance()
    {
        if (guidanceOutline != null)
            guidanceOutline.ShowGuidance();
    }

    public void HideGuidance()
    {
        if (guidanceOutline != null)
            guidanceOutline.HideGuidance();
    }
}