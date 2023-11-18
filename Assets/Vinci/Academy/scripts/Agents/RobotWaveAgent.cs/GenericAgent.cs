using System;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using Unity.Sentis;
using UnityEngine;

public abstract class GenericAgent : Agent, IAgent
{
    public int instanceId = 0;

    protected EnvironementBase _env;
    BehaviorParameters _beahivor;


    public bool isheuristicInput = false;
    protected bool _isReplay = false;
    private int _steps = 0;

    protected EnvironementSensor environamentSensor;

    //Replay
    public List<ActionRobotBufferMsg> actionsBuffer = new List<ActionRobotBufferMsg>();
    private Queue<ActionRobotBufferMsg> _actionsQueueReceived;

    //Evaluation Metrics
    protected EvaluationMetrics _evaluationMetrics;

    public abstract event Action<IAgent> agentDied;
    public abstract event Action agentKill;

    protected override void Awake()
    {
        base.Awake();
        _beahivor = GetComponent<BehaviorParameters>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
        _env?.EpisodeBegin();
        _steps = 0;
        OnAgentEpisodeBegin();
    }

    protected abstract void OnAgentEpisodeBegin();

    public override void CollectObservations(VectorSensor sensor)
    {
        if(!_isReplay)
        {
            AgentCollectObservations(sensor);
        }
    }

    protected abstract void AgentCollectObservations(VectorSensor sensor);

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

        OnAgentActionReceived(actionBuffers);


#if !UNITY_EDITOR && UNITY_SERVER

        ActionRobotBufferMsg bufferActions = new ActionRobotBufferMsg()
        {
            direction = discreteActionsOut[0],
            fire = discreteActionsOut[1]
        };

        actionsBuffer.Add(bufferActions);
#endif

    }

    protected abstract void OnAgentActionReceived(ActionBuffers actionBuffers);


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        if(isheuristicInput)
        {
            AgentHeuristic(actionsOut);
        }
    }

    protected abstract void AgentHeuristic(in ActionBuffers actionsOut);

    protected void RegisterMetric(string metricKey, MetricType type, object value, bool isHigherBetter = true)
    {
        _evaluationMetrics.UpdateAgentMetricForEpisode(
            _env.episodeCount(), metricKey, new MetricValue(type, value, isHigherBetter)
        );
    }

    public void SetBehaviorType(BehaviorType type)
    {    
        _beahivor.BehaviorType = type;
    }

    public void LoadModel(string behaviorName, ModelAsset model)
    {
        Debug.Log("Set model!: " + model.name);
        SetModel(behaviorName, model);
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

    public void SetObservationHelper(EnvironementSensor observationHelper)
    {
        environamentSensor = observationHelper;
    }

    public void SetEnv(EnvironementBase env)
    {
        _env = env;
    }

    public void SetEvaluationMetrics(EvaluationMetrics evaluationMetrics)
    {
        _evaluationMetrics = evaluationMetrics;
    }

    public virtual void Reset()
    {
    }
}
