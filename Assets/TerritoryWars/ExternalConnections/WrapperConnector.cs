using TerritoryWars.General;
using TerritoryWars.Tools;
using TerritoryWars.UI;
using UnityEngine;

namespace TerritoryWars.ExternalConnections
{
    public class WrapperConnector : MonoBehaviour
    {
        public static WrapperConnector instance;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void LoginController()
        {
            
        }

        public void OnUsernameReceived(string username)
        {
            Debug.Log("Username received: " + username);
            MenuUIController.Instance?._namePanelController.SetName(username);
        }

        public void OnControllerLogin()
        {
            CustomSceneManager.Instance.LoadingScreen.SetActive(true, null, LoadingScreen.launchGameText);
            EntryPoint.Instance.InitializeGameAsync();
        }

        public void OnControllerNotLoggedIn()
        {
            EntryPoint.Instance.InitializeGameAsync();
        }
    }
}