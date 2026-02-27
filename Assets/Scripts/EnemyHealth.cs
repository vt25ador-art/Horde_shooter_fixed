using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float _maxHealth = 50f;
    [SerializeField] private bool _destroyOnDeath = true;

    [SerializeField] private GameObject HitVFX;

    private float _currentHealth;
    private bool _isDead;

    private MonoBehaviour[] _behaviousToDisable;
    private Rigidbody2D _rb2d;
    private Collider2D _col2d;

    private void Awake()
    {
        var all = GetComponents<MonoBehaviour>();
        var list = new List<MonoBehaviour>(all.Length);

        foreach (var mb in all)
        {
            if (mb == this) continue;
            list.Add(mb);
        }
        _behaviousToDisable = list.ToArray();

        TryGetComponent<Rigidbody2D>(out _rb2d);
        TryGetComponent<Collider2D>(out _col2d);
    }

    private void Start()
    {
       _currentHealth = _maxHealth;
        _isDead = false;
    }


    public void TakeDamage(float amount)
    {
        if (_isDead || amount <= 0f) return;
        
        _currentHealth -= amount;
        if(_currentHealth < 0f) _currentHealth = 0f;

        //_currentHealth = Mathf.Max(0f, _currentHealth);

        SpawnHitVFX();

        if (_currentHealth <= 0f)
            Die();

    }

    public void AddHealth(float amount)
    {
        if (_isDead || amount <= 0f) return;
        _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
    }   

    public float ReimainingHealthPercentage =>  (_maxHealth <= 0f) ? 0f : (_currentHealth / _maxHealth) * 100f;


    private void SpawnHitVFX()
    {
        if (HitVFX == null)
            return;
        {
            {
                Debug.LogWarning("HitVFX is not assigned on " + gameObject.name);
            }

            var fx = Instantiate(HitVFX, transform.position, Quaternion.identity);

            var ps = fx.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                float life = main.duration + main.startLifetime.constantMax;
                Destroy(fx, life + 0.1f);
                return;
            }

            var vfx = fx.GetComponent<VisualEffect>();
            if (vfx != null)
            {
                Destroy(fx, 2f);
                return;
            }

            Destroy(fx, 2f);
        }
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        {
            for (int i = 0; i < _behaviousToDisable.Length; i++)
            {
                var mb = _behaviousToDisable[i];
                if (mb != null)
                    mb.enabled = false;
            }
        }

        if (_rb2d != null) _rb2d.simulated = false;
        if (_col2d != null) _col2d.enabled = false;

        if (_destroyOnDeath)
            Destroy(gameObject, 5f);


        //var mbs = GetComponents<MonoBehaviour>();
        //foreach (var mb in mbs)
        //{
        //if (mb != this)
        //mb.enabled = false;
        //}

        //var rb2d = GetComponent<Rigidbody2D>();
        //if (rb2d != null) rb2d.simulated = false;
        
        //var cold2d = GetComponent<Collider2D>();
        //if (cold2d != null) cold2d.enabled = false;

        //if(_destroyOnDeath)StartCoroutine(DespawnAfterDelay(5f));


        //if (_destroyOnDeath)
        //Destroy(gameObject);
    }

    //private IEnumerator DespawnAfterDelay(float delay)
    //{
        //yield return new WaitForSeconds(delay);
        //Destroy(gameObject);
    //}
}

