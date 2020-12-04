using System;
using UnityEngine;

public class ArcherEnemy : EnemyBase
{
    [SerializeField] Transform _attackRangeChecker;
    [Header("Attack behaviour")]
    [SerializeField] private float _attackCooldownSeconds;
    [SerializeField] private Transform _shootPoint;
    [SerializeField] private GameObject _projectile;

    private Health _health;
    private Health _playerHealth;
    private AudioSource _audioSource;
    private DateTime _lastAttack;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
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
            return;
        }

        if ((DateTime.Now - _lastAttack).TotalSeconds >= _attackCooldownSeconds)
        {
            GameObject projectile = Instantiate(_projectile, _shootPoint.position, Quaternion.identity);
            projectile.GetComponent<ProjectileBase>().Flip = !(transform.rotation.y == 180);
            Destroy(projectile, 8f);
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

        if (Mathf.Abs(_attackRangeChecker.position.x-_target.transform.position.x) <= _attackRadius)
        {
            TurnToTarget();
            ChangeState(EnemyState.Attack);
        }
        else
        {
            ChangeState(EnemyState.TargetSpoted);
        }
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

    private void OnDie(GameObject gameObj)
    {
        Destroy(gameObject);
    }
}
