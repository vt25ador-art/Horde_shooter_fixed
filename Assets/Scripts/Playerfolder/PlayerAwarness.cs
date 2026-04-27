using UnityEngine;

public class PlayerAwarness : MonoBehaviour
{
    public enum AwarenessMode
    {
        DistanceAndLineOfSight,
        AlwaysAware
    }

    [Header("Mode")]
    [SerializeField] private AwarenessMode awarenessMode = AwarenessMode.DistanceAndLineOfSight;

    [Header("Detection")]
    [SerializeField] private float awarenessDistance = 12f;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private Transform player;

    public bool AwarePlayer { get; private set; }
    public Vector2 DirectionToPlayer { get; private set; }
    public float DistanceToPlayer { get; private set; }

    private float awarenessDistanceSqr;

    private void Awake()
    {
        CacheValues();
        FindPlayerIfNeeded();
    }

    private void OnValidate()
    {
        CacheValues();
    }

    private void Update()
    {
        if (player == null)
        {
            FindPlayerIfNeeded();

            if (player == null)
            {
                AwarePlayer = false;
                DirectionToPlayer = Vector2.zero;
                DistanceToPlayer = 0f;
                return;
            }
        }

        Vector2 toPlayer = player.position - transform.position;
        float sqrDistance = toPlayer.sqrMagnitude;

        DistanceToPlayer = Mathf.Sqrt(sqrDistance);
        DirectionToPlayer = sqrDistance > 0.0001f ? toPlayer / DistanceToPlayer : Vector2.zero;

        if (awarenessMode == AwarenessMode.AlwaysAware)
        {
            AwarePlayer = true;
            return;
        }

        if (sqrDistance > awarenessDistanceSqr)
        {
            AwarePlayer = false;
            return;
        }

        bool blockedByWall = Physics2D.Raycast(
            transform.position,
            DirectionToPlayer,
            DistanceToPlayer,
            wallLayer
        );

        AwarePlayer = !blockedByWall;
    }

    private void FindPlayerIfNeeded()
    {
        if (player != null) return;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;
    }

    private void CacheValues()
    {
        awarenessDistance = Mathf.Max(0f, awarenessDistance);
        awarenessDistanceSqr = awarenessDistance * awarenessDistance;
    }
}
