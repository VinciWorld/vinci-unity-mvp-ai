using System;
using System.Collections.Generic;
using Unity.MLAgents.Policies;
using Unity.Sentis;
using UnityEngine;
using UnityEngine.Rendering;

public interface IAgent
{
    public event Action<IAgent> agentDied;

    public int GetSteps();
    public void SetSteps(int steps);
    public void SetActionsQueueReceived(Queue<ActionRobotBufferMsg> actionsQueue);
    public Queue<ActionRobotBufferMsg> GetActionsQueueReceived();

    public void SetAgentPose(Vector3 position, Quaternion quaternion);
    public void EndEpisode();
    public void AddReward(float reward);
    public void SetObservationHelper(ObservationHelper obsHelper);
    public void SetEnv(EnvironementBase env);
    public void SetEvaluationMetrics(EvaluationMetrics evaluationMetrics);
    public void Reset();
    public void SetBehaviorType(BehaviorType type);
    public void LoadModel(string behaviorName, ModelAsset model);
    public void SetIsReplay(bool isReplay);
    public GameObject GetGameObject();
}