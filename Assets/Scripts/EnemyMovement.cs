using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMovement : MonoBehaviour
{
    public static int AliveCount { get; private set; }

    [Header("Base movement")]
    [SerializeField] private float _speed = 3f;
    [SerializeField] private float _rotationSpeed = 540f;

    [Header("Pressure (L4D-ish)")]
    [SerializeField] private float _boostStartDistance = 7f;
    [SerializeField] private float _stopDistance = 0.9f;
    [SerializeField] private float _maxBoost = 1.5f; // nära => speed * _maxBoost

    [Header("Steering noise")]
    [SerializeField] private float _steerNoiseStrength = 0.25f; // lite “drift”
    [SerializeField] private float _steerNoiseSpeed = 1.2f;

    [Header("Separation")]
    [SerializeField] private float _neighborRange = 1.5f;
    [SerializeField] private float _seperationStrength = 1.0f;
    [SerializeField, Min(4)] private int _maxNeighbors = 8;
    [SerializeField] private LayerMask _enemyMask;

    private Rigidbody2D _rb;
    private PlayerAwarness _aw;
    private Vector2 _targetDir;

    private Collider2D[] _hits;
    private float _neighborRangeSqr;
    private float _seed;

    private void OnEnable() => AliveCount++;
    private void OnDisable() => AliveCount--;
    private void OnDestroy() => AliveCount--;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _aw = GetComponent<PlayerAwarness>();

        _maxNeighbors = Mathf.Max(4, _maxNeighbors);
        _hits = new Collider2D[_maxNeighbors];
        _neighborRangeSqr = _neighborRange * _neighborRange;

        _seed = Random.value * 1000f;
    }

    private void OnValidate()
    {
        _neighborRange = Mathf.Max(0f, _neighborRange);
        _neighborRangeSqr = _neighborRange * _neighborRange;
        _maxNeighbors = Mathf.Max(4, _maxNeighbors);
        _stopDistance = Mathf.Max(0f, _stopDistance);
        _boostStartDistance = Mathf.Max(_stopDistance + 0.01f, _boostStartDistance);
    }

    private void FixedUpdate()
    {
        UpdateTargetDirection();
        RotateTowardsTarget();
        ApplyVelocity();
    }

    private void UpdateTargetDirection()
    {
        Vector2 desire = Vector2.zero;

        if (_aw != null && _aw.AwarePlayer)
        {
            if (_aw.DistanceToPlayer > _stopDistance)
                desire = _aw.DirectionToPlayer;
        }

        Vector2 sep = ComputeSeparation();
        Vector2 noise = ComputeSteerNoise();

        Vector2 combined = desire + sep * _seperationStrength + noise * _steerNoiseStrength;

        _targetDir = combined.sqrMagnitude > 1e-6f ? combined.normalized : Vector2.zero;
    }

    private Vector2 ComputeSeparation()
    {
        int count = Physics2D.OverlapCircleNonAlloc(_rb.position, _neighborRange, _hits, _enemyMask);
        Vector2 sep = Vector2.zero;
        int considered = 0;

        for (int i = 0; i < count; i++)
        {
            var c = _hits[i];
            if (!c) continue;
            if (c.attachedRigidbody == _rb) continue;

            if (!c.TryGetComponent<EnemyMovement>(out var other)) continue;

            Vector2 diff = _rb.position - (Vector2)other.transform.position;
            float sq = diff.sqrMagnitude;
            if (sq <= Mathf.Epsilon || sq > _neighborRangeSqr) continue;

            sep += diff / sq;
            considered++;
        }

        if (considered > 0)
        {
            sep /= considered;
            sep = Vector2.ClampMagnitude(sep, 1f);
        }

        return sep;
    }

    private Vector2 ComputeSteerNoise()
    {
        // enkel “perlin” drift: ger lite variation och orbit-känsla
        float t = Time.time * _steerNoiseSpeed;
        float nx = Mathf.PerlinNoise(_seed, t) - 0.5f;
        float ny = Mathf.PerlinNoise(_seed + 19.1f, t) - 0.5f;
        return new Vector2(nx, ny);
    }

    private void RotateTowardsTarget()
    {
        if (_targetDir.sqrMagnitude <= 1e-6f) return;

        float targetAngle = Mathf.Atan2(_targetDir.y, _targetDir.x) * Mathf.Rad2Deg - 90f;
        float newAngle = Mathf.MoveTowardsAngle(_rb.rotation, targetAngle, _rotationSpeed * Time.fixedDeltaTime);
        _rb.MoveRotation(newAngle);
    }

    private void ApplyVelocity()
    {
        if (_targetDir.sqrMagnitude <= 1e-6f)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        float speed = _speed;

        if (_aw != null && _aw.AwarePlayer)
        {
            float dist = _aw.DistanceToPlayer;
            if (dist < _boostStartDistance)
            {
                float t = Mathf.InverseLerp(_boostStartDistance, _stopDistance, dist);
                speed *= Mathf.Lerp(1f, _maxBoost, t);
            }
        }

        _rb.linearVelocity = _targetDir * speed;
    }
}
