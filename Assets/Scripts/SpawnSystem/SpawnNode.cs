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
        //validera att minDist och maxDist är inte negativa och att maxDist är inte mindre än minDist, och cachea deras kvadrater för att optimera avståndsberäkningarna i TrySpawn
        minDist = Mathf.Max(0f, minDist);
        maxDist = Mathf.Max(minDist, maxDist);
        minDistSqr = minDist * minDist;
        maxDistSqr = maxDist * maxDist;
    }

    public void TickNode(float dt)
    {
        //publik tick som kallas av SpawnDirector varje frame för att uppdatera nodens timer och växla mellan active och inactive när timer når 0
        timer -= dt;
        if (timer > 0f) return;

        active = !active;
        timer = active ? activeFor : disabledFor;
    }

    public int TrySpawn(Transform player, Camera cam, int budgetLeft)
    {
        //testa spawnlogiken och returnera antalet fiender som spawnades, eller 0 om inga spawnades. Vi kollar först om noden är aktiv, sen om prefab, player och budget är giltiga, sen om dörren krävs och inte är upplåst, sen avståndet till spelaren, och slutligen om den inte får vara synlig i kameran.
        if (!active) return 0;
        if (enemyPrefab == null || player == null || budgetLeft <= 0) return 0;

        // NYTT: blockera spawn om dörren till området inte är upplåst
        if (requiredDoor != null && !requiredDoor.IsUnlocked)
            return 0;

        Vector3 diff = transform.position - player.position;
        float distSqr = diff.sqrMagnitude;

        if (distSqr < minDistSqr || distSqr > maxDistSqr)
            return 0;

        //denna är valfri eftersom det kan vara frustrerande att spawnas på
        if (requireNotVisible && cam != null && IsVisible(cam))
            return 0;

        //int count är ett slumpmässigt tal mellan burstMin och burstMax
        int count = Mathf.Min(Random.Range(burstMin, burstMax + 1), budgetLeft);
        Vector3 pos = transform.position;

        for (int i = 0; i < count; i++)
            Instantiate(enemyPrefab, pos, Quaternion.identity);

        return count;
    }


    private void OnDrawGizmosSelected()
    {
        //debug i editor som visar spawnområdet med två sfärer, en gul för minDist och en röd för maxDist
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minDist);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxDist);
    }


    bool IsVisible(Camera cam)
    {
        //är den synlig i kameran? Vi kollar det genom att konvertera spawn nodens position till viewport coordinates, där x och y mellan 0 och 1 betyder att den är inom skärmen, och z > 0 betyder att den är framför kameran.
        Vector3 v = cam.WorldToViewportPoint(transform.position);
        return v.z > 0f && v.x > 0f && v.x < 1f && v.y > 0f && v.y < 1f;
    }
}
