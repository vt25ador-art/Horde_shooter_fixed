using UnityEngine;
using UnityEngine.Events;

public class HealthController : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float _currentHealth = 100f;
    [SerializeField] private float _maxiumHealth = 100f;

    [Header("Downed")]
    public bool isDowned { get; private set; }

    [SerializeField] private float downTime = 20f;
    private float downTimer;

    private bool isDead;

    public float RemainingDownTime => downTimer;
    public bool IsDead => isDead;

    public float ReimainingHealthPercentage
    {
        get
        {
            if (_maxiumHealth <= 0f)
                return 0f;

            return _currentHealth / _maxiumHealth;
        }
    }

    public UnityEvent OnDowned;
    public UnityEvent OnRevived;
    public UnityEvent OnDied;
    public UnityEvent OnHealthChanged;

    private void Awake()
    {
        _currentHealth = Mathf.Clamp(_currentHealth, 0f, _maxiumHealth);
    }

    private void Update()
    {
        if (!isDowned || isDead)
            return;

        downTimer -= Time.deltaTime;

        if (downTimer <= 0f)
        {
            Die();
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead)
            return;

        if (isDowned)
            return;

        _currentHealth -= damageAmount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0f, _maxiumHealth);

        OnHealthChanged.Invoke();

        if (_currentHealth <= 0f)
        {
            EnterDownState();
        }
    }

    private void EnterDownState()
    {
        if (isDowned || isDead)
            return;

        isDowned = true;
        downTimer = downTime;

        Debug.Log(gameObject.name + " is DOWN!");

        OnDowned.Invoke();
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
        isDowned = false;

        Debug.Log(gameObject.name + " died");

        OnDied.Invoke();
    }

    public void AddHealth(float amountToAdd)
    {
        if (isDead)
            return;

        if (isDowned)
            return;

        _currentHealth += amountToAdd;
        _currentHealth = Mathf.Clamp(_currentHealth, 0f, _maxiumHealth);

        OnHealthChanged.Invoke();
    }

    public void Revive(float reviveHealth)
    {
        if (isDead)
            return;

        if (!isDowned)
            return;

        isDowned = false;
        downTimer = 0f;

        _currentHealth = Mathf.Clamp(reviveHealth, 1f, _maxiumHealth);

        Debug.Log(gameObject.name + " revived");

        OnHealthChanged.Invoke();
        OnRevived.Invoke();
    }
}
