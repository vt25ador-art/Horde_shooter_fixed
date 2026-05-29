using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RadioFinaleEvent : MonoBehaviour
{
    [Header("Finale")]
    [SerializeField] private float surviveTime = 300f; // 5 minuter
    [SerializeField] private HordeEventController hordeEventController;

    [Header("Rescue Object Optional")]
    [SerializeField] private GameObject rescueObject; // båt/helikopter/bil
    [SerializeField] private Transform rescueStartPoint;
    [SerializeField] private Transform rescueEndPoint;
    [SerializeField] private float rescueMoveSpeed = 3f;

    [Header("Ending")]
    [SerializeField] private GameObject endingScreen;
    [SerializeField] private TMP_Text endingText;
    [SerializeField] private string creditsSceneName;

    [Header("UI")]
    [SerializeField] private GameObject finalePanel;
    [SerializeField] private TMP_Text finaleText;
    [SerializeField] private TMP_Text timerText;

    private bool finaleRunning;
    private bool finaleCompleted;
    private float timer;

    public bool FinaleRunning => finaleRunning;
    public bool FinaleCompleted => finaleCompleted;

    private void Start()
    {
        if (finalePanel != null)
            finalePanel.SetActive(false);

        if (endingScreen != null)
            endingScreen.SetActive(false);

        if (rescueObject != null)
        {
            rescueObject.SetActive(false);

            if (rescueStartPoint != null)
                rescueObject.transform.position = rescueStartPoint.position;
        }
    }

    public void StartFinale()
    {
        if (finaleRunning || finaleCompleted)
            return;

        StartCoroutine(FinaleRoutine());
    }

    private IEnumerator FinaleRoutine()
    {
        finaleRunning = true;
        timer = surviveTime;

        if (finalePanel != null)
            finalePanel.SetActive(true);

        if (finaleText != null)
            finaleText.text = "RADIO CONTACTED - HOLD OUT UNTIL RESCUE ARRIVES";

        if (hordeEventController != null)
            hordeEventController.StartHordeEvent();

        while (timer > 0f)
        {
            timer -= Time.deltaTime;

            if (timerText != null)
                timerText.text = FormatTime(timer);

            yield return null;
        }

        CompleteFinale();
    }

    private void CompleteFinale()
    {
        finaleRunning = false;
        finaleCompleted = true;

        if (hordeEventController != null)
            hordeEventController.ForceStopEvent();

        if (finaleText != null)
            finaleText.text = "RESCUE HAS ARRIVED!";

        if (timerText != null)
            timerText.text = "00:00";

        StartCoroutine(RescueAndEndingRoutine());
    }

    private IEnumerator RescueAndEndingRoutine()
    {
        if (rescueObject != null)
            rescueObject.SetActive(true);

        if (rescueObject != null && rescueEndPoint != null)
        {
            while (Vector3.Distance(rescueObject.transform.position, rescueEndPoint.position) > 0.05f)
            {
                rescueObject.transform.position = Vector3.MoveTowards(
                    rescueObject.transform.position,
                    rescueEndPoint.position,
                    rescueMoveSpeed * Time.deltaTime
                );

                yield return null;
            }
        }

        yield return new WaitForSeconds(2f);

        ShowEndingScreen();
    }

    private void ShowEndingScreen()
    {
        if (finalePanel != null)
            finalePanel.SetActive(false);

        if (endingScreen != null)
            endingScreen.SetActive(true);

        if (endingText != null)
            endingText.text =
                "THE SURVIVORS ESCAPED\n\n" +
                "Campaign Complete\n\n" +
                "Created by Talismanen2";

        if (!string.IsNullOrEmpty(creditsSceneName))
            StartCoroutine(LoadCreditsAfterDelay());
    }

    private IEnumerator LoadCreditsAfterDelay()
    {
        yield return new WaitForSeconds(8f);
        SceneManager.LoadScene(creditsSceneName);
    }

    private string FormatTime(float time)
    {
        time = Mathf.Max(0f, time);

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        return minutes.ToString("00") + ":" + seconds.ToString("00");
    }
}

