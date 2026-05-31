using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BotShoot))]
public class BotAI : MonoBehaviour
{
    enum BotState { Follow, Combat, Regroup, Revive}

    [Header("Teleport Failsafe")]
    [SerializeField] private KeyCode teleportBotKey = KeyCode.T;
    [SerializeField] private float teleportDistance = 18f;
    [SerializeField] private float teleportBehindPlayerDistance = 1.5f;
    [SerializeField] private bool onlyTeleportWhenFarAway = true;


    [SerializeField] private bool autoTeleportIfVeryFar = true;
    [SerializeField] private float autoTeleportDistance = 35f;

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

    [Header("Wall Avoidance")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallcheckDistance = 0.8f;
    [SerializeField] private float avoidStrenght = 1.2f;

    [Header("Command")]
    [SerializeField] private KeyCode callBotKey = KeyCode.C;
    [SerializeField] private float regroupDuration = 6f;
    [SerializeField] private float regroupStopDistance = 1.2f;

    private BotState state = BotState.Follow;
    private float regroupTimer;

    [Header("Revive")]
    [SerializeField] private BotRevivePlayer botRevivePlayer;


    [Header("Optimization")]
    [SerializeField] private float targetScanInterval = 0.2f;

    private float nextTargetScanTime;

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
        if (Input.GetKeyDown(teleportBotKey))
        {
            TryTeleportToPlayer();
        }

        if (Input.GetKeyDown(callBotKey))
        {
            StartRegroup();
        }

        if (state == BotState.Regroup)
        {
            regroupTimer -= Time.deltaTime;

            if (regroupTimer <= 0f)
                state = BotState.Follow;

            currentTarget = null;
            return;
        }

        if (Time.time >= nextTargetScanTime)
        {
            nextTargetScanTime = Time.time + targetScanInterval;
            currentTarget = FindClosestEnemy();
        }
    }

    void FixedUpdate()
    {
        if (!player)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (autoTeleportIfVeryFar && Vector2.Distance(transform.position, player.position) > autoTeleportDistance)
        {
            TeleportToPlayer();
            return;
        }

        if (botRevivePlayer != null && botRevivePlayer.ShouldRevivePlayer)
        {
            state = BotState.Revive;
            ReviveBehaviour();
            return;
        }

        if (state == BotState.Regroup)
        {
            RegroupBehaviour();
            return;
        }

        if (shoot != null && !shoot.enabled)
            shoot.enabled = true;

        if (currentTarget)
        {
            state = BotState.Combat;
            AttackBehaviour();
        }
        else
        {
            state = BotState.Follow;
            FollowBehaviour();
        }
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
            MoveWithAvoidance(toPlayer.normalized * followSpeed);
        else if (dist < stopDistance)
            rb.linearVelocity = Vector2.zero;

        RotateTowards(toPlayer);
    }

    void StartRegroup()
    {
        state = BotState.Regroup;
        regroupTimer = regroupDuration;
        currentTarget = null;

        if (shoot != null)
            shoot.enabled = false;

        Debug.Log("Bot called to regroup");
    }

    void RegroupBehaviour()
    {
        Vector2 toPlayer = (Vector2)player.position - rb.position;
        float dist = toPlayer.magnitude;

        if (dist > regroupStopDistance)
        {
            Vector2 desiredVelocity = toPlayer.normalized * followSpeed;
            MoveWithAvoidance(desiredVelocity);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }

        RotateTowards(toPlayer);
    }


    void TryTeleportToPlayer()
    {
        if (player == null)
            return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (onlyTeleportWhenFarAway && distance < teleportDistance)
        {
            Debug.Log("Bot is not far enough to teleport. Distance: " + distance.ToString("0.0"));
            return;
        }

        TeleportToPlayer();
    }

    void TeleportToPlayer()
    {
        Vector2 playerPos = player.position;

        Vector2 behindDirection = -player.up;

        if (behindDirection.sqrMagnitude < 0.01f)
            behindDirection = Vector2.down;

        Vector2 targetPos = playerPos + behindDirection.normalized * teleportBehindPlayerDistance;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        rb.position = targetPos;
        transform.position = targetPos;

        state = BotState.Follow;
        currentTarget = null;

        if (shoot != null)
            shoot.enabled = true;

        Debug.Log("Bot teleported to player");
    }


    void ReviveBehaviour()
    {
        Vector2 toPlayer = (Vector2)botRevivePlayer.ReviveTargetPosition - rb.position;
        float dist = toPlayer.magnitude;

        if (dist > 1.2f)
        {
            Vector2 desiredVelocity = toPlayer.normalized * followSpeed;
            MoveWithAvoidance(desiredVelocity);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }

        RotateTowards(toPlayer);
    }


    void AttackBehaviour()
    {
        Vector2 toEnemy = (Vector2)currentTarget.position - rb.position;
        float dist = toEnemy.magnitude;

        if (dist > fireRange)
            MoveWithAvoidance(toEnemy.normalized * followSpeed);
        else
            rb.linearVelocity = Vector2.zero;

        RotateTowards(toEnemy);

        if (dist <= fireRange)
            shoot.BotFireAt(toEnemy);
    }



    void MoveWithAvoidance(Vector2 desiredVelocity)
    {
        if (desiredVelocity.sqrMagnitude < 0.001f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 dir = desiredVelocity.normalized;
        Vector2 move = desiredVelocity;

        RaycastHit2D frontHit = Physics2D.Raycast(rb.position, dir, wallcheckDistance, wallLayer);

        // Bara undvik om det faktiskt finns en vägg framför botten
        if (frontHit.collider != null)
        {
            Vector2 left = new Vector2(-dir.y, dir.x);
            Vector2 right = new Vector2(dir.y, -dir.x);

            bool leftBlocked = Physics2D.Raycast(rb.position, left, wallcheckDistance, wallLayer);
            bool rightBlocked = Physics2D.Raycast(rb.position, right, wallcheckDistance, wallLayer);

            if (!leftBlocked)
                move = (dir + left * avoidStrenght).normalized * desiredVelocity.magnitude;
            else if (!rightBlocked)
                move = (dir + right * avoidStrenght).normalized * desiredVelocity.magnitude;
            else
                move = Vector2.zero;
        }

        rb.linearVelocity = move;

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

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, wallcheckDistance);
    }
}
