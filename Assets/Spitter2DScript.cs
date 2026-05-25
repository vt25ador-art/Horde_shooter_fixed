using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
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

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 540f;

    private Rigidbody2D rb;
    private float attackTimer;
    private Vector2 moveDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.angularVelocity = 0f;
        rb.freezeRotation = true;
    }

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

        float distance = Vector2.Distance(rb.position, target.position);

        CalculateMovement(distance);
        HandleAttack(distance);
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveDirection * moveSpeed;

        if (target != null)
        {
            Vector2 toTarget = (Vector2)target.position - rb.position;
            RotateTowards(toTarget);
        }
    }

    private void CalculateMovement(float distance)
    {
        Vector2 direction = ((Vector2)target.position - rb.position).normalized;

        moveDirection = Vector2.zero;

        if (distance > stopDistance)
        {
            moveDirection = direction;
        }
        else if (distance < retreatDistance)
        {
            moveDirection = -direction;
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

    private void RotateTowards(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.001f)
            return;

        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        float angle = Mathf.MoveTowardsAngle(rb.rotation, targetAngle, rotationSpeed * Time.fixedDeltaTime);

        rb.MoveRotation(angle);
    }
}
