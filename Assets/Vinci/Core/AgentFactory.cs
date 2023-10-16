using UnityEngine;
using Vinci.Core.Utils;

public class AgentFactory : Singleton<AgentFactory> {

    public GameObject CreateAgent(AgentConfig config, Vector3 position, Quaternion rotation, Transform parent)
    {

    Debug.Log("AgentConfig: " + config + " config: " + config.AgentPrefab);
        GameObject agent = Instantiate(
          config.AgentPrefab, position,
          rotation, parent);

        return agent;
    }

}