using TerritoryWars.Dojo;
using UnityEngine;

public class GameMenuController
{
    private GameMenuView _view;
    private GameMenuModel _model;

    public void Initialize(GameMenuView view)
    {
        _view = view;
        _model = new GameMenuModel();
        SetupButtons();
    }

    private void SetupButtons()
    {
        _view.SnapshotButton.onClick.AddListener(OnSnapshotButtonClicked);
        _view.SettingsButton.onClick.AddListener(OnSettingsButtonClicked);
        _view.PlaybookButton.onClick.AddListener(OnPlaybookButtonClicked);
        _view.ExitButton.onClick.AddListener(OnExitButtonClicked); 
        _view.GameMenuButton.onClick.AddListener(() => SetActiveGameMenu(true));
        _view.GameMenuCloseButton.onClick.AddListener(() => SetActiveGameMenu(false));
    }

    private void OnSnapshotButtonClicked()
    {
        DojoGameManager.Instance.SessionManager.CreateSnapshot();
    }

    private void OnSettingsButtonClicked()
    {
        
    }

    private void OnPlaybookButtonClicked()
    {
        
    }

    private void OnExitButtonClicked()
    {
        SetActiveGameMenu(false);
        _view.CancelGamePopUp.SetActive(true);
    }

    private void SetActiveGameMenu(bool active)
    {
        _view.GameMenuPanel.SetActive(active);
    }
}
