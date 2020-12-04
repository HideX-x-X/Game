using System;
using UnityEngine;

public class SwordEnemy : EnemyBase
{
    [SerializeField] Transform _attackRangeChecker;
    [Header("Attack behaviour")]
    [SerializeField] private float _attackCooldownSeconds;
    [SerializeField] private int _damage;

    private Health _health;
    private Health _playerHealth;
    private DateTime _lastAttack;

    private void Awake()
    {
        _health = GetComponent<Health>();
        _health.AddOnDieListener(OnDie);
    }

    protected override void Start()
    {
        base.Start();
        _target = FindObjectOfType<PlayerController>().transform;
        _playerHealth = _target.GetComponent<Health>();
    }

    protected override void OnAttackState()
    {
        if (_playerHealth.IsDead)
        {
            ChangeState(EnemyState.Idle);
            _speed = 0f;
            return;
        }

        _rigidbody.velocity = transform.right * new Vector2(_speed, _rigidbody.velocity.y);

        if ((DateTime.Now - _lastAttack).TotalSeconds >= _attackCooldownSeconds)
        {
            _playerHealth.AffectHealth(-_damage);

            _lastAttack = DateTime.Now;
        }
    }

    protected override void CheckTargetInAttackRange()
    {
        if (_currentState == EnemyState.None)
        {
            _speed = 0f;
            return;
        }

        if (Mathf.Abs(Vector2.Distance(_attackRangeChecker.position, _target.position)) > _attackRadius)
        {
            TurnToTarget();

            if (_chaseTarget)
            {
                _rigidbody.velocity = transform.right * new Vector2(_speed, _rigidbody.velocity.y);
            }
            else
            {
                ChangeState(EnemyState.Attack);
            }
        }
        else
        {
            ChangeState(EnemyState.Attack);
        }
    }

    private void OnDie(GameObject gameObj)
    {
        ChangeState(EnemyState.None);
        GetComponent<Animator>().Play("Death");
    }

    protected override void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 from = new Vector3(transform.position.x - _triggerRange, transform.position.y);
        Vector3 to = new Vector3(transform.position.x + _triggerRange, transform.position.y);
        Gizmos.DrawLine(from, to);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(_attackRangeChecker.position, _attackRadius);
    }
}
