using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Rendering;
using Vinci.Core.BattleFramework;


[RequireComponent(typeof(Rigidbody))]
public class RobotWave : PlaceableEntity
{

    public float agentRunSpeed = 1.5f;
    public float rotationSpeed = 100f;

    private Rigidbody _rb;
    private Animator _animator;
    private WeaponController _weaponController;


    protected override void Awake()
    {
        base.Awake();

        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _weaponController = GetComponent<WeaponController>();
        _weaponController.Initialize();
    }

    // Start is called before the first frame update
    void Start()
    {
        targetable.died += OnDied;
    }

    public void AgentHeuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        HeuristicSimple(discreteActionsOut);
    }

    public void HeuristicAdvance(ActionSegment<int> discreteActionsOut)
    {
        if (Input.GetKey(KeyCode.E))
        {
            discreteActionsOut[0] = 3;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            discreteActionsOut[0] = 4;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = 5;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 6;
        }

        if (Input.GetMouseButton(1))
        {
            discreteActionsOut[1] = 1;
        }
        else
        {
            discreteActionsOut[1] = 0;
        }
    }

    public void HeuristicSimple(ActionSegment<int> discreteActionsOut)
    {
        if (Input.GetKey(KeyCode.E))
        {
            discreteActionsOut[0] = 3;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            discreteActionsOut[0] = 4;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }

        if (Input.GetMouseButton(0))
        {
            discreteActionsOut[1] = 1;
        }
        else
        {
            discreteActionsOut[1] = 0;
        }
    }

    public void MoveAgent(int action)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;
        switch (action)
        {
            case 1:
                dirToGo = transform.forward * 1f;
                break;
            case 2:
                dirToGo = transform.forward * -1f;
                break;
            case 3:
                rotateDir = transform.up * 1f;
                break;
            case 4:
                rotateDir = transform.up * -1f;
                break;
            case 5:
                dirToGo = transform.right * 1f;
                break;
            case 6:
                dirToGo = transform.right * -1;
                break;
        }

        transform.Rotate(rotateDir, Time.deltaTime * rotationSpeed);

        _rb.AddForce(dirToGo * agentRunSpeed, ForceMode.VelocityChange);
    }

    public bool Shoot()
    {
        return _weaponController.Shoot(targetable);
    }

    public bool CanShoot()
    {
        return _weaponController.currentWeapon.canShoot;
    }

    public override void Reset()
    {
        base.Reset();
    }


    private void OnDied(DamageableObject obj, float damage, Vector3 hitPoint)
    {
        //Debug.Log("DIED ROBOT");
    }
}
