using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    private Slider _slider;
    private Player _player;

    void Awake()
    {
        _slider = GetComponent<Slider>();
    }

    void Start()
    {
        _player = Player.Instance;
        if (_player != null)
            _player.OnHealthChanged += UpdateHealthBar;
    }

    private void UpdateHealthBar(float current, float max)
    {
        _slider.maxValue = max;
        _slider.value = current;
    }

    void OnDestroy()
    {
        if (_player != null)
            _player.OnHealthChanged -= UpdateHealthBar;
    }
}
