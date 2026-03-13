using UnityEngine;

public class SimpleRaycaster : MonoBehaviour
{
    [Header("Map")]
    public int mapWidth = 10;
    public int mapHeight = 10;
    public int[,] map =
    {
        {1,1,1,1,1,1,1,1,1,1,1,1,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,1,1,1,1,1,1,1,1,1,1,1,1},
    };

    [Header("Player")]
    public Vector2 playerPos = new Vector2(2.5f, 2.5f);
    public float playerAngle = 0f;
    public float moveSpeed = 3f;
    public float turnSpeed = 120f;

    [Header("Raycasting")]
    public int rayCount = 320;
    public float fov = 60f;
    public float maxDistance = 20f;
    public float stepSize = 0.02f;

    [Header("Render")]
    public int screenWidth = 640;
    public int screenHeight = 360;
    public Color wallColor = Color.white;
    public Color floorColor = new Color(0.15f, 0.15f, 0.15f);
    public Color ceilingColor = new Color(0.3f, 0.3f, 0.3f);

    Texture2D tex;

    void Start()
    {
        tex = new Texture2D(screenWidth, screenHeight, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
    }

    void Update()
    {
        HandleInput();
        RenderView();
    }

    void HandleInput()
    {
        float move = 0f;
        float turn = 0f;

        if (Input.GetKey(KeyCode.W)) move += 1f;
        if (Input.GetKey(KeyCode.S)) move -= 1f;
        if (Input.GetKey(KeyCode.A)) turn -= 1f;
        if (Input.GetKey(KeyCode.D)) turn += 1f;

        playerAngle += turn * turnSpeed * Time.deltaTime;

        Vector2 forward = new Vector2(
            Mathf.Cos(playerAngle * Mathf.Deg2Rad),
            Mathf.Sin(playerAngle * Mathf.Deg2Rad)
        );

        Vector2 newPos = playerPos + forward * move * moveSpeed * Time.deltaTime;

        if (!IsWall(newPos.x, playerPos.y)) playerPos.x = newPos.x;
        if (!IsWall(playerPos.x, newPos.y)) playerPos.y = newPos.y;
    }

    void RenderView()
    {
        ClearTexture();

        float halfFov = fov * 0.5f;

        for (int x = 0; x < rayCount; x++)
        {
            float t = x / (float)(rayCount - 1);
            float rayAngle = playerAngle - halfFov + fov * t;

            float dist = CastRay(rayAngle);

            // fisheye correction
            float correctedDist = dist * Mathf.Cos((rayAngle - playerAngle) * Mathf.Deg2Rad);
            correctedDist = Mathf.Max(0.0001f, correctedDist);

            int columnHeight = Mathf.RoundToInt(screenHeight / correctedDist);
            int drawStart = Mathf.Max(0, (screenHeight - columnHeight) / 2);
            int drawEnd = Mathf.Min(screenHeight - 1, drawStart + columnHeight);

            // shading by distance
            float shade = Mathf.Clamp01(1f - correctedDist / maxDistance);
            Color shadedWall = wallColor * shade;

            int texX = Mathf.RoundToInt(x / (float)rayCount * screenWidth);
            int texXNext = Mathf.RoundToInt((x + 1) / (float)rayCount * screenWidth);

            for (int sx = texX; sx < texXNext; sx++)
            {
                for (int y = 0; y < screenHeight; y++)
                {
                    if (y < drawStart)
                        tex.SetPixel(sx, y, ceilingColor);
                    else if (y > drawEnd)
                        tex.SetPixel(sx, y, floorColor);
                    else
                        tex.SetPixel(sx, y, shadedWall);
                }
            }
        }

        tex.Apply();
    }

    float CastRay(float angleDeg)
    {
        float angleRad = angleDeg * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

        Vector2 pos = playerPos;
        float dist = 0f;

        while (dist < maxDistance)
        {
            pos += dir * stepSize;
            dist += stepSize;

            if (IsWall(pos.x, pos.y))
                return dist;
        }

        return maxDistance;
    }

    bool IsWall(float x, float y)
    {
        int mx = Mathf.FloorToInt(x);
        int my = Mathf.FloorToInt(y);

        if (mx < 0 || my < 0 || mx >= mapWidth || my >= mapHeight)
            return true;

        return map[my, mx] == 1;
    }

    void ClearTexture()
    {
        Color[] pixels = new Color[screenWidth * screenHeight];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.black;

        tex.SetPixels(pixels);
    }

    void OnGUI()
    {
        if (tex != null)
            GUI.DrawTexture(new Rect(0, 0, screenWidth, screenHeight), tex);

        GUI.Label(new Rect(10, screenHeight + 10, 300, 20),
            $"Pos: {playerPos}  Angle: {playerAngle:F1}");
    }
}
