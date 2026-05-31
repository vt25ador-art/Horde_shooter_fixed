using UnityEngine;
using UnityEngine.SceneManagement;

public class RescueEscapeTrigger : MonoBehaviour
{
    [Header("Ending")]
    [SerializeField] private string endingSceneName = "Ending";
    [SerializeField] private bool requirePlayerTag = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private bool canEscape;
    private Collider2D triggerCollider;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();

        if (triggerCollider != null)
            triggerCollider.enabled = false;

        canEscape = false;
    }

    public void EnableEscape()
    {
        canEscape = true;

        if (triggerCollider != null)
            triggerCollider.enabled = true;

        if (debugLogs)
            Debug.Log("ESCAPE TRIGGER ENABLED");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryEscape(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryEscape(other);
    }

    private void TryEscape(Collider2D other)
    {
        if (debugLogs)
            Debug.Log("Something touched escape trigger: " + other.name + " Tag: " + other.tag);

        if (!canEscape)
        {
            if (debugLogs)
                Debug.Log("Escape not enabled yet");
            return;
        }

        if (requirePlayerTag && !other.CompareTag("Player"))
        {
            if (debugLogs)
                Debug.Log("Not Player tag");
            return;
        }

        if (debugLogs)
            Debug.Log("Loading ending scene: " + endingSceneName);

        SceneManager.LoadScene(endingSceneName);
    }
}