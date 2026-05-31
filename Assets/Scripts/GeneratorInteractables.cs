using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class GeneratorInteractable : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("UI")]
    [SerializeField] private TMP_Text promptText;

    [Header("Messages")]
    [SerializeField] private string farMessage = "Start the generator";
    [SerializeField] private string nearMessage = "Press E to start generator";

    [Header("Distance")]
    [SerializeField] private float showTextDistance = 20f;
    [SerializeField] private float interactDistance = 2f;

    [Header("Interaction")]
    [SerializeField] private Key interactKey = Key.E;
    [SerializeField] private bool canOnlyUseOnce = true;

    [Header("Optional")]
    [SerializeField] private HordeEventController hordeEvent;
    [SerializeField] private AudioSource generatorSound;
    [SerializeField] private GameObject lightObject;

    [SerializeField] private RadioFinaleEvent radioFinaleEvent;

    private bool hasBeenUsed;

    private void Awake()
    {
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");

            if (foundPlayer != null)
                player = foundPlayer.transform;
        }
    }

    private void Start()
    {
        HidePrompt();

        if (lightObject != null)
            lightObject.SetActive(false);
    }

    private void Update()
    {
        if (player == null)
            return;

        if (hasBeenUsed && canOnlyUseOnce)
        {
            HidePrompt();
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= interactDistance)
        {
            ShowPrompt(nearMessage);

            if (Keyboard.current != null && Keyboard.current[interactKey].wasPressedThisFrame)
            {
                StartGenerator();
            }
        }
        else if (distance <= showTextDistance)
        {
            ShowPrompt(farMessage);
        }
        else
        {
            HidePrompt();
        }
    }

    private void StartGenerator()
    {
        if (hasBeenUsed && canOnlyUseOnce)
            return;

        hasBeenUsed = true;

        HidePrompt();

        Debug.Log($"{name}: Generator started.", this);

        if (generatorSound != null)
            generatorSound.Play();

        if (lightObject != null)
            lightObject.SetActive(true);

        if (radioFinaleEvent != null)
        {
            radioFinaleEvent.StartFinale();
        }

        if (hordeEvent != null)
            hordeEvent.StartHordeEvent();
    }

    private void ShowPrompt(string message)
    {
        if (promptText == null)
            return;

        promptText.text = message;
        promptText.gameObject.SetActive(true);
    }

    private void HidePrompt()
    {
        if (promptText == null)
            return;

        promptText.text = "";
        promptText.gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, showTextDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}