using System.IO;
using UnityEngine;
using Vinci.Core.Utils;
using Unity.MLAgents;
using Solana.Unity.SDK;
using System;
using WebSocketSharp;
using Unity.Sentis;
using File = System.IO.File;
using Newtonsoft.Json;

namespace Vinci.Core.Managers
{
    public class GameManager : PersistentSingleton<GameManager>
    {
        public string version = "v0.0.3";
        public bool localEnv = false;

        public UserData UserData;
        public PlayerData playerData;

        public bool isLoggedIn = false;

        public double solanaBalance;    

        public ModelAsset baseNNModel;



        private const string PlayerDataFileName = "playerData.json";

        protected override void Awake()
        {
            base.Awake();
            isLoggedIn = false;
            Application.runInBackground = true;

            Debug.Log("Init ML "+ version);
            Unity.MLAgents.Academy.Instance.AutomaticSteppingEnabled = false;

            LoadPlayerData();

            Web3.OnBalanceChange += OneBalanceChange;
            UserData = new UserData();
        }

        void Start()
        {
            playerData.CheckAndIncreaseDailySteps();
            SavePlayerData();
        }

        private void OneBalanceChange(double sol)
        {
            solanaBalance = sol;
        }

        public void SavePlayerData()
        {
            string path = Path.Combine(Application.persistentDataPath, PlayerDataFileName);
            string json = JsonConvert.SerializeObject(playerData, Formatting.Indented);
            System.IO.File.WriteAllText(path, json);
            Debug.Log("Player data saved to " + path);
        }

        private void LoadPlayerData()
        {
           

            string path = Path.Combine(Application.persistentDataPath, PlayerDataFileName);
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                playerData = JsonConvert.DeserializeObject<PlayerData>(json);

                //TODO: REMOVE !!!!
                playerData.highScore = 0;
                Debug.Log("Player data loaded from " + path);

                LoadModelsRuntime();
            }
            else
            {
                playerData = new PlayerData(); // Initialize with default values

                Debug.Log("No saved player data found, initialized with default values.");
            }
        }

        private void LoadModelsRuntime()
        {
            foreach(var agent in playerData.agents)
            {
                agent.modelConfig.isModelLoaded = false;

                if(!agent.modelConfig.nnModelPath.IsNullOrEmpty())
                {
 
                    ModelAsset loadedModel = MLHelper.LoadModelRuntime(
                        agent.modelConfig.behavior.behavior_name, agent.modelConfig.nnModelPath
                    );

                    agent.modelConfig.nnModel = loadedModel;
                    agent.modelConfig.isModelLoaded = true;
            }
            }
        }
    }
}

