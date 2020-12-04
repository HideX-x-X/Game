using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Trap : MonoBehaviour
{
    [SerializeField] protected int _damage;
    [SerializeField] protected float _damageCooldown;
    protected float _nextAttackTime;
    protected bool _targetIn;
    protected Health _target;

    protected virtual void FixedUpdate()
    {
        if (!_targetIn || _target == null)
            return;

        if (Time.time > _nextAttackTime)
        {
            _target.AffectHealth(-_damage);
            _nextAttackTime = Time.time + _damageCooldown;
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        _target = collider.GetComponent<Health>();
        _targetIn = true;
    }

    protected virtual void OnTriggerExit2D(Collider2D collider)
    {
        _target = collider.GetComponent<Health>();
        _targetIn = false;
    }

}
