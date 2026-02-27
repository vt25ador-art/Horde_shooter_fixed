using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Collider2D _col;
    [SerializeField] private float _damage = 25f;

    private System.Action<Bullet> _release;
    private float _DespawnAt;
    private Collider2D _ignoreCol;


    private void Awake()
    {
        if (_rb == null) _rb = GetComponent<Rigidbody2D>();
        if (_col == null) _col = GetComponent<Collider2D>();

        // Apply settings only when components exist
        if (_rb != null)
        {
            _rb.gravityScale = 0f;
            _rb.angularDamping = 0f;
            _rb.freezeRotation = true;
        }
    }


    public void Init(System.Action<Bullet> releaseToPool) => _release = releaseToPool;

    public void Fire(Vector2 velocity, float lifetime, Collider2D ignoreCol)
    {
        if (_ignoreCol != null && _col != null)
            Physics2D.IgnoreCollision(_col, _ignoreCol, false);

        _ignoreCol = ignoreCol;
        if (_col != null && _ignoreCol != null)
            Physics2D.IgnoreCollision(_col, _ignoreCol, true);

        if (_rb != null)
            _rb.linearVelocity = velocity;

        _DespawnAt = Time.time + lifetime;
        enabled = true;
    }

    private void Update()
    {
        if (Time.time >= _DespawnAt)
            Despawn();
    }

    private void Despawn()
    {
        if (_ignoreCol != null && _col != null)
            Physics2D.IgnoreCollision(_col, _ignoreCol, false);

        _ignoreCol = null;
        _release?.Invoke(this);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (_ignoreCol != null && _col != null)
                Physics2D.IgnoreCollision(_col, _ignoreCol, false);

            _ignoreCol = null;

            var enemyHealth = collision.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(_damage);
            }
            else
            {
                var hc = collision.GetComponent<EnemyHealth>();
                if (hc != null)
                    hc.TakeDamage(_damage);
                else 
                    Destroy(collision.gameObject);
            }

            Despawn();
        }
    }
}
