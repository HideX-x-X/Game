using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [SerializeField] Slider _hpSlider;
    [SerializeField] Slider _armorSlider;
    [SerializeField] private bool useArmor;
    [Space]
    [SerializeField] internal int _maxHealth;
    [SerializeField] internal int _maxArmor;
    [Space]
    [SerializeField] private bool _useColoring;
    [SerializeField] private Color _onDamageColor;
    [SerializeField] private Color _onHealColor;
    private SpriteRenderer _renderred;

    public bool IgnoreHurt { get; set; }
    public delegate void dieHandler(GameObject gameObj);
    public delegate void healthChangedHandler(int value, int currentHealth, int currentArmor);
    public delegate void armorChangedHandler(int value, int currentArmor);

    private int _currentHealth;
    private int _currentArmor;
    private dieHandler onDie;
    private healthChangedHandler onHealthChanged;
    private armorChangedHandler onArmorChanged;

    public int HealthValue => _currentHealth;
    public int ArmorValue => _currentArmor;
    public bool IsDead => (_currentHealth <= 0);

    private void Awake()
    {
        _renderred = GetComponent<SpriteRenderer>();
        _currentHealth = _maxHealth;
        _currentArmor = _maxArmor;
    }

    private void Start()
    {
        _hpSlider.maxValue = _maxHealth;
        _hpSlider.value = _currentHealth;

        if(useArmor)
        {
            _armorSlider.value = _maxArmor;
            _armorSlider.value = _currentArmor;
        }
    }

    private int ApplyDamageToArmor(int value)
    {
        value = Mathf.Abs(value);
        int unabsorbed = 0;

        if (_currentArmor - value < 0)
        {
            unabsorbed = Mathf.Abs(_currentArmor - value);
            _currentArmor = 0;
        }
        else
        {
            _currentArmor -= value;
        }

        return unabsorbed;
    }

    internal void AffectHealth(int value)
    {
        if (IgnoreHurt)
        {
            Debug.Log("Hurt ignored for " + gameObject.name);
            return;
        }

        if (value < 0)
        {
            int unabsorbed = ApplyDamageToArmor(value);
            if (unabsorbed > 0)
            {
                _currentHealth = Mathf.Clamp(_currentHealth - unabsorbed, 0, _maxHealth);
            }
        }
        else
        {
            _currentHealth = Mathf.Clamp(_currentHealth + value, 0, _maxHealth);
        }

        if (_currentHealth <= 0)
        {
            OnDeath();
        }

        onHealthChanged?.Invoke(value, _currentHealth, _currentArmor);
        _hpSlider.value = _currentHealth;
        if(useArmor)
            _armorSlider.value = _currentArmor;

        if (_useColoring)
        {
            if(value < 0)
                StartColoring(_onDamageColor);
            else
                StartColoring(_onHealColor);
        }
    }

    internal void AffectArmor(int value)
    {
        _currentArmor = Mathf.Clamp(_currentArmor + value, 0, _maxArmor);
        onArmorChanged?.Invoke(value, _currentArmor);
        
        if(useArmor)
            _armorSlider.value = _currentArmor;
    }

    public void AddOnDieListener(dieHandler handler)
    {
        onDie += handler;
    }

    public void AddOnHealthChangedListener(healthChangedHandler handler)
    {
        onHealthChanged += handler;
    }

    public void AddOnArmorChangedListener(armorChangedHandler handler)
    {
        onArmorChanged += handler;
    }

    private void OnDeath()
    {
        Debug.Log(gameObject.name + ".OnDeath()");
        onDie?.Invoke(gameObject);
    }

    public Coroutine StartColoring(Color color)
    {
        if (_renderred.gameObject.activeSelf)
            return StartCoroutine(ApplyDamageColor(color));
        else
            return StartCoroutine(nameof(Empty));
    }

    private IEnumerator ApplyDamageColor(Color color)
    {
        _renderred.color = color;
        yield return new WaitForSeconds(0.4f);
        _renderred.color = Color.white;
    }

    private IEnumerator Empty()
    {
        yield return new WaitForEndOfFrame();
    }

    internal void Kill()
    {
        AffectHealth(_currentHealth + _currentArmor);
    }

}