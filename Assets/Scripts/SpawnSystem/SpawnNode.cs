using UnityEngine;

public class SpawnNode : MonoBehaviour
{
    // Lista med fiender som denna spawn node får välja mellan
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("Gate")]
    // Om en dörr krävs för området så blockeras spawn tills dörren är upplåst
    [SerializeField] private DoorUnlockByKills requiredDoor;

    [Header("Cycle")]
    // Hur länge noden är aktiv innan den blir inaktiv
    [SerializeField] private float activeFor = 10f;

    // Hur länge noden är avstängd innan den blir aktiv igen
    [SerializeField] private float disabledFor = 5f;

    [Header("Spawn rules")]
    // Minsta tillåtna avstånd från spelaren för att kunna spawna
    [SerializeField] private float minDist = 10f;

    // Största tillåtna avstånd från spelaren för att kunna spawna
    [SerializeField] private float maxDist = 35f;

    // Om true får noden inte vara synlig i kameran när den spawnar
    [SerializeField] private bool requireNotVisible = true;

    [Header("Burst")]
    // Minsta antal fiender som kan spawnas vid ett spawn-tillfälle
    [SerializeField] private int burstMin = 1;

    // Högsta antal fiender som kan spawnas vid ett spawn-tillfälle
    [SerializeField] private int burstMax = 4;

    // Timer som räknar ner tills noden byter mellan aktiv/inaktiv
    private float timer;

    // Om noden just nu är aktiv eller inte
    private bool active = true;

    // Cachade kvadrerade avstånd för snabbare jämförelser
    private float minDistSqr;
    private float maxDistSqr;

    // Publik read-only property så andra script kan se om noden är aktiv
    public bool IsActive => active;

    void Awake()
    {
        // Starta noden som aktiv och sätt första timern
        timer = activeFor;

        // Cachea avstånden direkt vid start
        CacheDistances();
    }

    void OnValidate()
    {
        // Körs i editorn när värden ändras i inspectorn
        // Bra för att direkt uppdatera cacheade värden
        CacheDistances();
    }

    void CacheDistances()
    {
        // Säkerställ att minDist aldrig är negativ
        minDist = Mathf.Max(0f, minDist);

        // Säkerställ att maxDist aldrig är mindre än minDist
        maxDist = Mathf.Max(minDist, maxDist);

        // Cachea kvadraterna för att undvika onödiga sqrt-beräkningar
        minDistSqr = minDist * minDist;
        maxDistSqr = maxDist * maxDist;
    }

    public void TickNode(float dt)
    {
        // Minska timern med tiden som gått sedan förra uppdateringen
        timer -= dt;

        // Om timern inte nått 0 än gör vi inget mer
        if (timer > 0f) return;

        // Växla mellan aktiv och inaktiv
        active = !active;

        // Sätt ny timer beroende på vilket läge noden nu är i
        timer = active ? activeFor : disabledFor;
    }

    public int TrySpawn(Transform player, Camera cam, int budgetLeft)
    {
        // Om noden inte är aktiv kan den inte spawna
        if (!active) return 0;

        // Om listan saknas, är tom, player saknas eller budgeten är slut så spawnas inget
        if (enemyPrefabs == null || enemyPrefabs.Length == 0 || player == null || budgetLeft <= 0)
            return 0;

        // Om en dörr krävs men inte är upplåst så blockeras spawn
        if (requiredDoor != null && !requiredDoor.IsUnlocked)
            return 0;

        // Räkna ut avståndet mellan spawn noden och spelaren
        Vector3 diff = transform.position - player.position;
        float distSqr = diff.sqrMagnitude;

        // Spawna bara om spelaren är inom tillåtet avståndsintervall
        if (distSqr < minDistSqr || distSqr > maxDistSqr)
            return 0;

        // Om noden inte får synas i kameran, avbryt om den är synlig
        if (requireNotVisible && cam != null && IsVisible(cam))
            return 0;

        // Välj hur många fiender som ska spawnas denna gång
        // Clampas av budgetLeft så vi inte överskrider total spawnbudget
        int count = Mathf.Min(Random.Range(burstMin, burstMax + 1), budgetLeft);

        // Spawnpositionen är nodens position
        Vector3 pos = transform.position;

        // Spawna fiender en i taget
        for (int i = 0; i < count; i++)
        {
            // Välj slumpmässigt en fiendeprefab från listan
            GameObject prefabToSpawn = GetRandomEnemyPrefab();

            // Säkerhetskoll så vi inte försöker instansiera null
            if (prefabToSpawn != null)
                Instantiate(prefabToSpawn, pos, Quaternion.identity);
        }

        // Returnera hur många som faktiskt spawnades
        return count;
    }

    private GameObject GetRandomEnemyPrefab()
    {
        // Om listan saknas eller är tom, returnera null
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            return null;

        // Välj ett slumpmässigt index i arrayen
        int index = Random.Range(0, enemyPrefabs.Length);

        // Returnera den prefab som valdes
        return enemyPrefabs[index];
    }

    private void OnDrawGizmosSelected()
    {
        // Visa minsta spawnradie i gult i scenvyn
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minDist);

        // Visa största spawnradie i rött i scenvyn
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxDist);
    }

    bool IsVisible(Camera cam)
    {
        // Gör om världens position till viewport-koordinater
        // x och y mellan 0 och 1 betyder att objektet är inom kamerans vy
        // z > 0 betyder att objektet ligger framför kameran
        Vector3 v = cam.WorldToViewportPoint(transform.position);

        return v.z > 0f && v.x > 0f && v.x < 1f && v.y > 0f && v.y < 1f;
    }
}
