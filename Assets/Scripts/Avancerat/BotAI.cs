using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BotShoot))]
public class BotAI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Follow")]
    [SerializeField] private float followSpeed = 4f;
    [SerializeField] private float followDistance = 2.5f;
    [SerializeField] private float stopDistance = 1.5f;

    [Header("Combat")]
    [SerializeField] private float detectRange = 8f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float fireRange = 7f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 540f;

    private Rigidbody2D rb;
    private BotShoot shoot;
    private Transform currentTarget;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        shoot = GetComponent<BotShoot>();

        if (!player)
            player = GameObject.FindWithTag("Player")?.transform;
    }

    void Update()
    {
        currentTarget = FindClosestEnemy();
    }

    void FixedUpdate()
    {
        if (!player)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (currentTarget)
            AttackBehaviour();
        else
            FollowBehaviour();
    }

    Transform FindClosestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectRange, enemyLayer);

        Transform best = null;
        float bestSqr = float.MaxValue;
        Vector2 pos = transform.position;

        for (int i = 0; i < hits.Length; i++)
        {
            if (!hits[i]) continue;

            float sqr = ((Vector2)hits[i].transform.position - pos).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = hits[i].transform;
            }
        }

        return best;
    }

    void FollowBehaviour()
    {
        Vector2 toPlayer = (Vector2)player.position - rb.position;
        float dist = toPlayer.magnitude;

        if (dist > followDistance)
            rb.linearVelocity = toPlayer.normalized * followSpeed;
        else if (dist < stopDistance)
            rb.linearVelocity = Vector2.zero;

        RotateTowards(toPlayer);
    }

    void AttackBehaviour()
    {
        Vector2 toEnemy = (Vector2)currentTarget.position - rb.position;
        float dist = toEnemy.magnitude;

        // stå kvar och skjut om nära nog, annars gå närmare
        if (dist > fireRange)
            rb.linearVelocity = toEnemy.normalized * followSpeed;
        else
            rb.linearVelocity = Vector2.zero;

        RotateTowards(toEnemy);

        if (dist <= fireRange)
            shoot.BotFireAt(toEnemy);
    }

    void RotateTowards(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.001f) return;

        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        float angle = Mathf.MoveTowardsAngle(rb.rotation, targetAngle, rotationSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(angle);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fireRange);
    }
}
