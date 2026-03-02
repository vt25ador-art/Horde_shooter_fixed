using UnityEngine;

public class SpawnNode : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;

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

    private float _timer;
    private bool _active;

    public bool IsActive => _active;

    private void Awake()
    {
        _active = true;
        _timer = activeFor;
    }

    private void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer > 0f) return;

        _active = !_active;
        _timer = _active ? activeFor : disabledFor;
    }

    public int TrySpawn(Transform player, Camera cam, int budgetLeft)
    {
        if (!_active) return 0;
        if (enemyPrefab == null || player == null) return 0;
        if (budgetLeft <= 0) return 0;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist < minDist || dist > maxDist) return 0;

        if (requireNotVisible && cam != null && IsVisible(cam, transform.position)) return 0;

        int count = Mathf.Min(Random.Range(burstMin, burstMax + 1), budgetLeft);
        for (int i = 0; i < count; i++)
            Instantiate(enemyPrefab, transform.position, Quaternion.identity);

        return count;
    }

    private bool IsVisible(Camera cam, Vector3 pos)
    {
        Vector3 v = cam.WorldToViewportPoint(pos);
        return v.z > 0 && v.x > 0 && v.x < 1 && v.y > 0 && v.y < 1;
    }
}
