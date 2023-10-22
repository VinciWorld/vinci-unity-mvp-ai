using System;
using Unity.Barracuda;
using Unity.MLAgents.Policies;
using UnityEngine;

public interface IAgent
{
    public event Action<IAgent> agentDied;

    public void Reset();
    public void SetBehaviorType(BehaviorType type);
    public void LoadModel(string behaviorName, NNModel model);
    public void SetIsReplay(bool isReplay);
    public GameObject GetGameObject();
}