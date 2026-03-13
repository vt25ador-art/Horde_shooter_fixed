using UnityEngine;

public class ScanWallsTo3D : MonoBehaviour
{
    [Header("Scan Area")]
    [SerializeField] private Vector2 origin = Vector2.zero;
    [SerializeField] private int width = 20;
    [SerializeField] private int height = 40;
    [SerializeField] private float cellSize = 1f;

    [Header("Layers")]
    [SerializeField] private LayerMask wallLayer;

    [Header("3D Prefabs")]
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject floorPrefab;

    [Header("3D Settings")]
    [SerializeField] private Transform parent;
    [SerializeField] private bool clearOld = true;
    [SerializeField] private float wallHeight = 2f;
    [SerializeField] private float floorY = 0f;

    [SerializeField] private bool built;

    public bool IsBuilt => built;

    [ContextMenu("Build 3D")]
    public void Build3D()
    {
        if (built) return;

        if (parent == null)
            parent = transform;

        if (clearOld)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
                DestroyImmediate(parent.GetChild(i).gameObject);
        }

        BuildFloor();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 cell2D = origin + new Vector2(
                    x * cellSize + cellSize * 0.5f,
                    y * cellSize + cellSize * 0.5f
                );

                bool hasWall = Physics2D.OverlapBox(
                    cell2D,
                    Vector2.one * (cellSize * 0.8f),
                    0f,
                    wallLayer
                );

                if (!hasWall || wallPrefab == null) continue;

                Vector3 cell3D = new Vector3(cell2D.x, floorY + wallHeight * 0.5f, cell2D.y);

                GameObject wall = Instantiate(wallPrefab, cell3D, Quaternion.identity, parent);
                wall.transform.localScale = new Vector3(cellSize, wallHeight, cellSize);
            }
        }

        built = true;
    }

    public void Clear3D()
    {
        if (parent == null) return;

        for (int i = parent.childCount - 1; i >= 0; i--)
            Destroy(parent.GetChild(i).gameObject);

        built = false;
    }

    void BuildFloor()
    {
        if (floorPrefab == null) return;

        Vector3 center = new Vector3(
            origin.x + width * cellSize * 0.5f,
            floorY,
            origin.y + height * cellSize * 0.5f
        );

        GameObject floor = Instantiate(floorPrefab, center, Quaternion.identity, parent);
        floor.transform.localScale = new Vector3(width * cellSize, 1f, height * cellSize);
    }
}