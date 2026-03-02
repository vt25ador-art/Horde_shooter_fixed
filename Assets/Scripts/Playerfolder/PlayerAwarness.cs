using UnityEngine;

public class PlayerAwarness : MonoBehaviour
{
    public bool AwarePlayer { get; private set; }
    public Vector2 DirectionToPlayer { get; private set; }
    public float DistanceToPlayer { get; private set; }

    [SerializeField] private float _playerAwarenessDistance = 12f;
    private Transform _player;
    private float _distSqr;

    private void Awake()
    {
        _player = FindAnyObjectByType<PlayerController>()?.transform;
        _distSqr = _playerAwarenessDistance * _playerAwarenessDistance;
    }

    private void Update()
    {
        if (_player == null) return;

        Vector2 toPlayer = (Vector2)_player.position - (Vector2)transform.position;
        DistanceToPlayer = toPlayer.magnitude;

        AwarePlayer = toPlayer.sqrMagnitude <= _distSqr;
        DirectionToPlayer = DistanceToPlayer > 0.001f ? toPlayer / DistanceToPlayer : Vector2.zero;
    }
}
