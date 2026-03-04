using UnityEngine;
using UnityEngine.Events;

public class HealthController : MonoBehaviour
{
    [SerializeField] private float _currentHealth;

    [SerializeField] private float _maxiumHealth;

    public bool isDowned {  get; private set; }

    [SerializeField] private float downTime = 20f;
    private float downTimer;

    public float ReimainingHealthPercentage
    {
        get
        {
            return _currentHealth / _maxiumHealth;
        }
    }

    public UnityEvent OnDied;

    public UnityEvent OnHealthChanged;


    void Update()
    {
        if (!isDowned) return;

        downTimer -= Time.deltaTime;

        if (downTimer <= 0)
        {
            Die();
        }
    }
    void Die()
    {
        Debug.Log("Player died");

        OnDied.Invoke();
    }


    public void TakeDamage(float damageAmount)
    {
        if (_currentHealth < 0)
        {
            EnterDownState();
        }

        void EnterDownState()
        {
            if (isDowned) return;

            isDowned = true;
            downTimer = downTime;

            Debug.Log("Player is DOWN!");
        }

        _currentHealth -= damageAmount;

        OnHealthChanged.Invoke();

        if (_currentHealth < 0)
        {
            _currentHealth = 0;
        }

        if(_currentHealth == 0)
        {
            OnDied.Invoke();
        }

    }

    public void AddHealth(float amountToAdd)
    {
        if (_currentHealth == _maxiumHealth)
        {
            return;
        }

        _currentHealth += amountToAdd;

        OnHealthChanged.Invoke();

        if (_currentHealth > _maxiumHealth)
        {
            _currentHealth = _maxiumHealth;
        }
    }

    public void Revive(float revivehealth)
    {
        if (!isDowned) return;

        _currentHealth = revivehealth;

        Debug.Log("Player revived");
    }
}
