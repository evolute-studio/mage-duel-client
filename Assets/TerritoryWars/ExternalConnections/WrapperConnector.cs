using TerritoryWars.General;
using TerritoryWars.Tools;
using TerritoryWars.UI;
using UnityEngine;

namespace TerritoryWars.ExternalConnections
{
    public class WrapperConnector : MonoBehaviour
    {
        public static WrapperConnector instance;
        
        public string username;
        public string address;
        
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
        
        public void OnControllerLogin(string data)
        {
            ControllerLoginData loginData = JsonUtility.FromJson<ControllerLoginData>(data);
            username = loginData.username;
            address = loginData.address;
            CustomSceneManager.Instance.LoadingScreen.SetActive(true, null, LoadingScreen.launchGameText);
            CustomLogger.LogDojoLoop("Controller logged in");
            EntryPoint.Instance.InitializeControllerGameAsync();
        }

        public void OnGuestLogin(string data)
        {
            CustomSceneManager.Instance.LoadingScreen.SetActive(true, null, LoadingScreen.launchGameText);
            CustomLogger.LogDojoLoop("Guest logged in");
            EntryPoint.Instance.InitializeGuestGameAsync();
        }

        public void OnControllerUsername(string name)
        {
            if(!MenuUIController.Instance) return;
            MenuUIController.Instance.ChangeNamePanelUIController.SetName(name, false);
        } 
        
        public void OnControllerNotLoggedIn()
        {
            
        }
        
        struct ControllerLoginData
        {
            public string username;
            public string address;
        }
    }
}