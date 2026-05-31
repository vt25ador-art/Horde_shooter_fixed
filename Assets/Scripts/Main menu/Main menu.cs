using UnityEngine;
using UnityEngine.SceneManagement;

public class Mainmenu : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string gameSceneName = "SampleScene";

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject controlsPanel;

    private void Start()
    {
        ShowMainMenu();
    }

    public void Play()
    {
        GameStats.ResetStats();
        SceneManager.LoadScene("SampleScene");
    }

    public void OpenControls()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (controlsPanel != null)
            controlsPanel.SetActive(true);
    }

    public void CloseControls()
    {
        ShowMainMenu();
    }

    public void Exit()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void ShowMainMenu()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (controlsPanel != null)
            controlsPanel.SetActive(false);
    }
}