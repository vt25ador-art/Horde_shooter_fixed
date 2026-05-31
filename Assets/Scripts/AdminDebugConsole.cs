#if UNITY_EDITOR || DEVELOPMENT_BUILD

using UnityEngine;
using UnityEngine.InputSystem;


public class AdminDebugConsole : MonoBehaviour
{
    [Header("Console")]
    [SerializeField] private Key toggleKey = Key.Backquote; // knappen under ESC: `
    [SerializeField] private bool consoleOpen;

    [Header("References")]
    [SerializeField] private SpawnDirector spawnDirector;

    private string commandInput = "";

    private bool shouldFocusInput;

    private GameObject playerObj;
    private HealthController playerHealth;
    private PlayerShoot playerShoot;
    private Collider2D playerCollider;

    private void Awake()
    {
        if (spawnDirector == null)
            spawnDirector = FindFirstObjectByType<SpawnDirector>();

        playerObj = GameObject.FindWithTag("Player");

        if (playerObj != null)
        {
            playerHealth = playerObj.GetComponent<HealthController>();
            playerShoot = playerObj.GetComponent<PlayerShoot>();
            playerCollider = playerObj.GetComponent<Collider2D>();
        }
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            consoleOpen = !consoleOpen;
            commandInput = "";

            if (consoleOpen)
                shouldFocusInput = true;
        }

        if (!consoleOpen)
            return;

        if (Keyboard.current.enterKey.wasPressedThisFrame ||
            Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            RunCommand(commandInput);
            commandInput = "";
            shouldFocusInput = true;
        }
    }

    private void OnGUI()
    {
        if (!consoleOpen)
            return;

        GUI.Box(new Rect(10, 10, 520, 100), "Admin Debug Console");

        GUI.SetNextControlName("CommandInput");
        commandInput = GUI.TextField(new Rect(20, 40, 490, 25), commandInput);

        GUI.Label(new Rect(20, 70, 490, 20), "help, god, director_stop, kill_all, clear_corpses");

        if (shouldFocusInput)
        {
            GUI.FocusControl("CommandInput");
            shouldFocusInput = false;
        }

        Event e = Event.current;

        if (e.type == EventType.KeyDown &&
            (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
        {
            RunCommand(commandInput);
            commandInput = "";
            shouldFocusInput = true;
            e.Use();
        }
    }

    private void RunCommand(string input)
    {
        input = input.Trim().ToLower();

        if (string.IsNullOrEmpty(input))
            return;

        string[] args = input.Split(' ');

        string command = args[0];

        Debug.Log("Admin command: " + input);

        switch (command)
        {
            case "director_stop":
                DirectorStop();
                break;

            case "director_start":
                DirectorStart();
                break;

            case "kill_all":
                KillAllEnemies();
                break;

            case "clear_corpses":
                ClearCorpses();
                break;

            case "fps_300":
                SetFps300();
                break;

            case "god":
                SetGodMode(true);
                break;

            case "god_off":
                SetGodMode(false);
                break;

            case "heal":
                HealPlayer();
                break;

            case "ammo":
                GiveAmmo();
                break;

            case "timescale":
                SetTimeScale(args);
                break;

            case "slowmo":
                Time.timeScale = 0.3f;
                Debug.Log("Slow motion enabled");
                break;

            case "normal_time":
                Time.timeScale = 1f;
                Debug.Log("Time scale reset");
                break;

            case "no_clip":
                SetNoClip(true);
                break;

            case "no_clip_off":
                SetNoClip(false);
                break;

            case "help":
                PrintHelp();
                break;

            default:
                Debug.LogWarning("Unknown admin command: " + command);
                break;
        }
    }

    private void DirectorStop()
    {
        if (spawnDirector != null)
        {
            spawnDirector.enabled = false;
            Debug.Log("Director stopped");
        }

        SpawnNode[] nodes = FindObjectsByType<SpawnNode>(FindObjectsSortMode.None);

        for (int i = 0; i < nodes.Length; i++)
            nodes[i].gameObject.SetActive(false);

        Debug.Log("All spawn nodes disabled");
    }

    private void DirectorStart()
    {
        SpawnNode[] nodes = FindObjectsByType<SpawnNode>(FindObjectsSortMode.None);

        for (int i = 0; i < nodes.Length; i++)
            nodes[i].gameObject.SetActive(true);

        if (spawnDirector != null)
            spawnDirector.enabled = true;

        Debug.Log("Director started");
    }


    private void SetGodMode(bool state)
    {
        GameObject playerObj = GameObject.FindWithTag("Player");

        if (playerObj == null)
        {
            Debug.LogWarning("No Player found.");
            return;
        }

        HealthController health = playerObj.GetComponent<HealthController>();

        if (health == null)
        {
            Debug.LogWarning("Player has no HealthController.");
            return;
        }

        health.SetGodMode(state);

        if (state)
            Debug.Log("GOD MODE ENABLED");
        else
            Debug.Log("GOD MODE DISABLED");
    }

    private void HealPlayer()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");

        if (playerObj == null)
            return;

        HealthController health = playerObj.GetComponent<HealthController>();

        if (health == null)
            return;

        health.AddHealth(9999f);

        Debug.Log("Player healed.");
    }

    private void GiveAmmo()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");

        if (playerObj == null)
            return;

        PlayerShoot playerShoot = playerObj.GetComponent<PlayerShoot>();

        if (playerShoot == null)
        {
            Debug.LogWarning("Player has no PlayerShoot.");
            return;
        }

        playerShoot.AddAmmo(999);

        Debug.Log("Ammo given.");
    }

    private void SetTimeScale(string[] args)
    {
        if (args.Length < 2)
        {
            Debug.LogWarning("Usage: timescale 1");
            return;
        }

        if (!float.TryParse(args[1], out float value))
        {
            Debug.LogWarning("Invalid timescale value.");
            return;
        }

        value = Mathf.Clamp(value, 0f, 5f);
        Time.timeScale = value;

        Debug.Log("Time scale set to: " + value);
    }

    private void SetNoClip(bool state)
    {
        GameObject playerObj = GameObject.FindWithTag("Player");

        if (playerObj == null)
            return;

        Collider2D col = playerObj.GetComponent<Collider2D>();

        if (col == null)
        {
            Debug.LogWarning("Player has no Collider2D.");
            return;
        }

        col.enabled = !state;

        Debug.Log(state ? "NO CLIP ENABLED" : "NO CLIP DISABLED");
    }

    private void PrintHelp()
    {
        Debug.Log(
            "Admin Commands:\n" +
            "director_stop\n" +
            "director_start\n" +
            "kill_all\n" +
            "clear_corpses\n" +
            "god\n" +
            "god_off\n" +
            "heal\n" +
            "ammo\n" +
            "timescale 0.5\n" +
            "timescale 1\n" +
            "slowmo\n" +
            "normal_time\n" +
            "no_clip\n" +
            "no_clip_off\n" +
            "fps_300"
        );
    }

    private void KillAllEnemies()
    {
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);

        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] != null && !enemies[i].IsDead)
                enemies[i].ForceKill();
        }

        Debug.Log("Killed enemies: " + enemies.Length);
    }

    private void ClearCorpses()
    {
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);

        int count = 0;

        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] != null && enemies[i].IsDead)
            {
                Destroy(enemies[i].gameObject);
                count++;
            }
        }

        Debug.Log("Cleared corpses: " + count);
    }

    private void SetFps300()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 300;

        Debug.Log("FPS target set to 300. VSync disabled.");
    }
}

#endif