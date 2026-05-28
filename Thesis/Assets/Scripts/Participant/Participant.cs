using UnityEngine;

public class Participant : MonoBehaviour
{
    public enum DominantHand
    {
        Left,
        Right
    }

    public static Participant Instance { get; private set; }

    [Header("Participant Settings")]
    [SerializeField] private string participantId;
    [SerializeField] private DominantHand participantDominantHand = DominantHand.Right;

    [Header("Controllers")]
    [SerializeField] private GameObject leftController;
    [SerializeField] private GameObject leftRayOrigin;
    [SerializeField] private GameObject rightController;
    [SerializeField] private GameObject rightRayOrigin;

    public string ParticipantId => participantId;
    public DominantHand ParticipantDominantHand => participantDominantHand;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        SetDominantHand();
    }

    private void SetDominantHand()
    {
        bool leftHand = participantDominantHand == DominantHand.Left;
        bool rightHand = participantDominantHand == DominantHand.Right;

        if (leftController != null)
            leftController.SetActive(leftHand);

        if (leftRayOrigin != null)
            leftRayOrigin.SetActive(leftHand);

        if (rightController != null)
            rightController.SetActive(rightHand);

        if (rightRayOrigin != null)
            rightRayOrigin.SetActive(rightHand);
    }
}