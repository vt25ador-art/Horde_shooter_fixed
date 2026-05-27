using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BotHealthUI : MonoBehaviour
{
    [Header("Bot")]
    [SerializeField] private HealthController botHealth;

    [Header("UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text stateText;

    private void Start()
    {
        UpdateUI();

        if (botHealth != null)
            botHealth.OnHealthChanged.AddListener(UpdateUI);

        if (botHealth != null)
            botHealth.OnDowned.AddListener(UpdateUI);

        if (botHealth != null)
            botHealth.OnRevived.AddListener(UpdateUI);

        if (botHealth != null)
            botHealth.OnDied.AddListener(UpdateUI);
    }

    private void Update()
    {
        // Behövs för att down-timern ska uppdateras i UI
        if (botHealth != null && botHealth.isDowned)
            UpdateUI();
    }

    private void OnDestroy()
    {
        if (botHealth == null)
            return;

        botHealth.OnHealthChanged.RemoveListener(UpdateUI);
        botHealth.OnDowned.RemoveListener(UpdateUI);
        botHealth.OnRevived.RemoveListener(UpdateUI);
        botHealth.OnDied.RemoveListener(UpdateUI);
    }

    private void UpdateUI()
    {
        if (botHealth == null)
            return;

        if (healthSlider != null)
        {
            healthSlider.maxValue = botHealth.MaxHealth;
            healthSlider.value = botHealth.CurrentHealth;
        }

        if (healthText != null)
        {
            healthText.text = botHealth.CurrentHealth.ToString("0") + " / " + botHealth.MaxHealth.ToString("0");
        }

        if (stateText != null)
        {
            if (botHealth.IsDead)
            {
                stateText.text = "DEAD";
            }
            else if (botHealth.isDowned)
            {
                stateText.text = "DOWNED: " + botHealth.DownTimeRemaining.ToString("0.0") + "s";
            }
            else
            {
                stateText.text = "OK";
            }
        }
    }
}