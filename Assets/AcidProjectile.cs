using UnityEngine;

public class AcidProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 8f;

    [Header("Acid Pool")]
    [SerializeField] private GameObject acidPoolPrefab;

    private Vector3 targetPosition;
    private bool hasTarget;

    public void SetTargetPosition(Vector3 position)
    {
        targetPosition = position;
        hasTarget = true;
    }

    private void Update()
    {
        if (!hasTarget)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );

        float distance = Vector3.Distance(transform.position, targetPosition);

        if (distance <= 0.05f)
        {
            SpawnAcidPool();
            Destroy(gameObject);
        }
    }

    private void SpawnAcidPool()
    {
        if (acidPoolPrefab != null)
            Instantiate(acidPoolPrefab, transform.position, Quaternion.identity);
    }
}

