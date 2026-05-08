using UnityEngine;

/// <summary>
/// Activates the Health Bar GameObject when the game starts.
/// Attach this script to the Health Bar GameObject in the Canvas.
/// </summary>
public class HealthBarActivator : MonoBehaviour
{
    private void Awake()
    {
        gameObject.SetActive(true);
    }
}
