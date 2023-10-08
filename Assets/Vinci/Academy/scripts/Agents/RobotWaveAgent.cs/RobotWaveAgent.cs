using System;
using System.Collections.Generic;
using Unity.Barracuda;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Vinci.Core.BattleFramework;

public class RobotWaveAgent : Agent, IAgent
{

    InputControllerBasic _input;
    Robot _robot;
    public EnvironementBase env;
    BehaviorParameters _beahivor;

    Rigidbody _rb;

    private bool _isReplay = false;
    public int steps = 0;

    public List<ActionRobotBufferMsg> actionsBuffer = new List<ActionRobotBufferMsg>();
    
    //Replay
    public Queue<ActionRobotBufferMsg> actionsQueueReceived;

    public float agentRunSpeed = 1.5f;
    public float rotationSpeed = 100f;

    protected override void Awake()
    {
        base.Awake();

        _robot = GetComponent<Robot>();
        _input = GetComponent<InputControllerBasic>();
        _rb = GetComponent<Rigidbody>();
        _beahivor = GetComponent<BehaviorParameters>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();


    }

    // Start is called before the first frame update
    void Start()
    {
        _robot.targetable.hitTarget += OnhitTarget;
        _robot.targetable.missedTarget += OnMissedTarget;
        _robot.targetable.killedTarget += OnKilledTarget;
        _robot.targetable.died += OnDied;
    }



    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
        env.EpisodeBegin();
        steps = 0;
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(StepCount / (float)MaxStep);
    }


    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        steps++;
        var discreteActionsOut = actionBuffers.DiscreteActions;

        if (_isReplay)
        {
            if (actionsQueueReceived.Count > 0)
            {
                ActionRobotBufferMsg actionRobotBufferMsg = actionsQueueReceived.Dequeue();
                discreteActionsOut[0] = actionRobotBufferMsg.direction;
                discreteActionsOut[1] = actionRobotBufferMsg.fire;
            }
        }

        AddReward(-1f / MaxStep);

        MoveAgent(actionBuffers.DiscreteActions);
        if (discreteActionsOut[1] == 1)
        {
            _robot.Shoot();
        }

#if !UNITY_EDITOR && UNITY_SERVER

        ActionRobotBufferMsg bufferActions = new ActionRobotBufferMsg()
        {
            direction = discreteActionsOut[0],
            fire = discreteActionsOut[1]
        };

        actionsBuffer.Add(bufferActions);
#endif

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = 3;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 4;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
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

    private void OnDied(DamageableObject @object, float arg2, Vector3 vector)
    {
        Debug.Log("DIEDDDDD");
        AddReward(-1f);
        EndEpisode();
    }

    private void OnKilledTarget()
    {
        Debug.Log("kills ");

        AddReward(0.05f);
    }

    private void OnMissedTarget()
    {
        Debug.Log("miss ");

        AddReward(-0.05f);
    }

    private void OnhitTarget()
    {
        Debug.Log("hit ");
        AddReward(0.001f);
    }


    // Update is called once per frame
    void FixedUpdate()
    {/*
        _robot.UpdateRobot(
            _input.GetMove(),
            _input.GetMoveDirection(),
            _input.GetFireButton(),
            Time.fixedDeltaTime);

    */
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var action = act[0];
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
        }
        transform.Rotate(rotateDir, Time.deltaTime * rotationSpeed);

        _rb.AddForce(dirToGo * agentRunSpeed, ForceMode.VelocityChange);
    }


    public void Reset()
    {
        _robot.Reset();
    }

    public void SetBehaviorType(BehaviorType type)
    {
        _beahivor.BehaviorType = type;
    }

    public void LoadModel(string behaviorName, NNModel model)
    {
        _beahivor.BehaviorType = BehaviorType.HeuristicOnly;
        SetModel(behaviorName, model);
        _beahivor.BehaviorType = BehaviorType.InferenceOnly;
    }

    public void SetIsReplay(bool isReplay)
    {
        _isReplay = isReplay;
    }
}
