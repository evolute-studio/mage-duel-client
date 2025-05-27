using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using Dojo;
using NUnit.Framework;
using TerritoryWars.Dojo;
using TerritoryWars.General;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.Tools;
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
        SessionManagerOld.Instance.gameUI.SetFinishGameUI(false);
    }

    private async void PlayFinishGameAnimation()
    {
        foreach (var contest in _contests)
        {
            HashSet<KeyValuePair<TileParts, Side>> roadTileParts = new HashSet<KeyValuePair<TileParts, Side>>();
            List<TileParts> cityTileParts = new List<TileParts>();
            Vector2Int coord = OnChainBoardDataConverter.GetPositionByRoot(contest.Root);
            GameObject tile = SessionManagerOld.Instance.Board.GetTileObject(coord.x, coord.y);
            TileParts tileParts = tile.GetComponentInChildren<TileParts>();
            switch (contest.ContestType)
            {
                case ContestType.Road:
                    roadTileParts = SessionManagerOld.Instance.StructureHoverManager.GetRoadTilePartsForHighlight(tile.transform, tileParts, contest.Root);
                    foreach (var roadTilePart in roadTileParts)
                    {
                        roadTilePart.Key.RoadOutline(true, roadTilePart.Value, true);
                    }
                    break;
                case ContestType.City:
                    cityTileParts = SessionManagerOld.Instance.StructureHoverManager.GetCityTilePartsForHighlight(tile.transform);
                    foreach (TileParts cityTilePart in cityTileParts)
                    {
                        cityTilePart.CityOutline(true, true);
                    }
                    break;
                case ContestType.None:
                    break;
            }
        }
        
        await CoroutineAsync(() => { }, 2f);
        
        foreach (var contest in _contests)
        {
            HashSet<KeyValuePair<TileParts, Side>> roadTileParts = new HashSet<KeyValuePair<TileParts, Side>>();
            List<TileParts> cityTileParts = new List<TileParts>();
            Vector2Int coord = OnChainBoardDataConverter.GetPositionByRoot(contest.Root);
            GameObject tile = SessionManagerOld.Instance.Board.GetTileObject(coord.x, coord.y);
            TileParts tileParts = tile.GetComponentInChildren<TileParts>();
            MoveCameraToContest(new Vector3(tile.transform.position.x, tile.transform.position.y, _mainCamera.transform.position.z), contest.ContestAction);
            
            switch (contest.ContestType)
            {
                case ContestType.Road:
                    roadTileParts = SessionManagerOld.Instance.StructureHoverManager.GetRoadTilePartsForHighlight(tile.transform, tileParts, contest.Root);
                    foreach (var roadTilePart in roadTileParts)
                    {
                        roadTilePart.Key.RoadOutline(true, roadTilePart.Value);
                    }
                    break;
                case ContestType.City:
                    cityTileParts = SessionManagerOld.Instance.StructureHoverManager.GetCityTilePartsForHighlight(tile.transform);
                    foreach (TileParts cityTilePart in cityTileParts)
                    {
                        cityTilePart.CityOutline(true);
                    }
                    break;
                case ContestType.None:
                    break;
            }
            
            await CoroutineAsync(() => { }, 1f);

            if (contest.ContestType == ContestType.City)
            {
                SessionManagerOld.Instance.Board.CloseCityStructure(contest.Root);
                cityTileParts = SessionManagerOld.Instance.StructureHoverManager.GetCityTilePartsForHighlight(tile.transform);
                foreach (TileParts cityTilePart in cityTileParts)
                {
                    cityTilePart.CityOutline(true);
                }
            }
            
            await CoroutineAsync(() => { }, 2f);
            
            switch (contest.ContestType)
            {
                case ContestType.Road:
                    roadTileParts = SessionManagerOld.Instance.StructureHoverManager.GetRoadTilePartsForHighlight(tile.transform, tileParts, contest.Root);
                    foreach (var roadTilePart in roadTileParts)
                    {
                        roadTilePart.Key.RoadOutline(false, roadTilePart.Value);
                    }
                    break;
                case ContestType.City:
                    cityTileParts = SessionManagerOld.Instance.StructureHoverManager.GetCityTilePartsForHighlight(tile.transform);
                    foreach (TileParts cityTilePart in cityTileParts)
                    {
                        cityTilePart.CityOutline(false);
                    }
                    break;
                case ContestType.None:
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
