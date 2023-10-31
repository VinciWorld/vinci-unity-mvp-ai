using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
using Unity.Barracuda;
using System.Collections.Generic;
using System;

public class HallwayAgent : Agent, IAgent
{
    public bool useVectorObs = true;
    public int selection = 0;
    public HallwaySettings hallwaySettings;
    public EnvironementBase env;

    Rigidbody _agentRb;
    StatsRecorder _statsRecorder;
    BehaviorParameters _beahivor;

    private int _actionFromServer = 0;

    private bool _isReplay = false;
   
    public List<int> actionsBuffer = new List<int>();

    //Replay
    public Queue<int> actionsQueueReceived;


    public int steps = 0;
    private int countObservations = 0;
    private int CountHeuristic = 0;

    public event Action<IAgent> agentDied;

    protected override void Awake()
    {
        base.Awake();
        _beahivor = GetComponent<BehaviorParameters>();
    }

    void Start()
    {
         SetupAgent();
    }

    public override void Initialize()
    {
        base.Initialize();
        SetupAgent();
        _statsRecorder = Academy.Instance.StatsRecorder;
        
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("actions: " + steps + " obs: " + countObservations + " heuristic: " + CountHeuristic);
        steps = 0;
        countObservations = 0;
        CountHeuristic = 0;

        _agentRb.velocity *= 0f;
        env.EpisodeBegin();
        _statsRecorder.Add("Goal/Correct", 0, StatAggregationMethod.Sum);
        _statsRecorder.Add("Goal/Wrong", 0, StatAggregationMethod.Sum);
    }

    public override void CollectObservations(VectorSensor sensor)
    {

        countObservations++;
        if (useVectorObs)
        {
            sensor.AddObservation(StepCount / (float)MaxStep);
        }
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
        transform.Rotate(rotateDir, Time.deltaTime * 150f);

        if(hallwaySettings != null)
        {
            _agentRb.AddForce(dirToGo * hallwaySettings.agentRunSpeed, ForceMode.VelocityChange);
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        steps++;
        var discreteActionsOut = actionBuffers.DiscreteActions;

        if(_isReplay)
        {
            if(actionsQueueReceived.Count > 0)
            {
                discreteActionsOut[0] = actionsQueueReceived.Dequeue();
            }
        }

        AddReward(-1f / MaxStep);
        MoveAgent(actionBuffers.DiscreteActions);

#if !UNITY_EDITOR && UNITY_SERVER
        actionsBuffer.Add(discreteActionsOut[0]);
#endif

    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("symbol_O_Goal") || col.gameObject.CompareTag("symbol_X_Goal"))
        {
            if ((selection == 0 && col.gameObject.CompareTag("symbol_O_Goal")) ||
                (selection == 1 && col.gameObject.CompareTag("symbol_X_Goal")))
            {
                SetReward(1f);
                env.GoalCompleted(true); 
                _statsRecorder.Add("Goal/Correct", 1, StatAggregationMethod.Sum);
            }
            else
            {
                SetReward(-0.1f);
                env.GoalCompleted(false);
                _statsRecorder.Add("Goal/Wrong", 1, StatAggregationMethod.Sum);
            }
            if (!_isReplay)
            {
                EndEpisode();
            }
           
        }
    }

    public void ActionFromServer(int action)
    {
        _actionFromServer = action;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        CountHeuristic++;
        var discreteActionsOut = actionsOut.DiscreteActions;

        //discreteActionsOut[0] = _actionFromServer;

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
    }

    public void LoadModel(string behaviorName, NNModel model)
    {
        _beahivor.BehaviorType = BehaviorType.HeuristicOnly;
        Debug.Log(behaviorName);
        SetModel(behaviorName, model);
        _beahivor.BehaviorType = BehaviorType.InferenceOnly;
    }

    public void SetupAgent()
    {
        hallwaySettings = FindObjectOfType<HallwaySettings>();
        _agentRb = GetComponent<Rigidbody>();
    }

    public void SetBehaviorType(BehaviorType type)
    {
        _beahivor.BehaviorType = type;
    }

    public Pose ResetPosition(Vector3 groundPos)
    {
        var agentOffset = -15f;

        transform.position = new Vector3(0f + UnityEngine.Random.Range(-3f, 3f),
        1f, agentOffset + UnityEngine.Random.Range(-5f, 5f))
        + groundPos;
        transform.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);

        return new Pose(transform.position, transform.rotation);
    }

    public void Reset()
    {
        throw new System.NotImplementedException();
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
        return steps;
    }

    public void SetSteps(int steps)
    {
        throw new NotImplementedException();
    }

    public void SetActionsQueueReceived(Queue<ActionRobotBufferMsg> actionsQueue)
    {
        throw new NotImplementedException();
    }

    public Queue<ActionRobotBufferMsg> GetActionsQueueReceived()
    {
        return null;
    }

    public void SetAgentPose(Vector3 position, Quaternion quaternion)
    {
        throw new NotImplementedException();
    }

    public void SetObservationHelper(ObservationHelper obsHelper)
    {
        throw new NotImplementedException();
    }

    public void SetEnv(EnvironementBase env)
    {
        throw new NotImplementedException();
    }

    public void SetEvaluationMetrics(EvaluationMetrics evaluationMetrics)
    {
        throw new NotImplementedException();
    }
}
