using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    private static GameUIManager _instance;
    public static GameUIManager Instance => _instance;

    public JoyStick JoyStick;
    [SerializeField] private Image _healthBar;
    [SerializeField] private TMP_Text _ammoText;

    private void Awake()
    {
        _instance = this;
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    public void UpdateHealthBar(int current)
    {
        _healthBar.fillAmount = current / 100f;
    }

    public void UpdateAmmoCount(int ammo)
    {
        _ammoText.text = $"{ammo} / ∞";
    }
}
