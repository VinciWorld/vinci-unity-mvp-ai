using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using Unity.Sentis;
using UnityEngine;

public static class DataManager
{
    private const string PlayerDataFileName = "PlayerData.json"; // Filename for player data

    public static void SavePlayerData(PlayerData playerData)
    {
        string path = Path.Combine(Application.persistentDataPath, PlayerDataFileName);
        string json = JsonConvert.SerializeObject(playerData, Formatting.Indented, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Converters = new List<JsonConverter> { new MetricValueConverter() }
        });

        File.WriteAllText(path, json);
        Debug.Log("Player data saved to " + path);
    }

    // Load PlayerData which includes EnvMetricsData
    public static PlayerData LoadPlayerData()
    {
        string path = Path.Combine(Application.persistentDataPath, PlayerDataFileName);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            PlayerData playerData = JsonConvert.DeserializeObject<PlayerData>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = new List<JsonConverter> { 
                    new MetricValueConverter(),
                    new EnvMetricsDataConverter()
                }

            });

            LoadModelsRuntime(playerData);

            Debug.Log("Player data loaded from " + path);
 
            return playerData;
        }
        else
        {
            Debug.Log("No saved player data found, initializing with default values.");
            return new PlayerData();
        }
    }

    private static void LoadModelsRuntime(PlayerData playerData)
    {
        foreach (var agent in playerData.agents)
        {
            agent.modelConfig.isModelLoaded = false;

            if (!(agent.modelConfig.nnModelResourcePath != null || agent.modelConfig.nnModelResourcePath.Length == 0))
            {

                ModelAsset loadedModel = MLHelper.LoadModelRuntime(
                    agent.modelConfig.behavior.behavior_name, agent.modelConfig.nnModelResourcePath
                );

                agent.modelConfig.nnModel = loadedModel;
                agent.modelConfig.isModelLoaded = true;
            }
        }
    }
}

