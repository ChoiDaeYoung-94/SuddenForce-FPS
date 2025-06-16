using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    public static UIManager Instance => _instance;

    public JoyStick JoyStick;
    [SerializeField] private Slider _healthBar;
    [SerializeField] private Text _ammoText;

    private void Awake()
    {
        _instance = this;
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    public void UpdateHealthBar(int current, int max = 100)
    {
        _healthBar.maxValue = max;
        _healthBar.value = current;
    }

    public void UpdateAmmoCount(int ammo)
    {
        _ammoText.text = ammo.ToString();
    }
}
