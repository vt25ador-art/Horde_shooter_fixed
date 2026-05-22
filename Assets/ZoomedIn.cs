using UnityEngine;

public class ZoomedIn : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera targetCamera;

    [Header("Zoom")]
    [SerializeField] private float insideZoom = 5f;
    [SerializeField] private float outsideZoom = 8f;
    [SerializeField] private float zoomSpeed = 3f;

    private float targetZoom;

    private void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera != null)
            targetZoom = targetCamera.orthographicSize;
    }

    private void Update()
    {
        if (targetCamera == null)
            return;

        targetCamera.orthographicSize = Mathf.Lerp(
            targetCamera.orthographicSize,
            targetZoom,
            Time.deltaTime * zoomSpeed
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        targetZoom = insideZoom;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        targetZoom = outsideZoom;
    }
}
