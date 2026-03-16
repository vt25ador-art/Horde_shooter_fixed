using System.Collections.Generic;
using UnityEngine;

public class SpawnDirector : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] Camera cam;

    [Header("Limits")]
    [SerializeField] int maxEnemies = 50;

    [Header("Timing")]
    [SerializeField] float relaxTime = 8f;
    [SerializeField] float peakTime = 14f;
    [SerializeField] float tickInterval = 1f;

    [Header("Budget")]
    [SerializeField] int relaxBudget = 0;
    [SerializeField] int peakBudget = 6;

    [SerializeField] List<SpawnNode> nodes = new();

    enum Mode { Relax, Peak }

    Mode mode;
    float modeTimer;
    float tick;

    readonly List<SpawnNode> shuffled = new();

    void Awake()
    {
        player ??= GameObject.FindWithTag("Player")?.transform;
        cam ??= Camera.main;

        if (nodes.Count == 0)
            // Replace this line:
            // nodes.AddRange(FindObjectsByType<SpawnNode>());

            // With the following line:
            nodes.AddRange(FindObjectsByType<SpawnNode>(FindObjectsSortMode.None));

        shuffled.AddRange(nodes);
        modeTimer = relaxTime;
        tick = tickInterval;
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

        // uppdatera node-cykler bara vid tick
        for (int i = 0; i < nodes.Count; i++)
            nodes[i].TickNode(tickInterval);

        ShuffleNodes();

        // testa varje node max en gång per tick
        for (int i = 0; i < shuffled.Count && budget > 0; i++)
        {
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
