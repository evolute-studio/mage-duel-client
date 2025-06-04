using DG.Tweening;
using TerritoryWars.Dojo;
using TerritoryWars.ExternalConnections;
using UnityEngine;

public class GameMenuController : MonoBehaviour
{
    [SerializeField] private GameMenuView _view;
    private GameMenuModel _model;

    public void Awake()
    {
        _model = new GameMenuModel();
        _view.Initialize();
        SetupButtons();
    }

    private void SetupButtons()
    {
        _view?.SnapshotButton.onClick.AddListener(OnSnapshotButtonClicked);
        _view?.PlaybookButton.onClick.AddListener(OnPlaybookButtonClicked);
        _view?.ExitButton.onClick.AddListener(OnExitButtonClicked); 
        _view?.GameMenuButton.onClick.AddListener(SetActiveGameMenu);
    }

    private void OnSnapshotButtonClicked()
    {
        DojoGameManager.Instance.DojoSessionManager.CreateSnapshot();
    }

    private void OnSettingsButtonClicked()
    {
        
    }

    private void OnPlaybookButtonClicked()
    {
        JSBridge.OpenURL("https://evolute.notion.site/playbook");
    }

    private void OnExitButtonClicked()
    {
        _view.CancelGamePopUp.SetActive(true);
    }

    private void SetActiveGameMenu()
    {
        _model.IsGameMenuActive = !_model.IsGameMenuActive;
        
        if (_model.IsGameMenuActive)
        {
            _view.GameMenuPanelRectTransform.DOKill();
            _view.GameMenuPanelRectTransform.DOAnchorPosY(0f, 0.25f);
        }
        else
        {
            _view.GameMenuPanelRectTransform.DOKill();
            _view.GameMenuPanelRectTransform.DOAnchorPosY(-_view.GameMenuPanelRectTransform.rect.height, 0.25f);
        }
    }
}
