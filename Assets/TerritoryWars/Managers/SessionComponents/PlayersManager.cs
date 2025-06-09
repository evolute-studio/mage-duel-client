using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TerritoryWars.ConnectorLayers.Dojo;
using TerritoryWars.DataModels;
using TerritoryWars.Dojo;
using TerritoryWars.General;
using TerritoryWars.UI;
using UnityEngine;

namespace TerritoryWars.Managers.SessionComponents
{
    public class PlayersManager: ISessionComponent
    {
        private SessionManagerContext _managerContext;

        public Player[] Players { get; private set; }
        
        
        private Vector3[] SpawnPoints => _managerContext.SessionContext.SpawnPoints;

        public void Initialize(SessionManagerContext managerContext)
        {
            _managerContext = managerContext;
            SpawnPlayers();
            ShowPlayers();
        }
        
        public void SpawnPlayers()
        {
            if (_managerContext.SessionContext.IsGameWithBot)
            {
                DojoGameManager.Instance.LocalBot.SessionStarted();
            }
            if(_managerContext.SessionContext.IsGameWithBotAsPlayer)
            {
                DojoGameManager.Instance.LocalBot.SessionStarted();
                DojoGameManager.Instance.LocalBotAsPlayer.SessionStarted();
            }
            
            Players = new Player[2];
            ref Board board = ref _managerContext.SessionContext.Board;
            GameObject hostPrefab = PrefabsManager.Instance.GetPlayer(board.Player1.ActiveSkin);
            GameObject guestPrefab = PrefabsManager.Instance.GetPlayer(board.Player2.ActiveSkin);
            GameObject hostObject = Object.Instantiate(hostPrefab, Vector3.zero, Quaternion.identity);
            GameObject guestObject = Object.Instantiate(guestPrefab, Vector3.zero, Quaternion.identity);

            Players[0] = hostObject.GetComponent<Player>();
            Players[1] = guestObject.GetComponent<Player>();

            Players[0].Initialize(_managerContext.SessionContext.PlayersData[0]);
            Players[1].Initialize(_managerContext.SessionContext.PlayersData[1]);
            
            _managerContext.SessionContext.Players = Players;
            _managerContext.SessionContext.LocalPlayer = Players[0].PlayerId == DojoLayer.Instance.LocalPlayerId 
                ? Players[0] : Players[1];
            _managerContext.SessionContext.RemotePlayer = _managerContext.SessionContext.LocalPlayer.PlayerId == Players[0].PlayerId 
                ? Players[1] : Players[0];
        }
        

        public void ShowPlayers()
        {
            int localIndex = SetLocalPlayerData.GetLocalIndex(0);
            int guestPlayerIndex = SetLocalPlayerData.GetLocalIndex(1);
            
            Vector3[] leftCharacterPath = new Vector3[3];
            leftCharacterPath[0] = new Vector3(SpawnPoints[0].x, SpawnPoints[0].y + 15, 0);
            leftCharacterPath[1] = new Vector3(SpawnPoints[0].x - 5, SpawnPoints[0].y + 7, 0);
            leftCharacterPath[2] = SpawnPoints[0];


            Vector3[] rightCharacterPath = new Vector3[3];
            rightCharacterPath[0] = new Vector3(SpawnPoints[1].x, SpawnPoints[1].y + 15, 0);
            rightCharacterPath[1] = new Vector3(SpawnPoints[1].x + 5, SpawnPoints[1].y + 7, 0);
            rightCharacterPath[2] = SpawnPoints[1];

            PlayerInfoUI playerInfoUI = GameUI.Instance.playerInfoUI;
            Players[localIndex]
                .SetAnimatorController(playerInfoUI.charactersObject.GetAnimatorController(Players[localIndex].ActiveSkin));
            Players[guestPlayerIndex]
                .SetAnimatorController(playerInfoUI.charactersObject.GetAnimatorController(Players[guestPlayerIndex].ActiveSkin));

            
            Players[localIndex].transform.localScale = new Vector3(-0.7f, 0.7f, 1f);
            Players[localIndex].transform.position = leftCharacterPath[0];
            Players[guestPlayerIndex].transform.position = rightCharacterPath[0];
            
            Players[localIndex].transform
                .DOPath(leftCharacterPath, 2f, PathType.CatmullRom)
                .SetEase(Ease.OutQuad);

            Players[guestPlayerIndex].transform
                .DOPath(rightCharacterPath, 2f, PathType.CatmullRom)
                .SetEase(Ease.OutQuad);

            Camera camera = Camera.main;
            float startOrthographicSize = 7f;
            float endOrthographicSize = 4.5f;
            camera.orthographicSize = startOrthographicSize;
            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(0.5f);
            sequence.Append(DOTween.To(() => camera.orthographicSize, x => camera.orthographicSize = x,
                endOrthographicSize, 3.5f));
            sequence.Play();
        }

        public void Dispose()
        {
            
        }
    }
}