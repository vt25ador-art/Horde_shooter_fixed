using UnityEngine;

public class FogOfWarGrid2D : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Map Area")]
    [SerializeField] private Vector2 mapCenter;
    [SerializeField] private Vector2 mapSize = new Vector2(50, 50);

    [Header("Fog Grid")]
    [SerializeField] private Sprite fogSprite;
    [SerializeField] private float cellSize = 1f;

    [Header("Vision")]
    [SerializeField] private float visionRadius = 8f;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float updateRate = 0.1f;

    [Header("Fog Alpha")]
    [Range(0f, 1f)]
    [SerializeField] private float hiddenAlpha = 0.90f;

    [Range(0f, 1f)]
    [SerializeField] private float exploredAlpha = 0.45f;

    [Range(0f, 1f)]
    [SerializeField] private float visibleAlpha = 0f;

    [SerializeField] private bool rememberExploredArea = true;
    [SerializeField] private float fadeSpeed = 6f;

    [Header("Sorting")]
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int orderInLayer = 100;

    private FogCell[,] cells;
    private int width;
    private int height;
    private float timer;

    private class FogCell
    {
        public SpriteRenderer renderer;
        public Vector2 worldPosition;
        public bool explored;
        public float targetAlpha;
    }

    private void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
                target = player.transform;
        }

        GenerateFog();
        UpdateVisibility();
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            timer = updateRate;
            UpdateVisibility();
        }

        FadeFog();
    }

    private void GenerateFog()
    {
        if (fogSprite == null)
        {
            Debug.LogError("FogOfWarGrid2D saknar fogSprite!");
            return;
        }

        width = Mathf.CeilToInt(mapSize.x / cellSize);
        height = Mathf.CeilToInt(mapSize.y / cellSize);

        cells = new FogCell[width, height];

        Vector2 start = mapCenter - mapSize * 0.5f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 pos = start + new Vector2(
                    x * cellSize + cellSize * 0.5f,
                    y * cellSize + cellSize * 0.5f
                );

                GameObject cellObj = new GameObject("FogCell_" + x + "_" + y);
                cellObj.transform.SetParent(transform);
                cellObj.transform.position = pos;

                SpriteRenderer sr = cellObj.AddComponent<SpriteRenderer>();
                sr.sprite = fogSprite;
                sr.color = new Color(0f, 0f, 0f, hiddenAlpha);
                sr.sortingLayerName = sortingLayerName;
                sr.sortingOrder = orderInLayer;

                float spriteWorldSize = fogSprite.bounds.size.x;
                float scale = cellSize / spriteWorldSize;
                cellObj.transform.localScale = new Vector3(scale, scale, 1f);

                FogCell cell = new FogCell();
                cell.renderer = sr;
                cell.worldPosition = pos;
                cell.explored = false;
                cell.targetAlpha = hiddenAlpha;

                cells[x, y] = cell;
            }
        }
    }

    private void UpdateVisibility()
    {
        if (target == null || cells == null)
            return;

        Vector2 eyePosition = target.position;
        float visionRadiusSqr = visionRadius * visionRadius;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                FogCell cell = cells[x, y];

                Vector2 toCell = cell.worldPosition - eyePosition;
                float distSqr = toCell.sqrMagnitude;

                bool visible = false;

                if (distSqr <= visionRadiusSqr)
                {
                    float distance = Mathf.Sqrt(distSqr);
                    Vector2 direction = toCell.normalized;

                    RaycastHit2D hit = Physics2D.Raycast(
                        eyePosition,
                        direction,
                        distance,
                        wallLayer
                    );

                    visible = hit.collider == null;
                }

                if (visible)
                {
                    cell.explored = true;
                    cell.targetAlpha = visibleAlpha;
                }
                else
                {
                    if (rememberExploredArea && cell.explored)
                        cell.targetAlpha = exploredAlpha;
                    else
                        cell.targetAlpha = hiddenAlpha;
                }
            }
        }
    }

    private void FadeFog()
    {
        if (cells == null)
            return;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                FogCell cell = cells[x, y];

                if (cell.renderer == null)
                    continue;

                Color c = cell.renderer.color;

                c.a = Mathf.MoveTowards(
                    c.a,
                    cell.targetAlpha,
                    fadeSpeed * Time.deltaTime
                );

                cell.renderer.color = c;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(mapCenter, mapSize);

        if (target != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(target.position, visionRadius);
        }
    }
}
