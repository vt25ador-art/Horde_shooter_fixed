using UnityEngine;

public class SpawnNode : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefab;

    [Header("Cycle")]
    [SerializeField] float activeFor = 10f;
    [SerializeField] float disabledFor = 5f;

    [Header("Spawn rules")]
    [SerializeField] float minDist = 10f;
    [SerializeField] float maxDist = 35f;
    [SerializeField] bool requireNotVisible = true;

    [Header("Burst")]
    [SerializeField] int burstMin = 1;
    [SerializeField] int burstMax = 4;

    float timer;
    bool active = true;

    float minDistSqr;
    float maxDistSqr;

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
        if (!active || !enemyPrefab || !player || budgetLeft <= 0)
            return 0;

        Vector3 diff = transform.position - player.position;
        float distSqr = diff.x * diff.x + diff.y * diff.y + diff.z * diff.z;

        if (distSqr < minDistSqr || distSqr > maxDistSqr)
            return 0;

        if (requireNotVisible && cam && IsVisible(cam))
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
