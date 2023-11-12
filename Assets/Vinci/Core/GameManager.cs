using UnityEngine;
using Vinci.Core.Utils;
using Solana.Unity.SDK;
using Unity.Sentis;
using System;


namespace Vinci.Core.Managers
{
    public class GameManager : PersistentSingleton<GameManager>
    {
        public string version = "v0.0.3";
        public bool localEnv = false;

        public UserData UserData;
        public PlayerData playerData;
        public GameData gameData;

        public bool isLoggedIn = false;

        public double solanaBalance;    

        public ModelAsset baseNNModel;



        private const string PlayerDataFileName = "playerData.json";

        protected override void Awake()
        {
            base.Awake();
            playerData = DataManager.LoadPlayerData();

            isLoggedIn = false;
            Application.runInBackground = true;

            Debug.Log("Init ML "+ version);
            Unity.MLAgents.Academy.Instance.AutomaticSteppingEnabled = false;

    

            Web3.OnBalanceChange += OneBalanceChange;
            UserData = new UserData();
        }

        void Start()
        {
            playerData.CheckAndIncreaseDailySteps();
           // DataManager.LoadPlayerData();
        }

        private void OneBalanceChange(double sol)
        {
            solanaBalance = sol;
        }

        public void SavePlayerData()
        {
            try{
                DataManager.SavePlayerData(playerData);
            }
            catch(Exception e)
            {
                Debug.Log("Error savinf player data: " + e.Message + " trace: " + e.StackTrace);
            }

        }

        private void LoadPlayerData()
        {
            playerData = DataManager.LoadPlayerData();
        }        
    }
}

