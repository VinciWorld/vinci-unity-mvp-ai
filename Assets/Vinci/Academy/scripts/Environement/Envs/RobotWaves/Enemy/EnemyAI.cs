using System;
using UnityEngine;
using Vinci.Core.BattleFramework;


public enum AIState
{
    idle,
    Follow,
    Attack,
}

public class EnemyAI : AttackerPlacable
{
    private AIState _aistate = AIState.idle;
    private NavMeshMovement _navMeshMovement;

    public event Action<EnemyAI> died;

    [Range(0f, 1f)]
    public float AttackStopDistanceRatio = 0.5f;

    protected override void Awake()
    {
        base.Awake();
        _navMeshMovement = GetComponent<NavMeshMovement>();
    }

    protected override void Start()
    {
        base.Start();
        _detectionModule.AttackRange = weapon.GetWeaponRange();
        _detectionModule.onAttackeRange += OnAttackRange;
        _detectionModule.onDetectedTarget += OnDetectedTarget;
        _detectionModule.onLostTarget += OnLostTarget;

        targetable.died += OnDie;
    }

    private void FixedUpdate()
    {
        HandleAnimation();

        _detectionModule.HandleTargetDetection();

        //Debug.Log("Seeing target: " + _detectionModule.IsSeeingTarget);
        //Debug.Log("Attak range: " + _detectionModule.IsTargetInAttackRange);

        UpdateAiStateTransitions();
        UpdateCurrentAiState();

    }

    void HandleAnimation()
    {
        if (_navMeshMovement.GetAgentSpeed().magnitude >= 0.1f)
        {
            // _animator.SetBool("moving", true);
        }
        else
        {
            //_animator.SetBool("moving", false);
        }
    }

    public override void SetPosition(Vector3 position)
    {
        _navMeshMovement.SetPosition(position);
    }

    public void SetTarget(Targetable target)
    {
        if (_navMeshMovement != null)
        {
            _navMeshMovement.SetNavDestination(target.transform);

            _detectionModule.target = target;
        }
    }

    void UpdateAiStateTransitions()
    {
        // Handle transitions 
        switch (_aistate)
        {
            case AIState.Follow:

                // Transition to attack when there is a line of sight to the target
                if (_detectionModule.IsSeeingTarget && _detectionModule.IsTargetInAttackRange)
                {
                    _aistate = AIState.Attack;
                    _navMeshMovement.SetNavDestination(this.transform);
                }

                break;
            case AIState.Attack:
                // Transition to follow when no longer a target in attack range
                if (!_detectionModule.IsTargetInAttackRange && _detectionModule.IsSeeingTarget)
                {
                    _aistate = AIState.Follow;
                }
                break;
        }
    }

    void UpdateCurrentAiState()
    {
        // Handle logic 
        switch (_aistate)
        {
            case AIState.idle:
                break;
            case AIState.Follow:
                _navMeshMovement.SetNavDestination(_detectionModule.target.targetableTransform);
                break;
            case AIState.Attack:
                if(_detectionModule.target == null)
                {
                    _aistate = AIState.idle;
                    break;
                }
                
                if (Vector3.Distance(_detectionModule.target.position,
                        _detectionModule._detectionOriginPoint.position)
                    >= (AttackStopDistanceRatio * _detectionModule.AttackRange))
                {
                    _navMeshMovement.SetNavDestination(_detectionModule.target.targetableTransform);
                }
                else
                {
                    _navMeshMovement.SetNavDestination(transform);
                }

                _navMeshMovement.OrientTowards(_detectionModule.target.position);

                TryAtack(_detectionModule.target, _navMeshMovement);
                break;
        }
    }

    void OnDie(DamageableObject damageableObject, float damage, Vector3 damagePoint)
    {
        died?.Invoke(this);
    }


    private void OnLostTarget()
    {
        _aistate = AIState.idle;
    }

    private void OnDetectedTarget()
    {
        _aistate = AIState.Follow;
    }

    private void OnAttackRange()
    {
        _aistate = AIState.Attack;
    }

    public override void Reset()
    {
        base.Reset();
        _aistate = AIState.idle;
    }

}