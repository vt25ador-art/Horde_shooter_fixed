using UnityEngine;
using System.Collections;

public class HordeEventController : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private SpawnNode[] hordeNodes;

    [Header("Gate")]
    [SerializeField] private DoorOpen gateToOpen;

    [Header("Rules")]
    [SerializeField] private float eventDuration = 20f;
    [SerializeField] private bool useKillTarget = false;
    [SerializeField] private int killsRequired = 15;

    [Header("Fail-safe")]
    [SerializeField] private bool useMaxDurationEvenWithKillTarget = true;
    [SerializeField] private float maxDurationWithKillTarget = 60f;

    [Header("After Event")]
    [SerializeField] private SpawnNode[] normalNodesToEnableAfterEvent;


    private bool eventRunning;
    private int currentKills;
    private Coroutine eventRoutine;

    public bool EventRunning => eventRunning;
    public int CurrentKills => currentKills;
    public int KillsRequired => killsRequired;

    public bool StartHordeEvent()
    {
        if (eventRunning)
            return false;

        eventRoutine = StartCoroutine(HordeEventRoutine());
        return true;
    }

    private IEnumerator HordeEventRoutine()
    {
        eventRunning = true;
        currentKills = 0;

        SetHordeNodesActive(true);

        if (gateToOpen != null)
            gateToOpen.CloseDoor();

        float startTime = Time.time;

        if (useKillTarget)
        {
            while (currentKills < killsRequired)
            {
                if (useMaxDurationEvenWithKillTarget)
                {
                    float elapsed = Time.time - startTime;

                    if (elapsed >= maxDurationWithKillTarget)
                        break;
                }

                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(eventDuration);
        }

        CompleteEvent();
    }

    private void CompleteEvent()
    {
        SetHordeNodesActive(false);

        EnableNormalNodeAfterEvent();

        if (gateToOpen != null)
            gateToOpen.OpenDoor();

        eventRunning = false;
        eventRoutine = null;
    }

    private void SetHordeNodesActive(bool state)
    {
        if (hordeNodes == null)
            return;

        for (int i = 0; i < hordeNodes.Length; i++)
        {
            if (hordeNodes[i] != null)
                hordeNodes[i].SetForcedActive(state);
        }
    }

    private void EnableNormalNodeAfterEvent()
    {
        if (normalNodesToEnableAfterEvent == null)
            return;
        for (int i = 0; i < normalNodesToEnableAfterEvent.Length; i++)
        {
            if (normalNodesToEnableAfterEvent[i] != null && !normalNodesToEnableAfterEvent[i].IsHordeNode)
                normalNodesToEnableAfterEvent[i].EnableSpawn();

        }
    }

    public void RegisterKill()
    {
        if (!eventRunning)
            return;

        currentKills++;

        if (useKillTarget && currentKills >= killsRequired)
        {
            if (eventRoutine != null)
            {
                StopCoroutine(eventRoutine);
                eventRoutine = null;
            }

            CompleteEvent();
        }
    }

    public void ForceStopEvent()
    {
        if (!eventRunning)
            return;

        if (eventRoutine != null)
        {
            StopCoroutine(eventRoutine);
            eventRoutine = null;
        }

        CompleteEvent();
    }

    private void OnValidate()
    {
        eventDuration = Mathf.Max(0f, eventDuration);
        maxDurationWithKillTarget = Mathf.Max(0f, maxDurationWithKillTarget);
        killsRequired = Mathf.Max(1, killsRequired);
    }
}
