using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class GuidanceInteractionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ExperimentGuidanceManager experimentGuidanceManager;

    private XRBaseInteractor interactor;

    private void Awake()
    {
        interactor = GetComponent<XRBaseInteractor>();
    }

    private void OnEnable()
    {
        if (interactor == null)
            return;

        interactor.selectEntered.AddListener(OnSelectEntered);
    }

    private void OnDisable()
    {
        if (interactor == null)
            return;

        interactor.selectEntered.RemoveListener(OnSelectEntered);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (experimentGuidanceManager != null)
            experimentGuidanceManager.NotifyInteractionStarted();
    }
}