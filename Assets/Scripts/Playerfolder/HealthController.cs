using UnityEngine;
using UnityEngine.Events;

public class HealthController : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float _currentHealth = 100f;
    [SerializeField] private float _maxiumHealth = 100f;

    [Header("Downed")]
    public bool isDowned { get; private set; }

    public float CurrentHealth => _currentHealth;
    public float MaxHealth => _maxiumHealth;
    public float DownTimeRemaining => downTimer;

    [Header("Debug")]
    [SerializeField] private bool godMode;

    [Header("Down Limit")]
    [SerializeField] private int maxDownsBeforeDeath = 3;
    [SerializeField] private int downCount;

    public int DownCount => downCount;

    public bool GodMode => godMode;

    public void SetGodMode(bool state)
    {
        godMode = state;
        Debug.Log(gameObject.name + " GodMode: " + godMode);
    }


    [SerializeField] private float downTime = 30f;
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
        if (godMode)
            return;

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

        downCount++;

        Debug.Log(gameObject.name + " down count: " + downCount + "/" + maxDownsBeforeDeath);

        if (downCount >= maxDownsBeforeDeath)
        {
            Die();
            return;
        }

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
        downTimer = 0f;
        _currentHealth = 0f;

        Debug.Log(gameObject.name + " died");

        OnHealthChanged.Invoke();
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
