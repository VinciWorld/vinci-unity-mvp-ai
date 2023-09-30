using Vinci.Core.Utils;

namespace Vinci.Core.Managers
{
    public class GameManager : PersistentSingleton<GameManager>
    {
        public PlayerData playerData;
        
        protected override void Awake() 
        {
            base.Awake();
            LoadPlayerData();
        }


        private void LoadPlayerData()
        {

        }
    }
}

