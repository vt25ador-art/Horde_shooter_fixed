using UnityEngine;

public class BotIncapController : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private HealthController health;

    [Header("Disable When Downed")]
    [SerializeField] private MonoBehaviour[] scriptsToDisable;

    [Header("Rigidbody")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private bool freezeWhenDowned = true;

    [Header("Visual Optional")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite downedSprite;
    [SerializeField] private Sprite normalSprite;

    private RigidbodyConstraints2D originalConstraints;

    private void Awake()
    {
        if (health == null)
            health = GetComponent<HealthController>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (rb != null)
            originalConstraints = rb.constraints;
    }

    private void OnEnable()
    {
        if (health == null)
            return;

        health.OnDowned.AddListener(OnBotDowned);
        health.OnRevived.AddListener(OnBotRevived);
        health.OnDied.AddListener(OnBotDied);
    }

    private void OnDisable()
    {
        if (health == null)
            return;

        health.OnDowned.RemoveListener(OnBotDowned);
        health.OnRevived.RemoveListener(OnBotRevived);
        health.OnDied.RemoveListener(OnBotDied);
    }

    private void OnBotDowned()
    {
        SetBotControl(false);

        if (spriteRenderer != null && downedSprite != null)
            spriteRenderer.sprite = downedSprite;

        if (freezeWhenDowned && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    private void OnBotRevived()
    {
        SetBotControl(true);

        if (spriteRenderer != null && normalSprite != null)
            spriteRenderer.sprite = normalSprite;

        if (rb != null)
        {
            rb.constraints = originalConstraints;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    private void OnBotDied()
    {
        SetBotControl(false);

        if (freezeWhenDowned && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    private void SetBotControl(bool state)
    {
        if (scriptsToDisable == null)
            return;

        for (int i = 0; i < scriptsToDisable.Length; i++)
        {
            if (scriptsToDisable[i] != null)
                scriptsToDisable[i].enabled = state;
        }
    }
}