using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Mainmenu : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string gameSceneName = "SampleScene";

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Settings UI")]
    [SerializeField] private Slider corpseLimitSlider;
    [SerializeField] private TMP_Text corpseLimitText;

    private void Start()
    {
        ShowMainMenu();

        if (corpseLimitSlider != null)
        {
            corpseLimitSlider.minValue = 0;
            corpseLimitSlider.maxValue = 100;
            corpseLimitSlider.wholeNumbers = true;
            corpseLimitSlider.value = GameSettings.CorpseLimit;

            corpseLimitSlider.onValueChanged.AddListener(OnCorpseLimitChanged);
        }

        UpdateCorpseLimitText();
    }

    public void Play()
    {
        GameStats.ResetStats();
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenControls()
    {
        mainMenuPanel.SetActive(false);
        controlsPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        controlsPanel.SetActive(false);
        settingsPanel.SetActive(true);

        UpdateCorpseLimitText();
    }

    public void BackToMenu()
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

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    private void OnCorpseLimitChanged(float value)
    {
        GameSettings.CorpseLimit = Mathf.RoundToInt(value);
        UpdateCorpseLimitText();
    }

    private void UpdateCorpseLimitText()
    {
        if (corpseLimitText != null)
            corpseLimitText.text = "Corpse Limit: " + GameSettings.CorpseLimit;
    }
}