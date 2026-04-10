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

    //ett läge som växlar mellan "relax" och "peak", där relax är lugnare med mindre budget och peak är intensivare med mer budget
    private enum Mode { Relax, Peak }

    private Mode mode;
    private float modeTimer;
    private float tick;

    //spawnnoderna shufflas varje tick för att få en mer dynamisk och oförutsägbar spawnning, istället för att alltid kolla noderna i samma ordning
    private readonly List<SpawnNode> shuffled = new();
    private float maxNodeCheckDistanceSqr;

    void Awake()
    {
        player ??= GameObject.FindWithTag("Player")?.transform;

        //kameran sätts till main camera om den inte är satt i editorn
        cam ??= Camera.main;

        if (nodes.Count == 0)
            nodes.AddRange(FindObjectsByType<SpawnNode>(FindObjectsSortMode.None));

        shuffled.AddRange(nodes);

        //mode timer startar i relax mode
        modeTimer = relaxTime;
        tick = tickInterval;

        maxNodeCheckDistanceSqr = maxNodeCheckDistance * maxNodeCheckDistance;
    }

    void Update()
    {
        //om player är null eller inga noder finns så gör inget
        if (!player || nodes.Count == 0) return;

        //om mode timer är mindre eller lika med 0 så byt mode och sätt timer till rätt tid
        if ((modeTimer -= Time.deltaTime) <= 0f)
        {
            mode = mode == Mode.Relax ? Mode.Peak : Mode.Relax;
            modeTimer = mode == Mode.Relax ? relaxTime : peakTime;
        }

        if ((tick -= Time.deltaTime) > 0f) return;
        tick = tickInterval;

        if (EnemyMovement.AliveCount >= maxEnemies) return;

        //budget är hur mycket "kostnad" vi har för att spawna fiender i det här ticket, det bestäms av vilket mode vi är i
        int budget = mode == Mode.Peak ? peakBudget : relaxBudget;
        if (budget <= 0) return;

        for (int i = 0; i < nodes.Count; i++)
            nodes[i].TickNode(tickInterval);

        ShuffleNodes();

        for (int i = 0; i < shuffled.Count && budget > 0; i++)
        {
            //om noden är null så hoppa över den
            if (shuffled[i] == null) continue;

            //vector2 diff är skillnaden i position mellan noden och player, om den är större än maxNodeCheckDistance så hoppa över noden
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
        //shufflenoden i listan "shuffled" med Fisher-Yates algoritmen för att få en slumpmässig ordning varje tick,
        //fisher yates är en effektiv algoritm för att slumpa ordningen på en lista utan bias
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }
    }
}