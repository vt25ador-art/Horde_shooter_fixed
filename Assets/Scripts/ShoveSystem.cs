using UnityEngine;

public class PlayerShove : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode shoveKey = KeyCode.P;

    [Header("Shove")]
    [SerializeField] private float shoveRadius = 2f;
    [SerializeField] private float shoveForce = 6f;
    [SerializeField] private float shoveDuration = 0.4f;
    [SerializeField] private float shoveCooldown = 0.8f;

    [Header("Layers")]
    [SerializeField] private LayerMask enemyLayer;

    private float nextShoveTime;

    private readonly Collider2D[] enemyHits = new Collider2D[32];

    private void Update()
    {
        if (Input.GetKeyDown(shoveKey))
        {
            TryShove();
        }
    }

    private void TryShove()
    {
        if (Time.time < nextShoveTime)
            return;

        nextShoveTime = Time.time + shoveCooldown;

        Shove();
    }

    private void Shove()
    {
        int hitCount = Physics2D.OverlapCircleNonAlloc(
            transform.position,
            shoveRadius,
            enemyHits,
            enemyLayer
        );

        Debug.Log("Enemies found: " + hitCount);

        Vector2 playerPos = transform.position;

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D enemy = enemyHits[i];

            if (enemy == null)
                continue;

            EnemyMovement em = enemy.GetComponent<EnemyMovement>();

            if (em != null)
            {
                Vector2 direction = ((Vector2)enemy.transform.position - playerPos).normalized;

                if (direction.sqrMagnitude < 0.01f)
                    direction = transform.up;

                em.ApplyShove(direction * shoveForce, shoveDuration);
            }

            enemyHits[i] = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shoveRadius);
    }
}