using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Slider _mpSlider;
    [SerializeField] private int _maxMP;
    [SerializeField] private float _mpRegenCooldown;
    [SerializeField] private int _mpRegenValue;
    private int _currentMP;

    private PlayerMovement _playerMovement;
    private ServiceManager _serviceManager;
    private Health _health;

    void Start()
    {
        _health = GetComponent<Health>();
        _playerMovement = GetComponent<PlayerMovement>();
        _currentMP = _maxMP;
        _mpSlider.maxValue = _maxMP;
        _mpSlider.value = _currentMP;
        _serviceManager = ServiceManager.Instanse;
        
        _health.AddOnDieListener(OnDeath);
        _health.AddOnHealthChangedListener(OnHealthChanged);
        _health.AddOnArmorChangedListener(OnArmorChanged);

        StartCoroutine(nameof(MPRegen));
    }

    public bool GetIsCanCast(int cost) => (_currentMP-cost >= 0);

    public void ChangeMP(int value)
    {
        _currentMP = Mathf.Clamp(_currentMP + value, 0, _maxMP);
        _mpSlider.value = _currentMP;
    }

    private void OnHealthChanged(int value, int currentHealth, int currentArmor)
    {

    }

    private void OnArmorChanged(int value, int currentArmor)
    {
        
    }

    private void OnDeath(GameObject obj)
    {
        GetComponent<PCInputController>().CanMove = false;
        GetComponent<Health>().IgnoreHurt = true;
        GetComponent<Animator>().Play("Death");
    }

    public void Destroy()
    {
        _serviceManager.Restart();
    }

    private IEnumerator MPRegen()
    {
        while (!_health.IsDead)
        {
            ChangeMP(_mpRegenValue);
            yield return new WaitForSeconds(_mpRegenCooldown);
        }
    }

}
