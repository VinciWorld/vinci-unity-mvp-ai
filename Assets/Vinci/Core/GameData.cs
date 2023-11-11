using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Vinci.Academy.Environement;

public class GameData : MonoBehaviour {

    public AgentBlueprint[] agentsAvailable;
    public TrainEnvironmentConfig[] trainEvns;

    public TrainEnvironmentConfig GetTrainEnvById(string id)
    {
        return trainEvns.FirstOrDefault(env => env.env_id == id);
    }

    public AgentBlueprint GetAgentById(string id)
    {
        return agentsAvailable.FirstOrDefault(agent => agent.id == id);
    }

    public AgentBlueprint CreateInstanceById(string id)
    {
        var agent = agentsAvailable.FirstOrDefault(agent => agent.id == id);
        Debug.Log("agent: " + agent + " id: " + id);

        return agent.Clone();
    }
}

//TODO move to other file
public static class ScriptableObjectExtension
{
    /// <summary>
    /// Creates and returns a clone of any given scriptable object.
    /// </summary>
    public static T Clone<T>(this T scriptableObject) where T : ScriptableObject
    {
        if (scriptableObject == null)
        {
            Debug.LogError($"ScriptableObject was null. Returning default {typeof(T)} object.");
            return (T)ScriptableObject.CreateInstance(typeof(T));
        }

        T instance = Object.Instantiate(scriptableObject);
        instance.name = scriptableObject.name; // remove (Clone) from name
        return instance;
    }
}