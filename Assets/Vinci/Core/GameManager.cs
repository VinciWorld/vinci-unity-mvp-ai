using System.IO;
using UnityEngine;
using Vinci.Core.Utils;
using Unity.MLAgents;

namespace Vinci.Core.Managers
{
    public class GameManager : PersistentSingleton<GameManager>
    {
        public PlayerData playerData;

        private const string PlayerDataFileName = "playerData.json";

        protected override void Awake()
        {
            base.Awake();
            
            Debug.Log("Init ML agent");
            Unity.MLAgents.Academy.Instance.AutomaticSteppingEnabled = false;

            LoadPlayerData();
        }

        public void SavePlayerData()
        {
            string path = Path.Combine(Application.persistentDataPath, PlayerDataFileName);
            string json = JsonUtility.ToJson(playerData);
            File.WriteAllText(path, json);
            Debug.Log("Player data saved to " + path);
        }

        private void LoadPlayerData()
        {
            string path = Path.Combine(Application.persistentDataPath, PlayerDataFileName);
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                playerData = JsonUtility.FromJson<PlayerData>(json);
                Debug.Log("Player data loaded from " + path);
            }
            else
            {
                playerData = new PlayerData(); // Initialize with default values
                Debug.Log("No saved player data found, initialized with default values.");
            }
        }
    }
}

