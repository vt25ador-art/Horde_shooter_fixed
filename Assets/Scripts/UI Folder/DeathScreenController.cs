using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreenController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject deathPanel;

    [Header("Settings")]
    [SerializeField] private bool pauseTime = true;

    private bool _isDead;

    private void Awake()
    {
        if (deathPanel != null)
            deathPanel.SetActive(false);
    }

    public void ShowDeathScreen()
    {
        if (_isDead) return;
        _isDead = true;

        if (deathPanel != null)
            deathPanel.SetActive(true);

        if (pauseTime)
            Time.timeScale = 0f;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}