
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
        if (triggerOnlyOnce && hasTriggered)
            return;

        if (hordeEvent == null)
        {
            Debug.LogWarning($"{name}: No HordeEventController assigned.", this);
            return;
        }

        bool started = hordeEvent.StartHordeEvent();

        if (!started)
            return;

        if (warningUI != null)
            warningUI.ShowWarning(warningMessage);

        if (triggerOnlyOnce)
            hasTriggered = true;
    }
}




