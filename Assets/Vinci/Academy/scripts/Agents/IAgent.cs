using Unity.Barracuda;
using Unity.MLAgents.Policies;

public interface IAgent
{
    public void Reset();
    public void SetBehaviorType(BehaviorType type);
    public void LoadModel(string behaviorName, NNModel model);
}