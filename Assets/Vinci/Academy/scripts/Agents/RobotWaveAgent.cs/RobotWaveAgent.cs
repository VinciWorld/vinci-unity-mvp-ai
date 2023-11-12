using System;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using Unity.Sentis;
using UnityEngine;
using Vinci.Core.BattleFramework;

public class RobotWaveAgent : Agent, IAgent
{
    
    InputControllerBasic _input;
    public Robot _robot;
    private EnvironementBase _env;
    BehaviorParameters _beahivor;

    Rigidbody _rb;

    private bool isheuristicInput = false;
    private bool _isReplay = false;
    private int _steps = 0;

    //Replay
    public List<ActionRobotBufferMsg> actionsBuffer = new List<ActionRobotBufferMsg>();
    private Queue<ActionRobotBufferMsg> _actionsQueueReceived;

    //Evaluation Metrics
    private EvaluationMetrics _evaluationMetrics;
    private int killsPerEpisode;
    private int shootHitsPerEpisode;
    private int shootsMissedPerEpisode;



    public float agentRunSpeed = 1.5f;
    public float rotationSpeed = 100f;

    public event Action<IAgent> agentDied;
    public event Action agentKill;

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
        Debug.Log("Episode being: " + _env.episodeCount());
        base.OnEpisodeBegin();
        _env?.EpisodeBegin();
        _steps = 0;
        killsPerEpisode = 0;
        shootHitsPerEpisode = 0;
        shootsMissedPerEpisode = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if(!_isReplay)
        {
            sensor.AddObservation(StepCount / (float)MaxStep);
            sensor.AddObservation(true);
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        _steps++;
        var discreteActionsOut = actionBuffers.DiscreteActions;

        if (_isReplay)
        {
            if (_actionsQueueReceived.Count > 0)
            {
                ActionRobotBufferMsg actionRobotBufferMsg = _actionsQueueReceived.Dequeue();
                discreteActionsOut[0] = actionRobotBufferMsg.direction;
                discreteActionsOut[1] = actionRobotBufferMsg.fire;
            }
        }
        else
        {
            AddReward(-1f / MaxStep);
        }

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
        if(isheuristicInput)
        {
            var discreteActionsOut = actionsOut.DiscreteActions;

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

    }

    private void OnDied(DamageableObject @object, float arg2, Vector3 vector)
    {
        //Debug.Log("DIEDDDDD");
        AddReward(-1f);
        agentDied?.Invoke(this);
  
        if(_env != null)
        {
            _env.GoalCompleted(false);
        }

        EndEpisode();
    }

    private void OnKilledTarget()
    {
        //Debug.Log("kills ");
        killsPerEpisode++;
        agentKill?.Invoke();
        AddReward(0.08f);
        _evaluationMetrics.UpdateAgentMetricForEpisode(
            _env.episodeCount(), MetricKeys.Kills.ToString(), new MetricValue(MetricType.Int, killsPerEpisode)
        );
    }

    private void OnMissedTarget()
    {
        //        Debug.Log("miss ");
        shootsMissedPerEpisode++;
        AddReward(-0.05f);
        _evaluationMetrics.UpdateAgentMetricForEpisode(
            _env.episodeCount(), MetricKeys.Shoots_Missed.ToString(), new MetricValue(MetricType.Int, shootsMissedPerEpisode)
        );
    }

    private void OnhitTarget()
    {
       //Debug.Log("hit ");
        shootHitsPerEpisode++;
        AddReward(0.001f);
        _evaluationMetrics.UpdateAgentMetricForEpisode(
            _env.episodeCount(), MetricKeys.Shoots_Hits.ToString(), new MetricValue(MetricType.Int, shootHitsPerEpisode)
        );
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


    public void Reset()
    {
        _robot.Reset();
        //_episodeCounter = 0;
    }

    public void SetBehaviorType(BehaviorType type)
    {    
        _beahivor.BehaviorType = type;
    }

    public void LoadModel(string behaviorName, ModelAsset model)
    {
        //_beahivor.BehaviorType = BehaviorType.HeuristicOnly;
        Debug.Log("Set model!: " + model.name);
        SetModel(behaviorName, model);
        //_beahivor.BehaviorType = BehaviorType.InferenceOnly;
    }

    public void SetIsReplay(bool isReplay)
    {
        _isReplay = isReplay;
    }

    public GameObject GetGameObject()
    {
        return this.gameObject;
    }

    public int GetSteps()
    {
        return _steps;
    }

    public void SetSteps(int steps)
    {
        _steps = steps;
    }

    public void SetActionsQueueReceived(Queue<ActionRobotBufferMsg> actionsQueue)
    {
        _actionsQueueReceived = actionsQueue;
    }

    public Queue<ActionRobotBufferMsg> GetActionsQueueReceived()
    {
        return _actionsQueueReceived;
    }

    public void SetAgentPose(Vector3 position, Quaternion quaternion)
    {
        transform.position = position;
        transform.rotation = quaternion;
    }

    public void SetObservationHelper(ObservationHelper obsHelper)
    {
        //obsHelper = obsHelper;
    }

    public void SetEnv(EnvironementBase env)
    {
        _env = env;
    }

    public void SetEvaluationMetrics(EvaluationMetrics evaluationMetrics)
    {
        _evaluationMetrics = evaluationMetrics;
    }
}
