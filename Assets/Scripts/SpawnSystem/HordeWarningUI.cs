using UnityEngine;
using TMPro;
using System.Collections;

public class HordeWarningUI : MonoBehaviour
{
    [SerializeField] private GameObject warningObject;
    [SerializeField] private TMP_Text warningText;
    [SerializeField] private float showTime = 3f;

    private Coroutine warningRoutine;

    public void ShowWarning(string message)
    {
        if (warningRoutine != null)
            StopCoroutine(warningRoutine);

        warningRoutine = StartCoroutine(ShowWarningRoutine(message));
    }

    private IEnumerator ShowWarningRoutine(string message)
    {
        if (warningObject != null)
            warningObject.SetActive(true);

        if (warningText != null)
            warningText.text = message;

        yield return new WaitForSeconds(showTime);

        if (warningObject != null)
            warningObject.SetActive(false);

        warningRoutine = null;
    }
}
