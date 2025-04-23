using DG.Tweening;
using NUnit.Framework.Constraints;
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
        _view.GameMenuButton.onClick.AddListener(SetActiveGameMenu);
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
        _view.CancelGamePopUp.SetActive(true);
    }

    private void SetActiveGameMenu()
    {
        _model.IsGameMenuActive = !_model.IsGameMenuActive;
        
        if (_model.IsGameMenuActive)
        {
            _view.GameMenuPanel.transform.DOKill();
            _view.GameMenuPanel.transform.DOMoveY(0f, 0.5f);
        }
        else
        {
            _view.GameMenuPanel.transform.DOKill();
            _view.GameMenuPanel.transform.DOMoveY(-80f, 0.5f);
        }
    }
}
