using UnityEngine;

public class HealthBarUi : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image _healthBarHealthImage;

    public void UpdateHealthBar(HealthController healthController)
    {
        _healthBarHealthImage.fillAmount = healthController.ReimainingHealthPercentage;
    }
}
