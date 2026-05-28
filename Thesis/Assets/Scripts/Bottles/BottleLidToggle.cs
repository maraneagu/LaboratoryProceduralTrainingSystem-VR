using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BottleLidToggle : MonoBehaviour
{
    [Header("References")]
    public Bottle bottle;
    [SerializeField] private Transform guidanceBottleLid;

    [Header("Animation")]
    public float moveDuration = 0.25f;

    [Header("Audio")]
    [SerializeField] private AudioClip openAndCloseBottleAudio;
    [SerializeField] private float openAndCloseBottleVolume = 0.15f;

    private Coroutine moveRoutine;
    private bool isMoving = false;

    private XRBaseInteractable interactable;

    private void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();

        if (bottle == null)
            bottle = GetComponentInParent<Bottle>();

        if (bottle != null && bottle.lidTransform == null)
            bottle.lidTransform = transform;
    }

    private void OnEnable()
    {
        if (interactable != null)
            interactable.selectEntered.AddListener(OnSelected);
    }

    private void OnDisable()
    {
        if (interactable != null)
            interactable.selectEntered.RemoveListener(OnSelected);
    }

    private void Start()
    {
        if (bottle != null && bottle.lidClosedPoint != null && bottle.lidTransform != null)
        {
            bottle.lidTransform.position = bottle.lidClosedPoint.position;
            bottle.lidTransform.rotation = bottle.lidClosedPoint.rotation;
            bottle.isLidOpen = false;
        }

        if (guidanceBottleLid != null && bottle != null && bottle.lidClosedPoint != null)
        {
            guidanceBottleLid.position = bottle.lidClosedPoint.position;
            guidanceBottleLid.rotation = bottle.lidClosedPoint.rotation;
        }
    }

    private void OnSelected(SelectEnterEventArgs args)
    {
        if (isMoving || bottle == null || bottle.lidClosedPoint == null || bottle.lidOpenPoint == null)
            return;

        if (bottle.isLidOpen && bottle.isPipetteInserted)
        {
            Debug.Log("[BottleLidToggle] Lid Can't Be Closed While Pipette Is Inserted!");
            return;
        }

        bool willOpen = !bottle.isLidOpen;
        Transform target = willOpen ? bottle.lidOpenPoint : bottle.lidClosedPoint;

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        AudioManager.Instance?.PlayAudio(openAndCloseBottleAudio, openAndCloseBottleVolume);

        moveRoutine = StartCoroutine(SmoothMoveLids(target));
        bottle.isLidOpen = willOpen;
    }

    private IEnumerator SmoothMoveLids(Transform target)
    {
        isMoving = true;

        Vector3 mainStartPosition = bottle.lidTransform.position;
        Quaternion mainStartRotation = bottle.lidTransform.rotation;

        Vector3 mainEndPosition = target.position;
        Quaternion mainEndRotation = target.rotation;

        Vector3 guidanceStartPosition = Vector3.zero;
        Quaternion guidanceStartRotation = Quaternion.identity;

        bool hasGuidanceBottleLid = guidanceBottleLid != null;

        if (hasGuidanceBottleLid)
        {
            guidanceStartPosition = guidanceBottleLid.position;
            guidanceStartRotation = guidanceBottleLid.rotation;
        }

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float time = Mathf.Clamp01(elapsed / moveDuration);

            bottle.lidTransform.position = Vector3.Lerp(mainStartPosition, mainEndPosition, time);
            bottle.lidTransform.rotation = Quaternion.Slerp(mainStartRotation, mainEndRotation, time);

            if (hasGuidanceBottleLid)
            {
                guidanceBottleLid.position = Vector3.Lerp(guidanceStartPosition, mainEndPosition, time);
                guidanceBottleLid.rotation = Quaternion.Slerp(guidanceStartRotation, mainEndRotation, time);
            }

            yield return null;
        }

        bottle.lidTransform.position = mainEndPosition;
        bottle.lidTransform.rotation = mainEndRotation;

        if (hasGuidanceBottleLid)
        {
            guidanceBottleLid.position = mainEndPosition;
            guidanceBottleLid.rotation = mainEndRotation;
        }

        isMoving = false;
    }

    public bool IsOpen()
    {
        return bottle != null && bottle.isLidOpen;
    }
}