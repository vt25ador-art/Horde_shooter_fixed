using UnityEngine;

public class BossSpawnZone2D : MonoBehaviour
{
    [Header("Boss")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Chance")]
    [Range(0f, 1f)]
    [SerializeField] private float spawnChance = 0.5f;

    [Header("Rules")]
    [SerializeField] private bool tryOnlyOncePerZone = true;
    [SerializeField] private bool disableZoneAfterTry = true;

    private bool hasTried;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (tryOnlyOncePerZone && hasTried)
            return;

        if (BossSpawnManager.Instance != null && BossSpawnManager.Instance.BossSpawned)
            return;

        hasTried = true;

        float roll = Random.value;

        if (roll <= spawnChance)
        {
            SpawnBoss();
        }
        else
        {
            Debug.Log("Ingen boss spawnade i denna zon. Roll: " + roll);
        }

        if (disableZoneAfterTry)
            gameObject.SetActive(false);
    }

    private void SpawnBoss()
    {
        if (bossPrefab == null)
        {
            Debug.LogWarning("BossSpawnZone2D saknar bossPrefab", this);
            return;
        }

        Vector3 spawnPosition = transform.position;

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);

            if (spawnPoints[randomIndex] != null)
                spawnPosition = spawnPoints[randomIndex].position;
        }

        Instantiate(bossPrefab, spawnPosition, Quaternion.identity);

        if (BossSpawnManager.Instance != null)
            BossSpawnManager.Instance.MarkBossSpawned();

        Debug.Log("Boss zombie spawned!");
    }
}
