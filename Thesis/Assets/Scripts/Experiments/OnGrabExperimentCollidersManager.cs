using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class OnGrabExperimentCollidersManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform experiment;

    [Header("Ignore Duration")]
    [SerializeField] private float ignoreDuration = 0.15f;

    private XRGrabInteractable grabInteractable;
    private Coroutine ignoreRoutine;

    private readonly List<Collider> grabbedColliders = new List<Collider>();
    private readonly List<Collider> experimentColliders = new List<Collider>();

    private bool collisionsIgnored = false;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        CacheGrabbedColliders();
    }

    private void OnEnable()
    {
        if (grabInteractable == null)
            return;

        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        grabInteractable.selectEntered.AddListener(OnGrabbed);
    }

    private void OnDisable()
    {
        if (grabInteractable != null)
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);

        if (ignoreRoutine != null)
        {
            StopCoroutine(ignoreRoutine);
            ignoreRoutine = null;
        }

        RestoreIgnoredCollisions();
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        if (experiment == null)
            return;

        if (ignoreRoutine != null)
        {
            StopCoroutine(ignoreRoutine);
            ignoreRoutine = null;
        }

        RestoreIgnoredCollisions();
        ignoreRoutine = StartCoroutine(IgnoreCollisions());
    }

    private IEnumerator IgnoreCollisions()
    {
        CacheGrabbedColliders();
        CacheExperimentColliders();

        for (int i = 0; i < grabbedColliders.Count; i++)
        {
            Collider collider = grabbedColliders[i];

            if (collider == null)
                continue;

            for (int j = 0; j < experimentColliders.Count; j++)
            {
                Collider experimentCollider = experimentColliders[j];

                if (experimentCollider == null)
                    continue;

                if (experimentCollider.transform.IsChildOf(transform))
                    continue;

                Physics.IgnoreCollision(collider, experimentCollider, true);
            }
        }

        collisionsIgnored = true;

        yield return new WaitForSeconds(ignoreDuration);

        RestoreIgnoredCollisions();
        ignoreRoutine = null;
    }

    private void RestoreIgnoredCollisions()
    {
        if (!collisionsIgnored)
            return;

        for (int i = 0; i < grabbedColliders.Count; i++)
        {
            Collider collider = grabbedColliders[i];

            if (collider == null)
                continue;

            for (int j = 0; j < experimentColliders.Count; j++)
            {
                Collider experimentCollider = experimentColliders[j];

                if (experimentCollider == null)
                    continue;

                if (experimentCollider.transform.IsChildOf(transform))
                    continue;

                Physics.IgnoreCollision(collider, experimentCollider, false);
            }
        }

        collisionsIgnored = false;
    }

    private void CacheGrabbedColliders()
    {
        grabbedColliders.Clear();
        GetComponentsInChildren(true, grabbedColliders);
    }

    private void CacheExperimentColliders()
    {
        experimentColliders.Clear();

        if (experiment == null)
            return;

        experiment.GetComponentsInChildren(true, experimentColliders);
    }
}