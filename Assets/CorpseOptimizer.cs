using System.Collections.Generic;
using UnityEngine;

public class CorpseOptimizer : MonoBehaviour
{
    public static CorpseOptimizer Instance { get; private set; }

    [Header("FPS Trigger")]
    [SerializeField] private bool enableOptimizer = true;
    [SerializeField] private float lowFpsThreshold = 30f;
    [SerializeField] private float lowFpsTimeRequired = 1f;

    [Header("Cleanup")]
    [SerializeField] private float checkInterval = 0.25f;
    [SerializeField] private float destroyDelayBetweenCorpses = 0.08f;
    [SerializeField] private int minCorpsesToKeep = 10;
    [SerializeField] private bool useMenuCorpseLimit = true;
    [SerializeField] private int corpsesRemovedPerCleanup = 3;

    [Header("Debug")]
    [SerializeField] private bool debugLogs;

    private readonly List<EnemyHealth> corpses = new List<EnemyHealth>();

    private float checkTimer;
    private float lowFpsTimer;
    private float cleanupTimer;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!enableOptimizer)
            return;

        checkTimer -= Time.unscaledDeltaTime;

        if (checkTimer > 0f)
            return;

        checkTimer = checkInterval;

        float fps = 1f / Mathf.Max(Time.unscaledDeltaTime, 0.0001f);

        if (fps <= lowFpsThreshold)
        {
            lowFpsTimer += checkInterval;
        }
        else
        {
            lowFpsTimer = 0f;
        }

        if (lowFpsTimer >= lowFpsTimeRequired)
        {
            TryCleanupCorpses(fps);
        }
    }

    public static void RegisterCorpse(EnemyHealth corpse)
    {
        if (Instance == null || corpse == null)
            return;

        Instance.Register(corpse);
    }

    public static void UnregisterCorpse(EnemyHealth corpse)
    {
        if (Instance == null || corpse == null)
            return;

        Instance.corpses.Remove(corpse);
    }

    private void Register(EnemyHealth corpse)
    {
        if (corpse == null)
            return;

        if (!corpses.Contains(corpse))
            corpses.Add(corpse);
    }

    private void TryCleanupCorpses(float fps)
    {
        cleanupTimer -= checkInterval;

        if (cleanupTimer > 0f)
            return;

        cleanupTimer = destroyDelayBetweenCorpses;

        CleanNullCorpses();

        int corpseLimit = useMenuCorpseLimit ? GameSettings.CorpseLimit : minCorpsesToKeep;

        corpseLimit = Mathf.Max(0, corpseLimit);

        if (corpses.Count <= corpseLimit)
            return;

        int removeCount = Mathf.Min(corpsesRemovedPerCleanup, corpses.Count - corpseLimit);

        for (int i = 0; i < removeCount; i++)
        {
            if (corpses.Count == 0)
                return;

            EnemyHealth oldestCorpse = corpses[0];
            corpses.RemoveAt(0);

            if (oldestCorpse != null && oldestCorpse.IsDead)
            {
                if (debugLogs)
                {
                    Debug.Log(
                        "CorpseOptimizer removed corpse. FPS: " +
                        fps.ToString("0.0") +
                        " Corpses: " +
                        corpses.Count +
                        " Limit: " +
                        corpseLimit
                    );
                }

                Destroy(oldestCorpse.gameObject);
            }
        }
    }

    private void CleanNullCorpses()
    {
        for (int i = corpses.Count - 1; i >= 0; i--)
        {
            if (corpses[i] == null)
                corpses.RemoveAt(i);
        }
    }
}