using System.Collections.Generic;
using UnityEngine;

public class SpawnDirector : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Camera cam;

    [Header("Global cap")]
    [SerializeField] private int maxEnemies = 50;

    [Header("Pacing")]
    [SerializeField] private float relaxSeconds = 8f;
    [SerializeField] private float peakSeconds = 14f;

    [Header("Budget")]
    [SerializeField] private int budgetPerTickRelax = 0;
    [SerializeField] private int budgetPerTickPeak = 6;
    [SerializeField] private float tickInterval = 1.0f;

    [Header("Nodes")]
    [SerializeField] private List<SpawnNode> nodes = new();

    private enum Mode { Relax, Peak }
    private Mode _mode = Mode.Relax;
    private float _modeTimer;

    private float _tick;

    private void Awake()
    {
        if (player == null) player = GameObject.FindWithTag("Player")?.transform;
        if (cam == null) cam = Camera.main;

        if (nodes.Count == 0)
            nodes.AddRange(FindObjectsOfType<SpawnNode>());

        _modeTimer = relaxSeconds;
    }

    private void Update()
    {
        if (player == null || nodes.Count == 0) return;

        // mode timer
        _modeTimer -= Time.deltaTime;
        if (_modeTimer <= 0f)
        {
            _mode = _mode == Mode.Relax ? Mode.Peak : Mode.Relax;
            _modeTimer = _mode == Mode.Relax ? relaxSeconds : peakSeconds;
        }

        // tick
        _tick -= Time.deltaTime;
        if (_tick > 0f) return;
        _tick = tickInterval;

        if (EnemyMovement.AliveCount >= maxEnemies) return;

        int budget = _mode == Mode.Peak ? budgetPerTickPeak : budgetPerTickRelax;
        if (budget <= 0) return;

        // försök spawna från några noder per tick
        // (random ordning)
        for (int i = 0; i < nodes.Count && budget > 0; i++)
        {
            var node = nodes[Random.Range(0, nodes.Count)];
            budget -= node.TrySpawn(player, cam, budget);
        }
    }
}
