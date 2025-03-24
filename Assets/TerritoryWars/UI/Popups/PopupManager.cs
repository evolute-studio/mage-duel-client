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

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                ShowTestPopup();
            }
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