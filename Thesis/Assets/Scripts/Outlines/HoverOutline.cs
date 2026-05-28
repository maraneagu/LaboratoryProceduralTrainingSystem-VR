using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class HoverOutline : MonoBehaviour
{
    [Header("References")]
    public XRBaseInteractable interactable;
    public Behaviour outlineBehaviour;

    private bool isHovered = false;
    private bool isSelected = false;

    private void Awake()
    {
        if (interactable == null)
            interactable = GetComponent<XRBaseInteractable>();

        if (outlineBehaviour != null)
            outlineBehaviour.enabled = false;
    }

    private void OnEnable()
    {
        if (interactable == null)
            return;

        interactable.hoverEntered.AddListener(OnHoverEntered);
        interactable.hoverExited.AddListener(OnHoverExited);
        interactable.selectEntered.AddListener(OnSelectEntered);
        interactable.selectExited.AddListener(OnSelectExited);
    }

    private void OnDisable()
    {
        if (interactable == null)
            return;

        interactable.hoverEntered.RemoveListener(OnHoverEntered);
        interactable.hoverExited.RemoveListener(OnHoverExited);
        interactable.selectEntered.RemoveListener(OnSelectEntered);
        interactable.selectExited.RemoveListener(OnSelectExited);
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        isHovered = true;
        UpdateOutline();
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        isHovered = false;
        UpdateOutline();
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        isSelected = true;
        UpdateOutline();
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        isSelected = false;
        UpdateOutline();
    }

    private void UpdateOutline()
    {
        if (outlineBehaviour == null)
            return;

        outlineBehaviour.enabled = isHovered && !isSelected;
    }
}