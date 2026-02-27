using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField]

    private float _speed;

    [SerializeField]
    private float _rotationSpeed;


    [SerializeField]
    private float _neighborRange = 1.5f;

    [SerializeField]
    private float _seperationstrength = 1.0f;

    [SerializeField, Min (4)] private int _maxNeighbors = 4;


    private Rigidbody2D _rigidbody;
    private PlayerAwarness _playerAwarness;
    private Vector2 _targetDirection;
    private Transform _transform;

    private Collider2D[] _overlapresults;

    private float _neighborRangeSqr;


    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _playerAwarness = GetComponent<PlayerAwarness>();
        _transform = transform;


        _maxNeighbors = Mathf.Max(4, _maxNeighbors);
        _overlapresults = new Collider2D[_maxNeighbors];
        _neighborRangeSqr = _neighborRange * _neighborRange;

    }


    private void OnValidate()
    {
        _neighborRange = Mathf.Max(0f, _neighborRange);
        _neighborRangeSqr = _neighborRange * _neighborRange;
        _maxNeighbors = Mathf.Max(4, _maxNeighbors);
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        UpdateTargetDirection();
        RotateTowardsTarget();
        SetVelocity();
    }



    private void UpdateTargetDirection()
    {
        Vector2 baseDirection = Vector2.zero;


        if (_playerAwarness != null && _playerAwarness.AwarePlayer)
            baseDirection = _playerAwarness.DirectionToPlayer;


        Vector2 seperation = ComputeSeparation();

        Vector2 combined = baseDirection + seperation * _seperationstrength;

        _targetDirection = (combined.sqrMagnitude <= 1e-6f) ? Vector2.zero : combined.normalized;
    }

    private Vector2 ComputeSeparation()
    {
        int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, _neighborRange, _overlapresults);
        Vector2 seperation = Vector2.zero;
        int considred = 0;

        for (int i = 0; i < hitCount; i++)
        {
            var c = _overlapresults[i];
            if (c == null) continue;
            var attached = c.attachedRigidbody;
            if (attached == null || attached.gameObject == gameObject) continue;

           if(!c.TryGetComponent<EnemyMovement>(out var other)) continue;


            Vector2 diff = (Vector2)transform.position - (Vector2)other.transform.position;
            float sqrdist = diff.sqrMagnitude;
            if (sqrdist <= Mathf.Epsilon) continue;


            seperation += diff / sqrdist;
            considred ++;
        }

        if (considred > 0)
        {
            seperation /= considred;
            seperation = Vector2.ClampMagnitude(seperation, 1f);
        }

        return seperation;
    
    }



    private void RotateTowardsTarget()
    {
        if (_targetDirection.sqrMagnitude <= 1e-6f) return;

        float targetAngle = Mathf.Atan2(_targetDirection.y, _targetDirection.x) * Mathf.Rad2Deg - 90f;
        float Newangle = Mathf.MoveTowardsAngle(_rigidbody.rotation, targetAngle, _rotationSpeed * Time.fixedDeltaTime);
        _rigidbody.MoveRotation(Newangle);
    }


    private void SetVelocity()
    {
        if (_targetDirection.sqrMagnitude <= 1e-6f)
        {
            _rigidbody.linearVelocity = Vector2.zero;
            return;
        }
        _rigidbody.linearVelocity = _targetDirection * _speed;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _neighborRange);
    }
} 
