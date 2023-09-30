using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
using Unity.Barracuda;
using System;

public class HallwayAgent : Agent
{
    public bool useVectorObs = true;
    public int selection;
    public HallwaySettings hallwaySettings;

    Rigidbody _agentRb;
    StatsRecorder _statsRecorder;
    BehaviorParameters _beahivor;

    public event Action episodeBegin;
    public event Action<int> OnActionsReceived;
    public event Action<bool> goalCompleted;

    int _actionFromServer = 0;

    void Start()
    {
        _beahivor = GetComponent<BehaviorParameters>();
#if UNITY_SERVER && !UNITY_EDITOR
        //_beahivor.BehaviorType = BehaviorType.Default;
#endif
        SetupAgent();
    }

    public void LoadModel(string behaviorName, NNModel model)
    {
        _beahivor.BehaviorType = BehaviorType.HeuristicOnly;
        SetModel(behaviorName, model);
        _beahivor.BehaviorType = BehaviorType.InferenceOnly;
    }

    public void SetupAgent()
    {
        hallwaySettings = FindObjectOfType<HallwaySettings>();
        _agentRb = GetComponent<Rigidbody>();
    }

    public override void Initialize()
    {
        base.Initialize();
        _statsRecorder = Academy.Instance.StatsRecorder;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
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
        _agentRb.AddForce(dirToGo * hallwaySettings.agentRunSpeed, ForceMode.VelocityChange);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var discreteActionsOut = actionBuffers.DiscreteActions;
        OnActionsReceived?.Invoke(discreteActionsOut[0]);
        
        AddReward(-1f / MaxStep);
        MoveAgent(actionBuffers.DiscreteActions);

    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("symbol_O_Goal") || col.gameObject.CompareTag("symbol_X_Goal"))
        {
            if ((selection == 0 && col.gameObject.CompareTag("symbol_O_Goal")) ||
                (selection == 1 && col.gameObject.CompareTag("symbol_X_Goal")))
            {
                SetReward(1f);
                goalCompleted?.Invoke(true);
                _statsRecorder.Add("Goal/Correct", 1, StatAggregationMethod.Sum);
            }
            else
            {
                SetReward(-0.1f);
                goalCompleted?.Invoke(false);
                _statsRecorder.Add("Goal/Wrong", 1, StatAggregationMethod.Sum);
            }
            EndEpisode();
        }
    }

    public void UpdateActions(int value)
    {
        _actionFromServer = value;
        Debug.Log(_actionFromServer + " server: " + value);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        discreteActionsOut[0] = _actionFromServer;

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

    public override void OnEpisodeBegin()
    {   
        episodeBegin?.Invoke();
        _agentRb.velocity *= 0f;
        _statsRecorder.Add("Goal/Correct", 0, StatAggregationMethod.Sum);
        _statsRecorder.Add("Goal/Wrong", 0, StatAggregationMethod.Sum);
    }

    public void ResetPosition(Vector3 groundPos)
    {
        var agentOffset = -15f;

        transform.position = new Vector3(0f + UnityEngine.Random.Range(-3f, 3f),
        1f, agentOffset + UnityEngine.Random.Range(-5f, 5f))
        + groundPos;
        transform.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
    }
}