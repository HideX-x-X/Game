using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D _playerRB;
    private Animator _playerAnimator;

    [Header("Horizontal movement")]
    [SerializeField] private float _speed;
    [Range(0, 1)]
    [SerializeField] private float _crouchSpeedReduce;

    private bool _faceRight = true;

    [Header("Jumping")]
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _groundCheckRadius;
    [SerializeField] private bool _airControll;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private LayerMask _whatIsGround;
    private bool _grounded;
    private bool _canDoubleJump;

    [Header("Crouching")]
    [SerializeField] private float _cellCheckRadius;
    [SerializeField] private Transform _cellCheck;
    [SerializeField] private Collider2D _headCollider;
    private bool _canStand;

    [Header("Casting")]
    [SerializeField] private GameObject _fireBall;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _castCooldown;
    [SerializeField] private int _castCost;
    private bool _isCasting;
    private float _castCooldownExpires;


    [Header("Strike")]
    [SerializeField] private Transform _strikePoint;
    [SerializeField] private int _damage;
    [SerializeField] private float _strikeRange;
    [SerializeField] private float _strikeCooldown;
    [SerializeField] private LayerMask _enemies;
    private bool _isStriking;
    private float _strikeCooldownExpires;

    [Header("Audio")]
    [SerializeField] private AudioClip _moveSound;
    private AudioSource _audioSource;

    private PlayerController _playerController;
    private List<GameObject> _damagedEnemies = new List<GameObject>();

    #region UnityMethods
    private void Start()
    {
        _playerController = GetComponent<PlayerController>();
        _playerRB = GetComponent<Rigidbody2D>();
        _playerAnimator = GetComponent<Animator>();

        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = _moveSound;
        _audioSource.loop = true;
    }

    private void FixedUpdate()
    {
        _grounded = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _whatIsGround);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_cellCheck.position, _cellCheckRadius);
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(_strikePoint.position, _strikeRange);
    }

    #endregion

    #region PrivateMethods
    private void ResetPlayer()
    {
        _playerAnimator.SetBool("Strike", false);
        _playerAnimator.SetBool("PowerStrike", false);
        _playerAnimator.SetBool("Casting", false);
        _playerAnimator.SetBool("Hurt", false);
        _isCasting = false;
        _isStriking = false;
    }

    private void Flip()
    {
        _faceRight = !_faceRight;
        transform.Rotate(0, 180, 0);
    }

    #endregion 

    #region PublicMethods
    public void Move(float move, bool jump, bool crouch)
    {
        #region Movement
        float speedModificator = _headCollider.enabled ? 1 : _crouchSpeedReduce;

        if ((_grounded || _airControll))
            _playerRB.velocity = new Vector2(_speed * move * speedModificator, _playerRB.velocity.y);

        if (move > 0 && !_faceRight)
        {
            Flip();
        }
        else if (move < 0 && _faceRight)
        {
            Flip();
        }
        #endregion

        #region Jumping

        if (jump && !crouch)
        {
            if (_grounded)
            {
                _playerRB.velocity = new Vector2(_playerRB.velocity.x, _jumpForce);
                _canDoubleJump = true;
            }
            else if (_canDoubleJump)
            {
                _playerRB.velocity = new Vector2(_playerRB.velocity.x, _jumpForce);
                _canDoubleJump = false;
            }
        }
        #endregion

        #region Crouching
        _canStand = !Physics2D.OverlapCircle(_cellCheck.position, _cellCheckRadius, _whatIsGround);
        if (crouch)
        {
            _headCollider.enabled = false;
        }
        else if (!crouch && _canStand)
        {
            _headCollider.enabled = true;
        }
        #endregion

        #region Animation
        _playerAnimator.SetFloat("Speed", Mathf.Abs(move));
        _playerAnimator.SetBool("Jump", !_grounded);
        _playerAnimator.SetBool("Crouch", !_headCollider.enabled);
        #endregion

        if (move != 0 && _grounded && _grounded && (!crouch && _canStand))
        {
            if (!_audioSource.isPlaying)
                _audioSource.Play();
        }
        else
        {
            _audioSource.Stop();
        }
    }

    public void StartStrike()
    {
        if (_isStriking || !_grounded)
            return;

        if(Time.time > _strikeCooldownExpires)
        {
            _isStriking = true;
            _playerAnimator.SetBool("Strike", true);
        }
    }

    public void Strike()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(_strikePoint.position, _strikeRange, _enemies);
        for (int i = 0; i < enemies.Length; i++)
        {
            Health enemy = enemies[i].GetComponent<Health>();
            if (enemy != null)
                enemy.AffectHealth(-_damage);
        }
    }

    public void EndStrike()
    {
        _strikeCooldownExpires = Time.time + _strikeCooldown;
        _isStriking = false;
        _playerAnimator.SetBool("Strike", false);
    }

    public void StartCast()
    {
        if (_isCasting || !_playerController.GetIsCanCast(_castCost) || 
            !_grounded || _playerRB.velocity != Vector2.zero)
            return;

        if (Time.time > _castCooldownExpires)
        {
            _playerController.ChangeMP(-_castCost);
            _isCasting = true;
            _playerAnimator.SetBool("Casting", true);
        }
    }

    public void Cast()
    {
        GameObject fireBall = Instantiate(_fireBall, _firePoint.position, Quaternion.identity);
        fireBall.GetComponent<ProjectileBase>().Flip = !_faceRight;
        Destroy(fireBall, 5f);
    }

    public void EndCast()
    {
        _castCooldownExpires = Time.time + _castCooldown;
        _isCasting = false;
        _playerAnimator.SetBool("Casting", false);
    }
    #endregion
}