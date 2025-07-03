using TerritoryWars.Dojo;
using TerritoryWars.ExternalConnections;
using TerritoryWars.General;
using TerritoryWars.Managers.Reports;
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
        public ReportPopup ReportPopup;


        public void ShowReportPopup()
        {
            PopupConfig popupConfig = new PopupConfig
            {
                Text = null,
                FirstOptionText = "Cancel",
                FirstOptionAction = () => { },
                SecondOptionText = "Send",
                SecondOptionAction = () =>
                {
                    ReportPopup.SendReport();
                }
            };
            ReportPopup.Setup(popupConfig);
            ReportPopup.SetActive(true);
        }
        
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
                Text = "[Invalid move]\nPlease reload the page or cancel the game",
                FirstOptionText = "Reload",
                FirstOptionAction = JSBridge.ReloadPage,
                SecondOptionText = "Cancel game",
                SecondOptionAction = () =>
                {
                    DojoConnector.CancelGame(DojoGameManager.Instance.LocalAccount);
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
                Text = "[Cannot finish game]\nPlease reload the page or cancel the game",
                FirstOptionText = "Reload",
                FirstOptionAction = JSBridge.ReloadPage,
                SecondOptionText = "Cancel game",
                SecondOptionAction = () =>
                {
                    DojoConnector.CancelGame(DojoGameManager.Instance.LocalAccount);
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
                Text = "[Not your turn]\nPlease reload the page or cancel the game",
                FirstOptionText = "Reload",
                FirstOptionAction = JSBridge.ReloadPage,
                SecondOptionText = "Cancel game",
                SecondOptionAction = () =>
                {
                    DojoConnector.CancelGame(DojoGameManager.Instance.LocalAccount);
                    CustomSceneManager.Instance.LoadLobby();
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