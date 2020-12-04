using System;
using System.Collections;
using UnityEngine;

public enum EnemyState
{
    None,
    Idle,
    Move,
    TargetSpoted,
    Attack,
}

public enum EnemyMovementType
{
    HoldPosition,
    Patrol
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public abstract class EnemyBase : MonoBehaviour
{
    [Header("Physical")]
    [SerializeField] protected float _speed;
    [SerializeField] protected float _triggerRange;
    [SerializeField] protected float _attackRadius;
    [SerializeField] protected EnemyMovementType _movementType;
    [Header("States")]
    [SerializeField] private EnemyState _targetLostState;
    [SerializeField] protected bool _chaseTarget = true;
    [SerializeField] private bool _disableFlip;
    [Header("Walk behaviour")]
    [SerializeField] private Transform[] _waypoints;

    protected Animator _animator;
    protected Rigidbody2D _rigidbody;
    protected Vector2 _initialPosition;
    protected EnemyState _currentState;
    protected DateTime _lastStateChange;
    protected float _timeToNextChange;
    protected bool isRightDirection = true;
    protected Transform _target;
    private int _currentWaypointIndex = -1;
    private Transform _wayPointTarget;
    private Transform currentWaypoint => _waypoints[_currentWaypointIndex];
    private bool isAttackRangeCheckRunned;
    private bool isRandomStatePassed => ((DateTime.Now - _lastStateChange).TotalSeconds >= _timeToNextChange);

    protected virtual void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();

        if (_movementType == EnemyMovementType.Patrol)
            SetToNextWaypoint();

        StartCoroutine(nameof(ScanForTarget));
    }

    protected virtual void FixedUpdate()
    {
        switch (_currentState)
        {
            case EnemyState.Idle:
                OnIdleState();
                break;
            case EnemyState.Move:
                OnMoveState();
                break;
            case EnemyState.TargetSpoted:
                OnTargetSpotedState();
                break;
            case EnemyState.Attack:
                OnAttackState();
                break;
        }

        Debug.Log(_currentState);
    }

    #region State_handlers
    protected virtual void OnIdleState()
    {
        if (_movementType == EnemyMovementType.Patrol)
            ChangeState(EnemyState.Move);
    }

    protected virtual void OnMoveState()
    {
        if (_movementType == EnemyMovementType.Patrol)
            MoveToWaypoint();
    }

    protected virtual void OnTargetSpotedState()
    {
        if (!isAttackRangeCheckRunned)
            StartCoroutine(nameof(CheckTargetInAttackRange));
    }

    protected virtual void OnAttackState()
    {
        Debug.Log($"{gameObject.name} Attacks!");
    }
    #endregion

    #region Waypoints
    private void SetToNextWaypoint()
    {
        if (_waypoints.Length == 0)
            return;

        _currentWaypointIndex++;
        if (_currentWaypointIndex >= _waypoints.Length)
            _currentWaypointIndex = 0;
    }

    private void MoveToWaypoint()
    {
        TurnToTarget(currentWaypoint);
        if (Mathf.Abs(Vector2.Distance(transform.position, currentWaypoint.transform.position)) > 1f)
        {
            _rigidbody.velocity = transform.right * new Vector2(_speed, _rigidbody.velocity.y);
        }
        else SetToNextWaypoint();
    }
    #endregion

    protected void Flip()
    {
        if (_disableFlip)
            return;

        isRightDirection = !isRightDirection;
        transform.Rotate(0, 180, 0);
    }

    protected virtual void ChangeState(EnemyState state)
    {
        _animator.SetBool(_currentState.ToString(), false);
        _currentState = state;
        _animator.SetBool(_currentState.ToString(), true);
    }

    protected IEnumerator ScanForTarget()
    {
        while (true)
        {
            CheckTargetInRange();
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator CheckTargetCanBeAttacked()
    {
        isAttackRangeCheckRunned = true;
        while (_currentState == EnemyState.TargetSpoted)
        {
            CheckTargetInAttackRange();
            yield return new WaitForSeconds(1f);
        }
        isAttackRangeCheckRunned = false;
    }

    protected virtual void CheckTargetInRange()
    {
        if (_target == null)
            return;

        bool isInRange = (Vector2.Distance(transform.position, _target.transform.position) < _triggerRange);
        if (isInRange)
        {
            ChangeState(EnemyState.TargetSpoted);
            TurnToTarget();
        }
        else
        {
            ChangeState(_targetLostState);
        }
    }

    protected virtual void CheckTargetInAttackRange()
    {
        if (Mathf.Abs(Vector2.Distance(transform.position, _target.position)) > _attackRadius)
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

    protected virtual void TurnToTarget()
    {
        if (_target.position.x - transform.position.x > 0 && !isRightDirection)
            Flip();
        else if (_target.transform.position.x - transform.position.x < 0 && isRightDirection)
            Flip();
    }

    private void TurnToTarget(Transform target)
    {
        if (target.position.x - transform.position.x > 0 && !isRightDirection)
            Flip();
        else if (target.transform.position.x - transform.position.x < 0 && isRightDirection)
            Flip();
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 from = new Vector3(transform.position.x - _triggerRange, transform.position.y);
        Vector3 to = new Vector3(transform.position.x + _triggerRange, transform.position.y);
        Gizmos.DrawLine(from, to);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, _attackRadius);
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

}
