using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject _enemyPrefab;

    [SerializeField] private float _miniumSpawnInterval;

    [SerializeField] private float _maximumSpawnTime;

    [SerializeField, Min(0)] private int _maxEnemies = 50;

    [SerializeField, Min(0)] private float _disableAfterSeconds = 10f;

    [SerializeField, Min(0)] private float _disableDurationSeconds = 5f;

    private float _TimeUntilSpawn;

    private void Awake()
    {
        SetTimeUntilSpawn();
    }

    private void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
        _TimeUntilSpawn -= Time.deltaTime;

        if (_TimeUntilSpawn <= 0f)
        {
            int currentEnemies = FindObjectsOfType<EnemyMovement>().Length;

            if (currentEnemies < _maxEnemies)
            {
                Instantiate(_enemyPrefab, transform.position, Quaternion.identity);
            }
            SetTimeUntilSpawn();
        }
    }

    private void SetTimeUntilSpawn()
    {
        _TimeUntilSpawn = Random.Range(_miniumSpawnInterval, _maximumSpawnTime);
    }
}
