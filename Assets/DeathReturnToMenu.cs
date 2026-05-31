using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathReturnToMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthController health;

    [Header("Scene")]
    [SerializeField] private string menuSceneName = "MainMenu";
    [SerializeField] private float delayBeforeLoad = 3f;

    [Header("Optional UI")]
    [SerializeField] private GameObject deathScreen;

    private bool loading;

    private void Awake()
    {
        if (health == null)
            health = GetComponent<HealthController>();

        if (deathScreen != null)
            deathScreen.SetActive(false);
    }

    private void OnEnable()
    {
        if (health != null)
            health.OnDied.AddListener(OnPlayerDied);
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnDied.RemoveListener(OnPlayerDied);
    }

    private void OnPlayerDied()
    {
        if (loading)
            return;

        loading = true;

        if (deathScreen != null)
            deathScreen.SetActive(true);

        StartCoroutine(ReturnToMenuRoutine());
    }

    private IEnumerator ReturnToMenuRoutine()
    {
        yield return new WaitForSeconds(delayBeforeLoad);

        SceneManager.LoadScene(menuSceneName);
    }
}