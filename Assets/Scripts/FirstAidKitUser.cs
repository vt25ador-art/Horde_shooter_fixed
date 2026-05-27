using UnityEngine;
using TMPro;

public class FirstAidUser : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private HealthController health;

    [Header("First Aid")]
    [SerializeField] private int maxFirstAid = 1;
    [SerializeField] private float healAmount = 50f;
    [SerializeField] private float useTime = 5f;

    [Header("Input")]
    [SerializeField] private KeyCode cancelKey = KeyCode.None;

    [Header("UI Optional")]
    [SerializeField] private GameObject healPanel;
    [SerializeField] private TMP_Text healText;
    [SerializeField] private TMP_Text firstAidCountText;

    private int currentFirstAid;
    private float useTimer;
    private bool isHealing;

    public int CurrentFirstAid => currentFirstAid;
    public bool HasFirstAid => currentFirstAid > 0;

    private void Awake()
    {
        if (health == null)
            health = GetComponent<HealthController>();
    }

    private void Start()
    {
        if (healPanel != null)
            healPanel.SetActive(false);

        UpdateFirstAidUI();
    }

    private void Update()
    {
        if (health == null)
            return;

        if (health.isDowned || health.IsDead)
        {
            CancelHeal();
            return;
        }

        if (Input.GetMouseButton(1))
        {
            TryHealTick();
        }
        else
        {
            CancelHeal();
        }

        if (cancelKey != KeyCode.None && Input.GetKeyDown(cancelKey))
        {
            CancelHeal();
        }
    }

    public bool AddFirstAid(int amount)
    {
        if (currentFirstAid >= maxFirstAid)
            return false;

        currentFirstAid += amount;
        currentFirstAid = Mathf.Clamp(currentFirstAid, 0, maxFirstAid);

        UpdateFirstAidUI();

        Debug.Log("Picked up first aid. Current: " + currentFirstAid);

        return true;
    }

    private void TryHealTick()
    {
        if (currentFirstAid <= 0)
            return;

        isHealing = true;
        useTimer += Time.deltaTime;

        if (healPanel != null)
            healPanel.SetActive(true);

        if (healText != null)
        {
            float remaining = Mathf.Max(0f, useTime - useTimer);
            healText.text = "Healing... " + remaining.ToString("0.0") + "s";
        }

        if (useTimer >= useTime)
        {
            CompleteHeal();
        }
    }

    private void CompleteHeal()
    {
        currentFirstAid--;

        if (health != null)
            health.AddHealth(healAmount);

        Debug.Log("First aid used!");

        useTimer = 0f;
        isHealing = false;

        if (healPanel != null)
            healPanel.SetActive(false);

        UpdateFirstAidUI();
    }

    private void CancelHeal()
    {
        if (!isHealing)
            return;

        useTimer = 0f;
        isHealing = false;

        if (healPanel != null)
            healPanel.SetActive(false);

        if (healText != null)
            healText.text = "";
    }

    private void UpdateFirstAidUI()
    {
        if (firstAidCountText != null)
            firstAidCountText.text = "First Aid: " + currentFirstAid + " / " + maxFirstAid;
    }
}
