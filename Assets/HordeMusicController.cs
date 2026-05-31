using System.Collections;
using UnityEngine;

public class HordeMusicController : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hordeTheme;

    [Header("Fade")]
    [SerializeField] private float fadeInTime = 1.5f;
    [SerializeField] private float fadeOutTime = 3f;
    [SerializeField] private float targetVolume = 0.8f;

    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = true;
            audioSource.spatialBlend = 0f;
            audioSource.volume = 0f;
        }
    }

    public void StartHordeMusic()
    {
        if (audioSource == null || hordeTheme == null)
            return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        audioSource.clip = hordeTheme;

        if (!audioSource.isPlaying)
            audioSource.Play();

        fadeRoutine = StartCoroutine(FadeVolume(targetVolume, fadeInTime));
    }

    public void StopHordeMusic()
    {
        if (audioSource == null)
            return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeOutAndStop());
    }

    private IEnumerator FadeVolume(float target, float duration)
    {
        float startVolume = audioSource.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            audioSource.volume = Mathf.Lerp(startVolume, target, t);
            yield return null;
        }

        audioSource.volume = target;
    }

    private IEnumerator FadeOutAndStop()
    {
        float startVolume = audioSource.volume;
        float timer = 0f;

        while (timer < fadeOutTime)
        {
            timer += Time.deltaTime;
            float t = timer / fadeOutTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
    }
}