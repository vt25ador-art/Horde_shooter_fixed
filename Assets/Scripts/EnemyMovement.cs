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


    private Rigidbody2D _rigidbody;
    private PlayerAwarness _playerAwarness;
    private Vector2 _targetDirection;


    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _playerAwarness = GetComponent<PlayerAwarness>();
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
        {
            baseDirection = _playerAwarness.DirectionToPlayer;
        }

        Vector2 seperation = ComputeSeparation();

        Vector2 combined = baseDirection + seperation * _seperationstrength;

        if (combined == Vector2.zero)
        {
            _targetDirection = Vector2.zero;
        }
         else
        {
            _targetDirection = combined.normalized;
        }
    }

    private Vector2 ComputeSeparation()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _neighborRange);
        Vector2 seperation = Vector2.zero;
        int count = 0;

        foreach (var c in hits)
        {
            if (c == null) continue;
            if (c.attachedRigidbody == null) continue;
            if (c.attachedRigidbody.gameObject == gameObject) continue;

            var other = c.GetComponent<EnemyMovement>();
            if (other == null) continue;

            Vector2 diff = (Vector2)transform.position - (Vector2)other.transform.position;
            float dist = diff.magnitude;
            if (dist <= 0f) continue;


            seperation += diff.normalized / dist;
            count ++;
        }

        if (count > 0)
        {
            seperation /= count;
            seperation = Vector2.ClampMagnitude(seperation, 1.0f);
        }

        return seperation;

    }



    private void RotateTowardsTarget()
    {
        if (_targetDirection == Vector2.zero) return;

        float targetAngle = Mathf.Atan2(_targetDirection.y, _targetDirection.x) * Mathf.Rad2Deg - 90f;
        float Newangle = Mathf.MoveTowardsAngle(_rigidbody.rotation, targetAngle, _rotationSpeed * Time.fixedDeltaTime);
        _rigidbody.MoveRotation(Newangle);

    }


    private void SetVelocity()
    {
        if (_targetDirection == Vector2.zero)
        {
            _rigidbody.linearVelocity = Vector2.zero;
            return;
        }
        else
        {
            _rigidbody.linearVelocity = transform.up * _speed;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _neighborRange);
    }
} 
