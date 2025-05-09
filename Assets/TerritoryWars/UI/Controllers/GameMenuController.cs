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
            _view.GameMenuPanelRectTransform.DOAnchorPosY(0f, 0.5f);
        }
        else
        {
            _view.GameMenuPanelRectTransform.DOKill();
            _view.GameMenuPanelRectTransform.DOAnchorPosY(-_view.GameMenuPanelRectTransform.rect.height, 0.5f);
        }
    }
}
