using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingCreditsRoll : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject endingScreen;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Credits")]
    [SerializeField] private RectTransform creditsTextRect;
    [SerializeField] private TMP_Text creditsText;

    [Header("Movement")]
    [SerializeField] private float startY = 650f;
    [SerializeField] private float endY = -650f;
    [SerializeField] private float scrollDuration = 18f;

    [Header("Fade")]
    [SerializeField] private float fadeInTime = 2f;
    [SerializeField] private float fadeOutTime = 2f;

    [Header("Testing")]
    [SerializeField] private bool allowTestKey = true;
    [SerializeField] private KeyCode testKey = KeyCode.F9;

    [Header("Next Scene Optional")]
    [SerializeField] private string mainMenuSceneName = "";

    private bool playing;

    private void Start()
    {
        if (endingScreen != null)
            endingScreen.SetActive(false);

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    private void Update()
    {
        if (!allowTestKey)
            return;

        if (Input.GetKeyDown(testKey))
            PlayEnding();
    }

    public void PlayEnding()
    {
        if (playing)
            return;

        if (endingScreen != null)
            endingScreen.SetActive(true);

        StartCoroutine(EndingRoutine());
    }

    private IEnumerator EndingRoutine()
    {
        playing = true;

        if (creditsText != null)
        {
            creditsText.text =
                "THE SURVIVORS ESCAPED\n\n" +
                "The radio signal reached the boat.\n" +
                "The horde was held back.\n\n" +
                "The survivors made it out alive.\n\n" +
                "Campaign Complete\n\n" +
                "Created by Adam Örn\n\n" +
                "Tack till:\n" +
                "Sabine\n" +
                "Youtube\n" +
                "Vanessa\n" +
                "Blå kuber\n\n" +
                
                
                "THANKS FOR PLAYING\n\n" +

                "THE SURVIVORS ARE SAFE FOR NOW!";

        }

        if (creditsTextRect != null)
            creditsTextRect.anchoredPosition = new Vector2(0f, startY);

        yield return Fade(0f, 1f, fadeInTime);

        float timer = 0f;

        while (timer < scrollDuration)
        {
            timer += Time.deltaTime;

            float t = timer / scrollDuration;
            float y = Mathf.Lerp(startY, endY, t);

            if (creditsTextRect != null)
                creditsTextRect.anchoredPosition = new Vector2(0f, y);

            yield return null;
        }

        yield return new WaitForSeconds(1f);

        yield return Fade(1f, 0f, fadeOutTime);

        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            if (endingScreen != null)
                endingScreen.SetActive(false);

            playing = false;
        }
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (canvasGroup == null)
            yield break;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = timer / duration;
            canvasGroup.alpha = Mathf.Lerp(from, to, t);

            yield return null;
        }

        canvasGroup.alpha = to;
    }
}
