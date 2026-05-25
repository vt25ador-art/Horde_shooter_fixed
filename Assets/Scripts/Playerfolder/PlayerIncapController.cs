using UnityEngine;

public class PlayerIncapController : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private HealthController health;

    [Header("Disable When Downed")]
    [SerializeField] private MonoBehaviour[] scriptsToDisable;

    [Header("Rigidbody")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private bool freezeRigidbodyWhenDowned = true;

    private RigidbodyConstraints2D originalConstraints;

    private void Awake()
    {
        if (health == null)
            health = GetComponent<HealthController>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (rb != null)
            originalConstraints = rb.constraints;
    }

    private void OnEnable()
    {
        if (health == null)
            return;

        health.OnDowned.AddListener(OnPlayerDowned);
        health.OnRevived.AddListener(OnPlayerRevived);
        health.OnDied.AddListener(OnPlayerDied);
    }

    private void OnDisable()
    {
        if (health == null)
            return;

        health.OnDowned.RemoveListener(OnPlayerDowned);
        health.OnRevived.RemoveListener(OnPlayerRevived);
        health.OnDied.RemoveListener(OnPlayerDied);
    }

    private void OnPlayerDowned()
    {
        SetPlayerControl(false);

        if (freezeRigidbodyWhenDowned && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    private void OnPlayerRevived()
    {
        SetPlayerControl(true);

        if (rb != null)
        {
            rb.constraints = originalConstraints;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    private void OnPlayerDied()
    {
        SetPlayerControl(false);

        if (freezeRigidbodyWhenDowned && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    private void SetPlayerControl(bool state)
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

