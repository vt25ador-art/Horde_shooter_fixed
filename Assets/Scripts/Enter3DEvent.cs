using UnityEngine;

public class Enter3DEvent : MonoBehaviour
{
    [Header("Requirements")]
    [SerializeField] private int killsNeeded = 20;

    [Header("Systems")]
    [SerializeField] private ScanWallsTo3D scanner;
    [SerializeField] private GameObject player2D;
    [SerializeField] private GameObject player3D;
    [SerializeField] private Camera cam2D;
    [SerializeField] private Camera cam3D;

    [Header("Spawn")]
    [SerializeField] private Transform spawn3D;

    [Header("Optional pause")]
    [SerializeField] private bool pauseBeforeTransition = true;

    private bool triggered;

    private void Awake()
    {
        // Startläge: 2D aktiv, 3D av
        if (player3D != null)
            player3D.SetActive(false);

        if (cam2D != null)
            cam2D.enabled = true;

        if (cam3D != null)
            cam3D.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;
        if (KillScore.Instance == null) return;
        if (KillScore.Instance.Kills < killsNeeded) return;

        triggered = true;
        Start3DEvent();
    }

    public void Start3DEvent()
    {
        if (pauseBeforeTransition)
            Time.timeScale = 0f;

        if (scanner != null && !scanner.IsBuilt)
            scanner.Build3D();

        if (player3D != null)
            player3D.SetActive(true);

        if (spawn3D != null && player3D != null)
            player3D.transform.position = spawn3D.position;

        if (cam3D != null)
        {
            cam3D.gameObject.SetActive(true);
            cam3D.enabled = true;
        }

        if (cam2D != null)
            cam2D.enabled = false;

        if (player2D != null)
            player2D.SetActive(false);

        Debug.Log("=== CAM3D DEBUG ===");
        Debug.Log("cam2D enabled: " + (cam2D != null && cam2D.enabled));
        Debug.Log("cam3D enabled: " + (cam3D != null && cam3D.enabled));
        Debug.Log("cam3D activeSelf: " + (cam3D != null && cam3D.gameObject.activeSelf));
        Debug.Log("cam3D activeInHierarchy: " + (cam3D != null && cam3D.gameObject.activeInHierarchy));
        Debug.Log("cam3D targetDisplay: " + (cam3D != null ? cam3D.targetDisplay.ToString() : "NULL"));
        Debug.Log("cam3D targetTexture null: " + (cam3D != null && cam3D.targetTexture == null));
        Debug.Log("player2D active: " + (player2D != null && player2D.activeSelf));
        Debug.Log("player3D active: " + (player3D != null && player3D.activeSelf));

        Time.timeScale = 1f;
    }
}
