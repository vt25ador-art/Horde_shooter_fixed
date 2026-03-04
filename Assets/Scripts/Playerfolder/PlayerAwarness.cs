using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerAwarness : MonoBehaviour
{

    private LayerMask WallLayer;
    public bool AwarePlayer { get; private set; }
    public Vector2 DirectionToPlayer { get; private set; }

    public Vector2 enemyToPlayer { get; private set; }
    public float DistanceToPlayer { get; private set; }

    [SerializeField] private float _playerAwarenessDistance = 12f;
    [SerializeField] private GameObject Player;
    private float _distSqr;



    private void Awake()
    {
        if (Player == null)
        {
            var byTag = GameObject.FindWithTag("Player");
            if (byTag != null) Player = byTag;
        }

        if (Player == null)
        {
            var pc = FindAnyObjectByType<PlayerController>();
            if (pc != null) Player = pc.gameObject;
        }

        _distSqr = _playerAwarenessDistance * _playerAwarenessDistance;
    }

    private void Update()
    {
        enemyToPlayer = Player.transform.position - transform.position;

        DirectionToPlayer = enemyToPlayer.normalized;

        float distance = enemyToPlayer.magnitude;

        if (distance <= _playerAwarenessDistance)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, DirectionToPlayer, distance, WallLayer);

            if (hit.collider == null)
            {
                AwarePlayer = true;
            }
            else
            {
                AwarePlayer = false;
            }
        }
        else
        {
            AwarePlayer = false ;
        }

        //if (_player == null) return;

        //var byTag = GameObject.FindWithTag("Player");
        //if (byTag != null) _player = byTag.transform;
        //else
        //{
            //var pc = FindAnyObjectByType<PlayerController>();
            //if (pc != null) return;
        //}
        
        //Vector2 toPlayer = (Vector2)_player.position - (Vector2)transform.position;
        //DistanceToPlayer = toPlayer.magnitude;

        //AwarePlayer = toPlayer.sqrMagnitude <= _distSqr;
        //DirectionToPlayer = DistanceToPlayer > 0.001f ? toPlayer / DistanceToPlayer : Vector2.zero;
    }

    private void OnValidate()
    {
        _playerAwarenessDistance = Mathf.Max(0f, _playerAwarenessDistance);
        _distSqr = _playerAwarenessDistance * _playerAwarenessDistance;
    }
}
