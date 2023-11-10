using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vinci.Academy.Environement;

public class PlayerSerilizeData
{
    public string username;

    public bool isPlayerRegisteredOnCompetition = false;
    public int highScore = 0;

    public int availableSteps = 0;
    public string _lastUpdatedDay = "2000-01-01";

    public List<AgentData> agentsData = new List<AgentData>();

    public void SerilizeAgentData(List<AgentBlueprint> agents)
    {

    }
}

[Serializable]
public class KeyValuePairs<K, V>
{
    public K key;
    public V value;
}

[Serializable]
public class SerializableDictionary<K, V>
{
    public List<KeyValuePairs<K, V>> list = new List<KeyValuePairs<K, V>>();

    public Dictionary<K, V> ToDictionary()
    {

        return list.ToDictionary(x => x.key, x => x.value);
    }

    public void FromDictionary(Dictionary<K, V> dictionary)
    {
        list.Clear();
        foreach (var kvp in dictionary)
        {
            list.Add(new KeyValuePairs<K, V>() { key = kvp.Key, value = kvp.Value });
        }
    }
}

