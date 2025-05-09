using TerritoryWars.UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameMenuView : MonoBehaviour
{ 
     public Button SnapshotButton;
     public Button SettingsButton;
     public Button PlaybookButton;
     public Button ExitButton;
     public Button GameMenuButton;
     public Button GameMenuCloseButton;
     public GameObject GameMenuPanel;
     public CancelGamePopup CancelGamePopUp;
     public RectTransform GameMenuPanelRectTransform;

     public void Initialize()
     {
          GameMenuPanelRectTransform = GameMenuPanel.GetComponent<RectTransform>();
     }
}
