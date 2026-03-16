using UnityEngine;

public class SpawnNode : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;

    [Header("Gate")]
    [SerializeField] private DoorUnlockByKills requiredDoor;

    //[Header("Despawn Spawn")]
    //[SerializeField] private DoorUnlockByKills despawnafteDoor;

    [Header("Cycle")]
    [SerializeField] private float activeFor = 10f;
    [SerializeField] private float disabledFor = 5f;

    [Header("Spawn rules")]
    [SerializeField] private float minDist = 10f;
    [SerializeField] private float maxDist = 35f;
    [SerializeField] private bool requireNotVisible = true;

    [Header("Burst")]
    [SerializeField] private int burstMin = 1;
    [SerializeField] private int burstMax = 4;

    private float timer;
    private bool active = true;

    private float minDistSqr;
    private float maxDistSqr;

    public bool IsActive => active;

    void Awake()
    {
        timer = activeFor;
        CacheDistances();
    }

    void OnValidate()
    {
        CacheDistances();
    }

    void CacheDistances()
    {
        minDist = Mathf.Max(0f, minDist);
        maxDist = Mathf.Max(minDist, maxDist);
        minDistSqr = minDist * minDist;
        maxDistSqr = maxDist * maxDist;
    }

    public void TickNode(float dt)
    {
        timer -= dt;
        if (timer > 0f) return;

        active = !active;
        timer = active ? activeFor : disabledFor;
    }

    public int TrySpawn(Transform player, Camera cam, int budgetLeft)
    {
        if (!active) return 0;
        if (enemyPrefab == null || player == null || budgetLeft <= 0) return 0;

        // NYTT: blockera spawn om dörren till området inte är upplåst
        if (requiredDoor != null && !requiredDoor.IsUnlocked)
            return 0;

        Vector3 diff = transform.position - player.position;
        float distSqr = diff.sqrMagnitude;

        if (distSqr < minDistSqr || distSqr > maxDistSqr)
            return 0;

        if (requireNotVisible && cam != null && IsVisible(cam))
            return 0;

        int count = Mathf.Min(Random.Range(burstMin, burstMax + 1), budgetLeft);
        Vector3 pos = transform.position;

        for (int i = 0; i < count; i++)
            Instantiate(enemyPrefab, pos, Quaternion.identity);

        return count;
    }

    bool IsVisible(Camera cam)
    {
        Vector3 v = cam.WorldToViewportPoint(transform.position);
        return v.z > 0f && v.x > 0f && v.x < 1f && v.y > 0f && v.y < 1f;
    }
}
