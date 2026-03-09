using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAwarness : MonoBehaviour
{
    [SerializeField] float awarenessDistance = 12f;
    [SerializeField] LayerMask wallLayer;
    [SerializeField] Transform player;

    public bool AwarePlayer { get; private set; }
    public Vector2 DirectionToPlayer { get; private set; }
    public float DistanceToPlayer { get; private set; }

    float distSqr;

    void Awake()
    {
        if (!player)
            player = GameObject.FindWithTag("Player")?.transform;

        distSqr = awarenessDistance * awarenessDistance;
    }

    void Update()
    {
        if (!player) return;

        Vector2 toPlayer = player.position - transform.position;
        float sqr = toPlayer.sqrMagnitude;

        DirectionToPlayer = toPlayer.normalized;
        DistanceToPlayer = Mathf.Sqrt(sqr);

        AwarePlayer = sqr <= distSqr &&
                      !Physics2D.Raycast(transform.position, DirectionToPlayer, DistanceToPlayer, wallLayer);
    }

    void OnValidate()
    {
        awarenessDistance = Mathf.Max(0f, awarenessDistance);
        distSqr = awarenessDistance * awarenessDistance;
    }
}
