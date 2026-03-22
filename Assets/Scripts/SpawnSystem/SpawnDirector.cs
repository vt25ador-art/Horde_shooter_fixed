using System.Collections.Generic;
using UnityEngine;

public class SpawnDirector : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Camera cam;

    [Header("Limits")]
    [SerializeField] private int maxEnemies = 50;

    [Header("Timing")]
    [SerializeField] private float relaxTime = 8f;
    [SerializeField] private float peakTime = 14f;
    [SerializeField] private float tickInterval = 1f;

    [Header("Budget")]
    [SerializeField] private int relaxBudget = 0;
    [SerializeField] private int peakBudget = 6;

    [Header("Global Spawn Distance")]
    [SerializeField] private float maxNodeCheckDistance = 30f;

    [SerializeField] private List<SpawnNode> nodes = new();

    private enum Mode { Relax, Peak }

    private Mode mode;
    private float modeTimer;
    private float tick;

    private readonly List<SpawnNode> shuffled = new();
    private float maxNodeCheckDistanceSqr;

    void Awake()
    {
        player ??= GameObject.FindWithTag("Player")?.transform;
        cam ??= Camera.main;

        if (nodes.Count == 0)
            nodes.AddRange(FindObjectsByType<SpawnNode>(FindObjectsSortMode.None));

        shuffled.AddRange(nodes);

        modeTimer = relaxTime;
        tick = tickInterval;

        maxNodeCheckDistanceSqr = maxNodeCheckDistance * maxNodeCheckDistance;
    }

    void Update()
    {
        if (!player || nodes.Count == 0) return;

        if ((modeTimer -= Time.deltaTime) <= 0f)
        {
            mode = mode == Mode.Relax ? Mode.Peak : Mode.Relax;
            modeTimer = mode == Mode.Relax ? relaxTime : peakTime;
        }

        if ((tick -= Time.deltaTime) > 0f) return;
        tick = tickInterval;

        if (EnemyMovement.AliveCount >= maxEnemies) return;

        int budget = mode == Mode.Peak ? peakBudget : relaxBudget;
        if (budget <= 0) return;

        for (int i = 0; i < nodes.Count; i++)
            nodes[i].TickNode(tickInterval);

        ShuffleNodes();

        for (int i = 0; i < shuffled.Count && budget > 0; i++)
        {
            if (shuffled[i] == null) continue;

            Vector2 diff = shuffled[i].transform.position - player.position;
            if (diff.sqrMagnitude > maxNodeCheckDistanceSqr)
                continue;

            int spent = shuffled[i].TrySpawn(player, cam, budget);
            if (spent > 0)
                budget -= spent;
        }
    }

    void ShuffleNodes()
    {
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }
    }
}