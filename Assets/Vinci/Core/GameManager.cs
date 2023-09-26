using Vinci.Core.Utils;

namespace Vinci.Core.Managers
{
    public class GameManager : PersistentSingleton<GameManager>
    {
        public PlayerData playerData;

        // Start is called before the first frame update
        void Start()
        {
            playerData = new PlayerData();

        }
    }
}

