using System.Collections;
using UnityEngine;

public class ExperimentSetupSwitcher : MonoBehaviour
{
    [Header("Setups")]
    [SerializeField] private GameObject instructionsSetup;
    [SerializeField] private GameObject experimentSetup;

    [Header("Transition")]
    [SerializeField] private CanvasGroup transitionGroup;
    [SerializeField] private float transitionDelay = 2.5f;
    [SerializeField] private float transitionDuration = 0.4f;
    [SerializeField] private float transitionAlpha = 0.7f;

    [Header("Audio")]
    [SerializeField] private AudioClip transitionAudio;
    [SerializeField] private float transitionVolume = 0.3f;

    private bool hasStarted = false;

    public void StartExperiment()
    {
        if (hasStarted)
            return;

        hasStarted = true;
        AudioManager.Instance?.PlayAudio(transitionAudio, transitionVolume);

        StartCoroutine(SwitchRoutine());
    }

    private IEnumerator SwitchRoutine()
    {
        yield return Fade(0f, transitionAlpha);

        yield return new WaitForSeconds(transitionDelay);

        if (instructionsSetup != null)
            instructionsSetup.SetActive(false);

        if (experimentSetup != null)
            experimentSetup.SetActive(true);

        yield return Fade(transitionAlpha, 0f);
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        if (transitionGroup == null)
            yield break;

        float elapsed = 0f;
        transitionGroup.alpha = startAlpha;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float time = Mathf.Clamp01(elapsed / transitionDuration);
            time = Mathf.SmoothStep(0f, 1f, time);

            transitionGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, time);
            yield return null;
        }

        transitionGroup.alpha = endAlpha;
    }
}