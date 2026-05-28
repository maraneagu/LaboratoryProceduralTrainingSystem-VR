using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ElectricHeater : MonoBehaviour
{
    [Header("References")]
    public ElectricHeaterBeakerInsertZone beakerInsertZone;
    public ParticleSystem boilingBubbles;
    public Behaviour outline;

    [Header("Heating Settings")]
    public float delayBeforeHeating = 1f;
    public float heatingDuration = 3f;
    public float delayAfterHeating = 1f;

    [Header("Audios")]
    [SerializeField] private AudioClip boilingBubblesAudio;

    private bool isHovered = false;
    private XRBaseInteractable interactable;

    private bool isHeating = false;
    private Coroutine heatingRoutine;

    private void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();

        if (interactable != null)
        {
            interactable.hoverEntered.AddListener(OnHoverEntered);
            interactable.hoverExited.AddListener(OnHoverExited);
            interactable.selectEntered.AddListener(OnSelected);
        }

        if (outline != null)
            outline.enabled = false;

        if (boilingBubbles != null)
            boilingBubbles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void OnDestroy()
    {
        if (interactable != null)
        {
            interactable.hoverEntered.RemoveListener(OnHoverEntered);
            interactable.hoverExited.RemoveListener(OnHoverExited);
            interactable.selectEntered.RemoveListener(OnSelected);
        }
    }

    private void Update()
    {
        UpdateOutlineState();
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        isHovered = true;
        UpdateOutlineState();
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        isHovered = false;

        if (outline != null)
            outline.enabled = false;
    }

    private void OnSelected(SelectEnterEventArgs args)
    {
        if (outline != null)
            outline.enabled = false;

        StartElectricHeater();
    }

    public void StartElectricHeater()
    {
        if (isHeating)
            return;

        if (beakerInsertZone == null || !beakerInsertZone.IsReactionTubeInserted)
            return;

        ReactionTube reactionTube = beakerInsertZone.LockedReactionTube;

        if (reactionTube == null || reactionTube.reactionTubeLiquid == null)
            return;

        heatingRoutine = StartCoroutine(SmoothStartElectricHeater(reactionTube));
    }

    private IEnumerator SmoothStartElectricHeater(ReactionTube reactionTube)
    {
        isHeating = true;

        if (outline != null)
            outline.enabled = false;

        if (delayBeforeHeating > 0f)
            yield return new WaitForSeconds(delayBeforeHeating);

        if (!IsReactionTubeStillValid(reactionTube))
        {
            ResetElectricHeater();
            yield break;
        }

        if (boilingBubbles != null)
            boilingBubbles.Play();

        AudioManager.Instance?.PlayAudio(boilingBubblesAudio);

        if (heatingDuration > 0f)
            yield return new WaitForSeconds(heatingDuration);

        if (!IsReactionTubeStillValid(reactionTube))
        {
            StopElectricHeater();
            ResetElectricHeater();
            yield break;
        }

        reactionTube.reactionTubeLiquid.ApplyHeating();

        if (delayAfterHeating > 0f)
            yield return new WaitForSeconds(delayAfterHeating);

        StopElectricHeater();
        ResetElectricHeater();
        UpdateOutlineState();
    }

    private void StopElectricHeater()
    {
        if (boilingBubbles != null)
            boilingBubbles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    private void ResetElectricHeater()
    {
        isHeating = false;
        heatingRoutine = null;
    }

    private void UpdateOutlineState()
    {
        if (outline == null)
            return;

        bool canShowOutline =
            isHovered &&
            !isHeating &&
            beakerInsertZone != null &&
            beakerInsertZone.IsReactionTubeInserted;

        outline.enabled = canShowOutline;
    }

    private bool IsReactionTubeStillValid(ReactionTube reactionTube)
    {
        return beakerInsertZone != null &&
               beakerInsertZone.IsReactionTubeInserted &&
               beakerInsertZone.LockedReactionTube == reactionTube;
    }
}