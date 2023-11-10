using UnityEngine;
using Vinci.Core.Utils;
using Solana.Unity.SDK;
using Unity.Sentis;


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
            isLoggedIn = false;
            Application.runInBackground = true;

            Debug.Log("Init ML "+ version);
            Unity.MLAgents.Academy.Instance.AutomaticSteppingEnabled = false;

            playerData = DataManager.LoadPlayerData();

            Web3.OnBalanceChange += OneBalanceChange;
            UserData = new UserData();
        }

        void Start()
        {
            playerData.CheckAndIncreaseDailySteps();
            DataManager.SavePlayerData(playerData);
        }

        private void OneBalanceChange(double sol)
        {
            solanaBalance = sol;
        }

        public void SavePlayerData()
        {
            DataManager.SavePlayerData(playerData);
        }

        private void LoadPlayerData()
        {
            playerData = DataManager.LoadPlayerData();
        }        
    }
}

