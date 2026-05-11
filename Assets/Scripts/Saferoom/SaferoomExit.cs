using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SafeRoomExit : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string nextSceneName;

    [Header("Rules")]
    [SerializeField] private float timeRequired = 5f;

    [Header("UI")]
    [SerializeField] private GameObject safeRoomPanel;
    [SerializeField] private TMP_Text timerText;

    [Header("Kills")]
    [SerializeField] private TMP_Text killsText;
    [SerializeField] private KillScore killScore; // din befintliga kill-räknare

    private float timer;
    private bool playerInside;
    private bool loading;

    private void Start()
    {
        if (safeRoomPanel != null)
            safeRoomPanel.SetActive(false);
    }

    private void Update()
    {
        if (!playerInside || loading)
            return;

        timer += Time.deltaTime;

        if (timerText != null)
        {
            float remaining = Mathf.Max(0f, timeRequired - timer);
            timerText.text = "Leaving in: " + remaining.ToString("0.0") + "s";
        }

        if (killsText != null && killScore != null)
        {
            killsText.text = "Kills: " + killScore.Kills;
        }

        if (timer >= timeRequired)
        {
            loading = true;
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;
        timer = 0f;

        if (safeRoomPanel != null)
            safeRoomPanel.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;
        timer = 0f;

        if (safeRoomPanel != null)
            safeRoomPanel.SetActive(false);
    }
}
