using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float _maxHealth = 50f;

    [Header("Death")]
    [SerializeField] private bool _destroyOnDeath = false;
    [SerializeField] private float destroyDelay = 5f;

    [Header("Corpse Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Tooltip("Om du bara vill ha ett lik.")]
    [SerializeField] private Sprite deadSprite;

    [Tooltip("Om du vill ha flera olika lik. Om denna har sprites väljs en random.")]
    [SerializeField] private Sprite[] randomDeadSprites;

    [SerializeField] private bool useRandomCorpseRotation = true;
    [SerializeField] private float randomRotationMin = -25f;
    [SerializeField] private float randomRotationMax = 25f;

    [Header("Corpse Sorting")]
    [SerializeField] private bool changeSortingOnDeath = true;
    [SerializeField] private int corpseOrderInLayer = -1;

    [Header("VFX")]
    [SerializeField] private GameObject HitVFX;

    private float _currentHealth;
    private bool _isDead;

    private MonoBehaviour[] _behavioursToDisable;
    private Rigidbody2D _rb2d;
    private Collider2D _col2d;

    [Header("VFX Optimization")]
    [SerializeField] private float hitVfxCooldown = 0.08f;
    private float _nextHitVfxTime;

    public bool IsDead => _isDead;

    public float ReimainingHealthPercentage
    {
        get
        {
            if (_maxHealth <= 0f)
                return 0f;

            return (_currentHealth / _maxHealth) * 100f;
        }
    }

    private void Awake()
    {
        MonoBehaviour[] all = GetComponents<MonoBehaviour>();
        List<MonoBehaviour> list = new List<MonoBehaviour>(all.Length);

        foreach (MonoBehaviour mb in all)
        {
            if (mb == this)
                continue;

            list.Add(mb);
        }

        _behavioursToDisable = list.ToArray();

        TryGetComponent(out _rb2d);
        TryGetComponent(out _col2d);

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        _currentHealth = _maxHealth;
        _isDead = false;
    }

    public void TakeDamage(float amount)
    {
        if (_isDead || amount <= 0f)
            return;

        _currentHealth -= amount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0f, _maxHealth);

        SpawnHitVFX();

        if (_currentHealth <= 0f)
            Die();
    }

    public void AddHealth(float amount)
    {
        if (_isDead || amount <= 0f)
            return;

        _currentHealth = Mathf.Clamp(_currentHealth + amount, 0f, _maxHealth);
    }

    private void Die()
    {
        if (_isDead)
            return;

        _isDead = true;

        // Om special infected håller fast player, släpp den
        //JockeyEnemy2D jockey = GetComponent<JockeyEnemy2D>();
        //if (jockey != null)
            //jockey.ReleaseVictim();

        ApplyCorpseVisual();

        DisableEnemyBehaviour();

        if (KillScore.Instance != null)
            KillScore.Instance.AddKill();

        if (_destroyOnDeath)
            Destroy(gameObject, destroyDelay);

        GameStats.AddKill();
    }

    private void ApplyCorpseVisual()
    {
        if (spriteRenderer == null)
            return;

        Sprite selectedCorpse = GetCorpseSprite();

        if (selectedCorpse != null)
            spriteRenderer.sprite = selectedCorpse;

        if (useRandomCorpseRotation)
        {
            float randomZ = Random.Range(randomRotationMin, randomRotationMax);
            transform.rotation = Quaternion.Euler(0f, 0f, randomZ);
        }

        if (changeSortingOnDeath)
            spriteRenderer.sortingOrder = corpseOrderInLayer;
    }

    private Sprite GetCorpseSprite()
    {
        if (randomDeadSprites != null && randomDeadSprites.Length > 0)
        {
            int index = Random.Range(0, randomDeadSprites.Length);

            if (randomDeadSprites[index] != null)
                return randomDeadSprites[index];
        }

        return deadSprite;
    }

    private void DisableEnemyBehaviour()
    {
        for (int i = 0; i < _behavioursToDisable.Length; i++)
        {
            MonoBehaviour mb = _behavioursToDisable[i];

            if (mb != null)
                mb.enabled = false;
        }

        if (_rb2d != null)
        {
            _rb2d.linearVelocity = Vector2.zero;
            _rb2d.angularVelocity = 0f;
            _rb2d.simulated = false;
        }

        if (_col2d != null)
            _col2d.enabled = false;
    }

    private void SpawnHitVFX()
    {
        if (HitVFX == null)
            return;

        if (Time.time < _nextHitVfxTime)
            return;

        _nextHitVfxTime = Time.time + hitVfxCooldown;

        GameObject fx = Instantiate(HitVFX, transform.position, Quaternion.identity);

        ParticleSystem ps = fx.GetComponent<ParticleSystem>();

        if (ps != null)
        {
            ParticleSystem.MainModule main = ps.main;
            float life = main.duration + main.startLifetime.constantMax;
            Destroy(fx, life + 0.1f);
            return;
        }

        VisualEffect vfx = fx.GetComponent<VisualEffect>();

        if (vfx != null)
        {
            Destroy(fx, 2f);
            return;
        }

        Destroy(fx, 2f);
    }

    public void ForceKill()
    {
        if (IsDead)
            return;

        TakeDamage(999999f);
    }

}
