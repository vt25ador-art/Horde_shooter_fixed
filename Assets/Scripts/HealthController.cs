using UnityEngine;
using UnityEngine.Events;

public class HealthController : MonoBehaviour
{
    [SerializeField] private float _currentHealth;

    [SerializeField] private float _maxiumHealth;


    public float ReimainingHealthPercentage
    {
        get
        {
            return _currentHealth / _maxiumHealth;
        }
    }

    public UnityEvent OnDied;

    public UnityEvent OnHealthChanged;


    public void TakeDamage(float damageAmount)
    {
        if (_currentHealth == 0)
        {
            return;
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
}
