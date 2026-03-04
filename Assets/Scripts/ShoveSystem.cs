using UnityEngine;

public class PlayerShove : MonoBehaviour
{
    [SerializeField] private float shoveRadius = 2f;
    [SerializeField] private float shoveForce = 6f;
    [SerializeField] private LayerMask enemyLayer;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Shove();
        }
    }

    void Shove()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            transform.position,
            shoveRadius,
            enemyLayer
        );

        Debug.Log("Enemies found:" + enemies.Length);

        foreach (Collider2D enemy in enemies)
        {
            EnemyMovement em = enemy.GetComponent<EnemyMovement>();
            if (em != null)
            {
                 Vector2 direction = (enemy.transform.position - transform.forward).normalized;
                em.ApplyShove(direction * shoveForce, 0.4f);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shoveRadius);
    }
}