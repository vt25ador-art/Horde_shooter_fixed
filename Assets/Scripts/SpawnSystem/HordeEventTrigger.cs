using UnityEngine;

public class HordeEventTrigger : MonoBehaviour
{
    [SerializeField] private HordeEventController hordeEvent;
    [SerializeField] private HordeWarningUI warningUI;
    [SerializeField] private bool triggerOnlyOnce = true;

    [Header("Warning")]
    [SerializeField] private string warningMessage = "HORDE INBOUND";

    private bool hasTriggered;

    public void TriggerEvent()
    {
        Debug.Log($"{name}: TriggerEvent() called.", this);

        if (triggerOnlyOnce && hasTriggered)
        {
            Debug.Log($"{name}: Event already triggered. Ignoring.", this);
            return;
        }

        if (hordeEvent == null)
        {
            Debug.LogWarning($"{name}: No HordeEventController assigned.", this);
            return;
        }

        Debug.Log($"{name}: Trying to start horde event.", this);

        bool started = hordeEvent.StartHordeEvent();

        Debug.Log($"{name}: StartHordeEvent returned: {started}", this);

        if (!started)
        {
            Debug.Log($"{name}: Horde event did not start, probably already running.", this);
            return;
        }

        if (warningUI != null)
        {
            Debug.Log($"{name}: Showing warning UI: {warningMessage}", this);
            warningUI.ShowWarning(warningMessage);
        }
        else
        {
            Debug.LogWarning($"{name}: No HordeWarningUI assigned.", this);
        }

        if (triggerOnlyOnce)
            hasTriggered = true;

        Debug.Log($"{name}: Horde trigger completed.", this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"{name}: Something entered trigger: {other.name}", this);

        if (!other.CompareTag("Player"))
            return;

        Debug.Log($"{name}: Player entered trigger. Starting event.", this);

        TriggerEvent();
    }
}

