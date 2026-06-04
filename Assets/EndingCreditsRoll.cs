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

    [Header("Auto Start")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private float startDelay = 0.5f;

    private bool playing;

    private void Start()
    {
        if (endingScreen != null)
            endingScreen.SetActive(false);

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        if (playOnStart)
            StartCoroutine(PlayAfterDelay());
    }

    private IEnumerator PlayAfterDelay()
    {
        yield return new WaitForSeconds(startDelay);
        PlayEnding();
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

            "The survivors regretted everything.\n\n" +

            "But somehow...\n\n" +
            "they made it out alive.\n\n" +

            "Campaign Complete\n\n" +

            "Statistics:\n" +
            "Bullets Fired: Not Enough\n" +
            "Reloads Panicked: Yes\n\n" +
            "Common Sense Used: 0\n\n" +
            "Blue Cubes Encountered: Classified\n\n" +

            "Art by Vanessa och GPT\n\n" +
            "Music by Sabine, Stich och Adam Örn\n\n" +
            "Created by Adam Örn\n\n" +
            "MainScreen inspiration from a childhood game\n\n" +

            "Thanks to:\n" +
            "Sabine\n" +
            "Stich\n" +
            "Youtube\n" +
            "Vanessa\n" +
            "Blå kuber\n" +
            "Unity Profiler\n" +
            "The broken reload sound\n" +
            "Every zombie who spawned too close\n" +
            "THE BACKROOMS?\n" +
            "The bot, despite everything\n" +
            "THE SOUND FILE THAT LAGGED THE EDITOR\n\n" +


            "Speciellt tack till:\n" +
            "Min kusin för sitt tålamod\n\n" +

            "Final Report:\n" +
            "The valley is silent again.\n" +
            "The rescue boat left the shore.\n" +
            "The spread of the infection is still ongoing.\n" +

                "The survivors are safe.\n\n" +

                "For now.\n\n" +


                "Zombies Killed: " + GameStats.TotalKills + "\n\n" +


                "THANKS FOR PLAYING";

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
