using UnityEngine;

public class SpawnNode : MonoBehaviour
{
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("Gate")]
    [SerializeField] private DoorUnlockByKills requiredDoor;

    [Header("Cycle")]
    [SerializeField] private float activeFor = 10f;
    [SerializeField] private float disabledFor = 5f;

    [Header("Spawn Rules")]
    [SerializeField] private float minDist = 10f;
    [SerializeField] private float maxDist = 35f;
    [SerializeField] private bool requireNotVisible = true;

    [Header("Burst")]
    [SerializeField] private int burstMin = 1;
    [SerializeField] private int burstMax = 4;

    [Header("Horde")]
    [SerializeField] private bool isHordeNode = false;

    private float timer;
    private bool active = true;
    private bool forcedActive;

    private float minDistSqr;
    private float maxDistSqr;

    //public bool IsActive => forcedActive || active;

    public bool isActive
    {
        get
        {
            if (isHordeNode)
                return forcedActive;
            return active;
        }
    }

    public bool IsHordeNode => isHordeNode;
    public bool ForcedActive => forcedActive;

    private void Awake()
    {
        timer = activeFor;
        CacheValues();
    }

    private void OnValidate()
    {
        CacheValues();
    }

    private void CacheValues()
    {
        activeFor = Mathf.Max(0.1f, activeFor);
        disabledFor = Mathf.Max(0.1f, disabledFor);

        minDist = Mathf.Max(0f, minDist);
        maxDist = Mathf.Max(minDist, maxDist);

        burstMin = Mathf.Max(1, burstMin);
        burstMax = Mathf.Max(burstMin, burstMax);

        minDistSqr = minDist * minDist;
        maxDistSqr = maxDist * maxDist;
    }

    public void TickNode(float deltaTime)
    {
        if (forcedActive)
            return;

        timer -= deltaTime;

        if (timer > 0f)
            return;

        active = !active;
        timer = active ? activeFor : disabledFor;
    }

    public void SetForcedActive(bool state)
    {
        if (!isHordeNode)
            return;

        forcedActive = state;

        if (!forcedActive)
        {
            active = false;
            timer = disabledFor;
        }
    }

    public int TrySpawn(Transform player, Camera cam, int budgetLeft)
    {
        if (!isActive)
            return 0;

        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            return 0;

        if (player == null || budgetLeft <= 0)
            return 0;

        if (requiredDoor != null && !requiredDoor.IsUnlocked)
            return 0;

        Vector3 diff = transform.position - player.position;
        float distSqr = diff.sqrMagnitude;

        if (distSqr < minDistSqr || distSqr > maxDistSqr)
            return 0;

        if (requireNotVisible && cam != null && IsVisible(cam))
            return 0;

        int amountToSpawn = Mathf.Min(Random.Range(burstMin, burstMax + 1), budgetLeft);

        for (int i = 0; i < amountToSpawn; i++)
        {
            GameObject prefab = GetRandomEnemyPrefab();

            if (prefab == null)
                continue;

            Instantiate(prefab, transform.position, Quaternion.identity);
        }

        return amountToSpawn;
    }

    private GameObject GetRandomEnemyPrefab()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            return null;

        return enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
    }

    private bool IsVisible(Camera cam)
    {
        Vector3 viewportPoint = cam.WorldToViewportPoint(transform.position);

        return viewportPoint.z > 0f &&
               viewportPoint.x > 0f &&
               viewportPoint.x < 1f &&
               viewportPoint.y > 0f &&
               viewportPoint.y < 1f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minDist);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxDist);
    }
}
