using UnityEngine;
using TMPro;

public class DoorUnlockByKills_old01 : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private int killsNeeded = 20;
    [SerializeField] private Collider2D doorCollider;
    [SerializeField] private SpriteRenderer doorSprite;
    [SerializeField] private Color unlockedColor = Color.green;
    [SerializeField] private bool destroyDoorOnUnlock = false;

    [Header("UI")]
    [SerializeField] private GameObject promptUI;   // Panel eller parent-objekt för UI
    [SerializeField] private TMP_Text promptText;   // Texten som visas
    [SerializeField] private bool updateTextWhilePlayerIsNear = true;

    private bool unlocked;
    private bool playerNearby;

    public bool IsUnlocked => unlocked;

    private void Awake()
    {
        if (doorCollider == null)
            doorCollider = GetComponent<Collider2D>();

        if (doorSprite == null)
            doorSprite = GetComponent<SpriteRenderer>();

        if (promptUI != null)
            promptUI.SetActive(false);
    }

    private void Update()
    {
        if (!unlocked && KillScore.Instance != null && KillScore.Instance.Kills >= killsNeeded)
            UnlockDoor();

        if (playerNearby && updateTextWhilePlayerIsNear)
            UpdatePrompt();
    }

    private void UnlockDoor()
    {
        unlocked = true;

        if (doorCollider != null)
            doorCollider.enabled = false;

        if (doorSprite != null)
            doorSprite.color = unlockedColor;

        UpdatePrompt();

        if (destroyDoorOnUnlock)
            Destroy(gameObject);
    }

    private void UpdatePrompt()
    {
        if (promptText == null)
            return;

        int currentKills = KillScore.Instance != null ? KillScore.Instance.Kills : 0;

        if (unlocked)
        {
            promptText.text = "Dörr upplåst";
        }
        else
        {
            int remaining = Mathf.Max(0, killsNeeded - currentKills);
            promptText.text = $"Kills: {currentKills}/{killsNeeded}\nKvar för att öppna: {remaining}";
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerNearby = true;

        if (promptUI != null)
            promptUI.SetActive(true);

        UpdatePrompt();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerNearby = false;

        if (promptUI != null)
            promptUI.SetActive(false);
    }
}