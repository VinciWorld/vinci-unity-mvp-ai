using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vinci.Core.BattleFramework;


[RequireComponent(typeof(Rigidbody))]
public class Robot : PlaceableEntity
{
    [SerializeField]
    private float _moveSpeed = 2f;
    [SerializeField]
    private float _rotSpeed = 10f;

    private Rigidbody _rb;
    private Animator _animator;
    private WeaponController _weaponController;

    private const float deadZone = 0.1f;
    private const float dirDeadZone = 0.1f;

    public Transform gizmoArrow;

    protected override void Awake()
    {
        base.Awake();

        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _weaponController = GetComponent<WeaponController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        targetable.died += OnDied;

    }

    private void OnDied(DamageableObject obj, float damage, Vector3 hitPoint)
    {
        Debug.Log("DIED ROBOT");
    }

    // Update is called once per frame
    public void UpdateRobot(Vector3 move, Vector3 direction, bool shoot, float time)
    {
        UpdateMovement(move, time);
        Look(direction, shoot, time);
        UpdateAnimations(move, time);
    }

    public void UpdateMovement(Vector3 move, float time)
    {
        if (move.magnitude > deadZone)
        {
            transform.Translate(Vector3.forward * move.z * time * _moveSpeed);
            transform.Translate(Vector3.right * move.x * time * _moveSpeed);
        }
    }

    public void UpdateMovementPhysics(Vector3 move, float time)
    {
        if (move.magnitude > deadZone)
        {
            _rb.MovePosition(_rb.position + move * _moveSpeed * time);
        }

        if (_rb.velocity.sqrMagnitude > 10f) // slow it down
        {
            _rb.velocity *= 0.95f;
        }
    }



    public void UpdateAnimations(Vector3 move, float time)
    {
        if (move.z < deadZone)
        {
            _animator.SetBool("isRunning", false);
        }
        else
        {
            _animator.SetBool("isRunning", true);
        }
    }

    public void Look(Vector3 point, bool shoot, float time)
    {
        transform.LookAt(new Vector3(point.x, transform.position.y, point.z), Vector3.up);

        if (shoot)
        {
            _weaponController.Shoot(targetable);
        }
    }

    public void Shoot()
    {
        _weaponController.Shoot(targetable);
    }


    public void UpdateAim(Vector3 direction, bool shoot, float time)
    {
        if (direction.magnitude > dirDeadZone)
        {
            Vector3 lookPos = new Vector3(direction.x + transform.position.x, transform.position.y, direction.z + transform.position.z);
            Vector3 directionToLook = lookPos - transform.position;

            Quaternion targetRotation = Quaternion.LookRotation(directionToLook);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotSpeed * time);
        }

        if (shoot)
        {
            _weaponController.Shoot(targetable);
        }
    }

    public override void Reset()
    {
        base.Reset();
    }
}
