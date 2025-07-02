using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using Dojo;
using NUnit.Framework;
using TerritoryWars.DataModels;
using TerritoryWars.Dojo;
using TerritoryWars.General;
using TerritoryWars.Managers.SessionComponents;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.Tools;
using TerritoryWars.UI;
using TerritoryWars.UI.Session;
using UnityEngine;

public class FinishGameContests
{
    public static Action FinishGameAction;
    private Camera _mainCamera;
    private MouseControll _mainCameraMouseControll;
    private MouseControll _mouseControll;
    private List<ContestInformation> _contests;

    public FinishGameContests(List<ContestInformation> contests)
    {
        _contests = contests;
        _mainCamera = Camera.main;
        _mainCameraMouseControll = _mainCamera.GetComponent<MouseControll>();
        PrepareCameraForContest();
        PlayFinishGameAnimation();
        GameUI.Instance.SetFinishGameUI(false);
    }

    private async void PlayFinishGameAnimation()
    {
        int outlineLayer = LayerMask.NameToLayer("Outline");
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        var outlineObjects = allObjects.Where(obj => obj.layer == outlineLayer).ToList();

        foreach (var outlineObject in outlineObjects)
        {
            outlineObject.layer = LayerMask.NameToLayer("Default");
        }
        SessionManager.Instance.StructureHoverManager.SetActivePanel(false);
        
        foreach (var contest in _contests)
        {
            HashSet<(TileParts, Side)> roadTileParts = new HashSet<(TileParts, Side)>();
            List<TileParts> cityTileParts = new List<TileParts>();
            switch (contest.StructureType)
            {
                case StructureType.Road:
                    roadTileParts = SessionManager.Instance.StructureHoverManager.GetRoadTilePartsForHighlight(contest.Position, contest.Side);
                    foreach (var roadTilePart in roadTileParts)
                    {
                        roadTilePart.Item1.RoadOutline(true, roadTilePart.Item2, true);
                    }
                    break;
                case StructureType.City:
                    cityTileParts = SessionManager.Instance.StructureHoverManager.GetCityTilePartsForHighlight(contest.Position);
                    foreach (TileParts cityTilePart in cityTileParts)
                    {
                        cityTilePart.CityOutline(true, true);
                    }
                    break;
                case StructureType.None:
                    break;
            }
        }
        
        await CoroutineAsync(() => { }, 1f);
        
        foreach (var contest in _contests)
        {
            HashSet<(TileParts, Side)> roadTileParts = new HashSet<(TileParts, Side)>();
            List<TileParts> cityTileParts = new List<TileParts>();
            GameObject tile = SessionManager.Instance.BoardManager.GetTileObject(contest.Position.x, contest.Position.y);
            MoveCameraToContest(new Vector3(tile.transform.position.x, tile.transform.position.y, _mainCamera.transform.position.z), contest.ContestAction);
            
            switch (contest.StructureType)
            {
                case StructureType.Road:
                    roadTileParts = SessionManager.Instance.StructureHoverManager.GetRoadTilePartsForHighlight(contest.Position, contest.Side);
                    foreach (var roadTilePart in roadTileParts)
                    {
                        roadTilePart.Item1.RoadOutline(true, roadTilePart.Item2);
                    }
                    break;
                case StructureType.City:
                    cityTileParts = SessionManager.Instance.StructureHoverManager.GetCityTilePartsForHighlight(contest.Position);
                    foreach (TileParts cityTilePart in cityTileParts)
                    {
                        cityTilePart.CityOutline(true);
                    }
                    break;
                case StructureType.None:
                    break;
            }
            
            await CoroutineAsync(() => { }, 1f);
        
            if (contest.StructureType == StructureType.City)
            {
                SessionManager.Instance.BoardManager.CloseCityStructure(contest.Position);
                cityTileParts = SessionManager.Instance.StructureHoverManager.GetCityTilePartsForHighlight(contest.Position);
                foreach (TileParts cityTilePart in cityTileParts)
                {
                    cityTilePart.CityOutline(true);
                }
            }
            
            await CoroutineAsync(() => { }, 2f);
            
            switch (contest.StructureType)
            {
                case StructureType.Road:
                    roadTileParts = SessionManagerOld.Instance.StructureHoverManager.GetRoadTilePartsForHighlight(contest.Position, contest.Side);
                    foreach (var roadTilePart in roadTileParts)
                    {
                        roadTilePart.Item1.RoadOutline(false, roadTilePart.Item2);
                    }
                    break;
                case StructureType.City:
                    cityTileParts = SessionManagerOld.Instance.StructureHoverManager.GetCityTilePartsForHighlight(contest.Position);
                    foreach (TileParts cityTilePart in cityTileParts)
                    {
                        cityTilePart.CityOutline(false);
                    }
                    break;
                case StructureType.None:
                    break;
            }
        }
        
        FinishGameAction?.Invoke();
    }

    private void MoveCameraToContest(Vector3 position, Action callback = null)
    {
        _mainCameraMouseControll.SetCameraPosition(position, callback);
    }

    private void PrepareCameraForContest()
    {
        _mainCameraMouseControll.SetCameraLock(true);
        _mainCamera.DOOrthoSize(2.5f, 1.2f);
    }
    
    private async Task CoroutineAsync(Action action, float delay = 0f)
    {
        var tcs = new TaskCompletionSource<bool>();
        Coroutines.StartRoutine(WaitForCoroutine(tcs, action, delay));
        await tcs.Task;
    }
    
    private IEnumerator WaitForCoroutine(TaskCompletionSource<bool> tcs, Action action, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);
        action();
        tcs.TrySetResult(true);
    }
}
