using System;
using System.Collections;
using System.Collections.Generic;
using TerritoryWars.DataModels;
using TerritoryWars.DataModels.Events;
using TerritoryWars.General;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.Tile;
using TerritoryWars.Tools;
using UnityEngine;

namespace TerritoryWars.Managers.SessionComponents
{
    public class ContestManager: ISessionComponent
    {
        private SessionManagerContext _managerContext;
        
        public ContestProcessor ContestProcessor = new ContestProcessor();
        
        public void Initialize(SessionManagerContext managerContext)
        {
            _managerContext = managerContext;
            EventBus.Subscribe<Contested>(OnContest);
        }

        private void OnContest(Contested contest)
        {
            ContestProcessor.AddModel(new ContestInformation(contest.Position, contest.Side, contest.Type,() =>
            {
                ContestAnimation(contest, RecolorStructures);
            }));
        }
        
        private void ContestAnimation(Contested contest, Action recoloring)
        {
            ClashAnimation contestAnimation = CreateContestAnimation();
            Vector2Int coord = contest.Position;
            GameObject tile = SessionManager.Instance.BoardManager.GetTileObject(coord.x, coord.y);
            
            Vector3 offset = new Vector3(0, 1.5f, 0);
            int winner = contest.WinnerId;
            ushort[] points = new ushort[] { contest.BluePoints, contest.RedPoints };
            bool isRoadContest = contest.Type == StructureType.Road;
            bool isCityContest = contest.Type == StructureType.City;
            if (tile)
            {
                Vector3 position = tile.transform.position;
                contestAnimation.Initialize(position + offset, winner, points, recoloring, isRoadContest, isCityContest);
            }
            else
            {
                Coroutines.StartRoutine(RemoteContestAnimation(coord, points, contestAnimation, recoloring, isRoadContest, isCityContest));
            }
        }
        
        private IEnumerator RemoteContestAnimation(Vector2Int coord, ushort[] points, ClashAnimation contestAnimation, Action recoloring, bool isRoadContest = false, bool isCityContest = false)
        {
            int i = 0;
            int maxAttempts = 6;
            while (i < maxAttempts)
            {
                GameObject tile = SessionManager.Instance.BoardManager.GetTileObject(coord.x, coord.y);
                if (tile)
                {
                    Vector3 position = tile.transform.position;
                    Vector3 offset = new Vector3(0, 1.5f, 0);
                    int winner;
                    if (points[0] > points[1])
                        winner = 0;
                    else if (points[0] < points[1])
                        winner = 1;
                    else
                        winner = -1;
                    contestAnimation.Initialize(position + offset, winner, points, recoloring, isRoadContest, isCityContest);
                    break;

                }
                i++;
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        public void RecolorStructures()
        {
            UnionFind unionFind = _managerContext.SessionContext.UnionFind;
            List<Structure> structures = unionFind.GetStructures();
                foreach (var structure in structures)
                {
                    bool isContested = structure.Contested;
                    foreach (var node in structure.Nodes)
                    {
                        var position = node.Position;
                        if(_managerContext.BoardManager.GetTileObject(position.x, position.y) == null)
                        {
                            continue;
                        }
                        
                        TileGenerator tileGenerator = _managerContext.BoardManager.GetTileObject(position.x, position.y).GetComponent<TileGenerator>();
                        TileData tileData = _managerContext.BoardManager.GetTileData(position.x, position.y);
                        int playerOwner;
                        if (isContested)
                        {
                            if (structure.Points[0] == structure.Points[1])
                            {
                                playerOwner = 3;
                            }
                            else
                            {
                                playerOwner = structure.Points[0] > structure.Points[1] ? 0 : 1;
                            }
                        }
                        else
                        {
                            playerOwner = _managerContext.SessionContext.Board.Tiles[position].PlayerSide;
                        }
                        if(node.Type == StructureType.City)
                        {
                            tileGenerator.RecolorHouses(playerOwner, isContested, (byte)tileData.Rotation);

                            if (isContested)
                            {
                                tileGenerator.ChangeEnvironmentForContest();
                                tileGenerator.tileParts.SetActiveWoodenArcs(false);
                                tileGenerator.tileParts.SetActiveWoodenBorderWall(false);
                            }
                        }
                        else if (node.Type == StructureType.Road)
                        {
                            tileGenerator.RecolorPinOnSide(playerOwner, (int)node.Side, isContested);
                            if (isContested)
                            {
                                TileParts tileParts = tileGenerator.CurrentTileGO.GetComponent<TileParts>();
                                tileParts.RoadRenderers[(int)node.Side].sprite =
                                    PrefabsManager.Instance.TileAssetsObject.GetContestedRoadByReference(tileParts.RoadRenderers[(int)node.Side].sprite);
                            }
                            
                        }
                        
                        
                        bool isCityContest = node.Type == StructureType.City && isContested;
                        bool isRoadContest = node.Type == StructureType.Road && isContested;
                        _managerContext.BoardManager.CheckAndConnectEdgeStructure(playerOwner, position.x, position.y,
                            structure.Type, isCityContest, isRoadContest);
                    }
                    
                }
        }
        
        private ClashAnimation CreateContestAnimation()
        {
            Vector3 offset = new Vector3(0, 0.5f, 0);
            GameObject contestAnimationGO = PrefabsManager.Instance.InstantiateObject(PrefabsManager.Instance.ClashAnimationPrefab);
            ClashAnimation contestAnimation = contestAnimationGO.GetComponent<ClashAnimation>();
            return contestAnimation;
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<Contested>(OnContest);
        }
    }
}