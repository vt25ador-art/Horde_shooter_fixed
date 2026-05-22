using UnityEngine;

public class SpitterEnemy2D : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private string targetTag = "Player";

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stopDistance = 6f;
    [SerializeField] private float retreatDistance = 3f;

    [Header("Attack")]
    [SerializeField] private GameObject acidProjectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float attackRange = 9f;
    [SerializeField] private float attackCooldown = 3f;

    private float attackTimer;

    private void Start()
    {
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(targetTag);

            if (playerObj != null)
                target = playerObj.transform;
        }
    }

    private void Update()
    {
        if (target == null)
            return;

        float distance = Vector2.Distance(transform.position, target.position);

        HandleMovement(distance);
        HandleAttack(distance);
    }

    private void HandleMovement(float distance)
    {
        Vector2 direction = (target.position - transform.position).normalized;

        if (distance > stopDistance)
        {
            transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);
        }
        else if (distance < retreatDistance)
        {
            transform.position -= (Vector3)(direction * moveSpeed * Time.deltaTime);
        }
    }

    private void HandleAttack(float distance)
    {
        attackTimer -= Time.deltaTime;

        if (distance > attackRange)
            return;

        if (attackTimer > 0f)
            return;

        ShootAcid();

        attackTimer = attackCooldown;
    }

    private void ShootAcid()
    {
        if (acidProjectilePrefab == null)
            return;

        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;

        GameObject projectileObj = Instantiate(acidProjectilePrefab, spawnPosition, Quaternion.identity);

        AcidProjectile projectile = projectileObj.GetComponent<AcidProjectile>();

        if (projectile != null)
            projectile.SetTargetPosition(target.position);
    }
}



