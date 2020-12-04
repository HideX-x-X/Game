using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PCInputController : MonoBehaviour
{
    PlayerMovement _playerMovement;
    DateTime _strikeClickTime;
    float _move;
    bool _jump;
    bool _crouch;

    public bool CanMove { get; set; } = true;

    private void Start()
    {
        _playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if(!CanMove)
        {
            _move = 0;
            _jump = false;
            _crouch = false;

            return;
        }

        _move = Input.GetAxisRaw("Horizontal");
        if (Input.GetButtonUp("Jump"))
        {
            _jump = true;
        }

        _crouch = Input.GetKey(KeyCode.C);

        if (Input.GetKey(KeyCode.E))
            _playerMovement.StartCast();

        if (!IsPointerOverUI())
        {
            if (Input.GetButton("Fire1"))
            {
                _playerMovement.StartStrike();
            }
        }
    }

    private void FixedUpdate()
    {
        _playerMovement.Move(_move, _jump, _crouch);
        _jump = false;
    }

    private bool IsPointerOverUI() => EventSystem.current.IsPointerOverGameObject();
}
