using TMPro;
using UnityEngine;

public class DownedUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthController playerHealth;

    [Header("UI")]
    [SerializeField] private GameObject downedPanel;
    [SerializeField] private TMP_Text downedText;

    private void Awake()
    {
        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<HealthController>();

        HideDownedUI();
    }

    private void OnEnable()
    {
        if (playerHealth == null)
            return;

        playerHealth.OnDowned.AddListener(ShowDownedUI);
        playerHealth.OnRevived.AddListener(HideDownedUI);
        playerHealth.OnDied.AddListener(HideDownedUI);
    }

    private void OnDisable()
    {
        if (playerHealth == null)
            return;

        playerHealth.OnDowned.RemoveListener(ShowDownedUI);
        playerHealth.OnRevived.RemoveListener(HideDownedUI);
        playerHealth.OnDied.RemoveListener(HideDownedUI);
    }

    private void Update()
    {
        if (playerHealth == null || downedText == null)
            return;

        if (!playerHealth.isDowned)
            return;

        downedText.text =
            "DOWNED\n" +
            "WAITING FOR REVIVE\n\n" +
            "TIME LEFT: " + playerHealth.DownTimeRemaining.ToString("0.0");
    }

    private void ShowDownedUI()
    {
        if (downedPanel != null)
            downedPanel.SetActive(true);

        if (downedText != null)
        {
            downedText.text =
                "DOWNED\n" +
                "WAITING FOR REVIVE";
        }
    }

    private void HideDownedUI()
    {
        if (downedPanel != null)
            downedPanel.SetActive(false);
    }
}