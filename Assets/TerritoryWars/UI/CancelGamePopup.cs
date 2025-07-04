using TerritoryWars.Dojo;
using TerritoryWars.ExternalConnections;
using TerritoryWars.General;
using TerritoryWars.Managers.SessionComponents;
using UnityEngine;
using UnityEngine.UI;

namespace TerritoryWars.UI
{
    public class CancelGamePopup : MonoBehaviour
    {
        public GameObject PopUp;
        public Button ConfirmButton;
        public Button CancelButton;
        

        public void Start()
        {
            ConfirmButton.onClick.AddListener(CancelGame);
            CancelButton.onClick.AddListener(HidePopUp);
        }

        private void CancelGame()
        {
            if (!SessionManager.Instance.SessionContext.IsSpectatingGame)
            {
                DojoConnector.CancelGame(DojoGameManager.Instance.LocalAccount);
            }
            CustomSceneManager.Instance.LoadLobby();
        }
        
        private void HidePopUp()
        {
            SetActive(false);
        }
        
        public void SetActive(bool active)
        {
            PopUp.SetActive(active);
        }
    }
}
