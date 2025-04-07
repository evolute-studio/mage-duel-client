using TerritoryWars.Dojo;
using TerritoryWars.ExternalConnections;
using TerritoryWars.General;
using TerritoryWars.Tools;
using UnityEngine;

namespace TerritoryWars.UI.Popups
{
    public class PopupManager : MonoBehaviour
    {
        public static PopupManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null)
            {
                CustomLogger.LogWarning("PopupManager already exists. Deleting new instance.");
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }
        
        public MessagePopupBase MessagePopup;
        
        
        public void ShowOpponentCancelGame()
        {
            PopupConfig popupConfig = new PopupConfig
            {
                Text = "Red player has left the game.",
                FirstOptionText = "Menu",
                FirstOptionAction = () =>
                {
                    CustomSceneManager.Instance.LoadLobby();
                }
            };
            MessagePopup.Setup(popupConfig);
            MessagePopup.SetActive(true);
        }
        
        public void ShowNewAccountPopup()
        {
            PopupConfig popupConfig = new PopupConfig
            {
                Text = "Are you sure you want to create a new account?",
                FirstOptionText = "Cancel",
                FirstOptionAction = () => { },
                SecondOptionText = "Create",
                SecondOptionAction = () =>
                {
                    MenuUIController.Instance.NewAccount();
                }
            };
            MessagePopup.Setup(popupConfig);
            MessagePopup.SetActive(true);
        }

        public void ShowInvalidMovePopup()
        {
            PopupConfig popupConfig = new PopupConfig
            {
                Text = "Invalid move. Please cancel game or restart.",
                FirstOptionText = "OK",
                FirstOptionAction = () => { },
                SecondOptionText = "Cancel game",
                SecondOptionAction = () =>
                {
                    DojoConnector.CancelGame(DojoGameManager.Instance.LocalBurnerAccount);
                    CustomSceneManager.Instance.LoadLobby();
                }
            };
            
            MessagePopup.Setup(popupConfig);
            MessagePopup.SetActive(true);
        }

        public void ShowCantFinishGamePopup()
        {
            PopupConfig popupConfig = new PopupConfig
            {
                Text = "Cannot finish game. Please restart or cancel game",
                FirstOptionText = "OK",
                FirstOptionAction = () => { },
                SecondOptionText = "Cancel game",
                SecondOptionAction = () =>
                {
                    DojoConnector.CancelGame(DojoGameManager.Instance.LocalBurnerAccount);
                    CustomSceneManager.Instance.LoadLobby();
                }
            };
            
            MessagePopup.Setup(popupConfig);
            MessagePopup.SetActive(true);
        }

        public void NotYourTurnPopup()
        {
            PopupConfig popupConfig = new PopupConfig
            {
                Text = "Not your turn",
                FirstOptionText = "OK",
                FirstOptionAction = () => 
                { 
                    
                }
            };
            
            MessagePopup.Setup(popupConfig);
            MessagePopup.SetActive(true);
        }

        public void ShowTestPopup()
        {
            PopupConfig popupConfig = new PopupConfig
            {
                Text = "Test popup",
                FirstOptionText = "OK",
                FirstOptionAction = () =>
                {
                    CustomLogger.LogInfo("OK button clicked");
                },
                SecondOptionText = "Cancel",
                SecondOptionAction = () =>
                {
                    CustomLogger.LogInfo("Cancel button clicked");
                }
            };
            MessagePopup.Setup(popupConfig);
            MessagePopup.SetActive(true);
        }
    }
}