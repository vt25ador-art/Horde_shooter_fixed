using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject _enemyPrefab;

    [SerializeField] private float _miniumSpawnInterval;

    [SerializeField] private float _maximumSpawnTime;

    private float _TimeUntilSpawn;


    private void Awake()
    {
        SetTimeUntilSpawn();
    }
    // Update is called once per frame
    void Update()
    {
        _TimeUntilSpawn -= Time.deltaTime;

        if (_TimeUntilSpawn <= 0f)
        {
            Instantiate(_enemyPrefab, transform.position, Quaternion.identity);
            SetTimeUntilSpawn();
        }
    }

    private void SetTimeUntilSpawn()
    {
        _TimeUntilSpawn = Random.Range(_miniumSpawnInterval, _maximumSpawnTime);
    }


}
