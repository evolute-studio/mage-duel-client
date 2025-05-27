using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Dojo.Starknet;
using TerritoryWars.Bots;
using TerritoryWars.Dojo;
using TerritoryWars.ExternalConnections;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.Tile;
using TerritoryWars.Tools;
using TerritoryWars.UI;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace TerritoryWars.General
{
    public class PlayerData
    {
        public string player_id;
        public string username;
        public int skin_id;

        public PlayerData(evolute_duel_Player model)
        {
            UpdatePlayerData(model);
        }
        
        public void UpdatePlayerData(evolute_duel_Player profile)
        {
            if (profile == null) return;
            player_id = profile.player_id.Hex();
            username = CairoFieldsConverter.GetStringFromFieldElement(profile.username);
            skin_id = profile.active_skin;
        }
    }
    
    public class SessionManagerOld : MonoBehaviour
    {
        public static SessionManagerOld Instance { get; private set; }

        public float StartDuration = 5f;

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError("SessionManager already exists. Deleting new instance.");
                Destroy(gameObject);
            }

            if (!CustomSceneManager.Instance.LoadingScreen.IsLoading)
            {
                CustomSceneManager.Instance.LoadingScreen.SetActive(true, 
                    () => DojoConnector.CancelGame(DojoGameManager.Instance.LocalAccount), 
                    LoadingScreen.connectingText);
            }
        }


        public BoardManager Board;
        [SerializeField] public GameUI gameUI;
        [SerializeField] public PlayerInfoUI sessionUI;
        [SerializeField] private DeckManager deckManager;
        public CloudsController CloudsController;
        public JokerManager JokerManager;
        public TileSelector TileSelector;
        public StructureHoverManager StructureHoverManager;

        public Vector3[] SpawnPoints;

        public Player[] Players;
        public PlayerData[] PlayersData;
        public Player CurrentTurnPlayer { get; private set; }
        public Player LocalPlayer { get; private set; }
        public Player RemotePlayer { get; private set; }

        public bool IsGameWithBot => DojoGameManager.Instance.DojoSessionManager.IsGameWithBot;
        public bool IsGameWithBotAsPlayer => DojoGameManager.Instance.DojoSessionManager.IsGameWithBotAsPlayer;
        public bool IsLocalPlayerTurn => CurrentTurnPlayer == LocalPlayer;
        public bool IsLocalPlayerHost => LocalPlayer.PlayerSide == 0;
        
        public bool IsSessionStarting { get; private set; } = true;

        public bool isPlayerMakeMove = false;
       

        public void Start()
        {
            Initialize();
            CustomSceneManager.Instance.LoadingScreen.SetActive(false);
        }

        public void Initialize()
        {
            InitializePlayers();
            InitializeBoard();
            DojoGameManager.Instance.DojoSessionManager.UpdateBoardAfterContests();
            //DojoGameManager.Instance.DojoSessionManager.UpdateBoardAfterRoadContest();
            //DojoGameManager.Instance.DojoSessionManager.UpdateBoardAfterCityContest();
            JokerManager = new JokerManager(this);
            gameUI.Initialize();
            sessionUI.Initialization();
            evolute_duel_Board board = DojoGameManager.Instance.DojoSessionManager.LocalPlayerBoard;
            int cityScoreBlue = board.blue_score.Item1;
            int cartScoreBlue = board.blue_score.Item2;
            int cityScoreRed = board.red_score.Item1;
            int cartScoreRed = board.red_score.Item2;
            GameUI.Instance.playerInfoUI.SetCityScores(cityScoreBlue, cityScoreRed, false);
            GameUI.Instance.playerInfoUI.SetRoadScores(cartScoreBlue, cartScoreRed, false);
            GameUI.Instance.playerInfoUI.SetPlayerScores(cityScoreBlue + cartScoreBlue, cityScoreRed + cartScoreRed, false);
            GameUI.Instance.playerInfoUI.SessionTimerUI.OnClientLocalPlayerTurnEnd.AddListener(ClientLocalPlayerSkip);
            GameUI.Instance.playerInfoUI.SessionTimerUI.OnOpponentPlayerTurnEnd.AddListener(ClientRemotePlayerSkip);
            JokerManager.Initialize(board);
            SetTilesInDeck(board.available_tiles_in_deck.Length);
            if (CheckGameStatus())
                CurrentTurnPlayer = Players[DojoGameManager.Instance.DojoSessionManager.WhoseMove()];
            else
            {
                FinishGame();
                return;
            }
            StartGame();
        }

        private void FinishGame()
        {
            DojoConnector.FinishGame(DojoGameManager.Instance.LocalAccount,
                DojoGameManager.Instance.DojoSessionManager.LocalPlayerBoard.id);
        }
        
        private bool CheckGameStatus()
        {
            int turns = DojoGameManager.Instance.DojoSessionManager.GetTurnCount();
            if (turns >= 2)
            {
                return false;
            }
            return true;
        }

        private void InitializeBoard()
        {
            evolute_duel_Board board = DojoGameManager.Instance.DojoSessionManager.LocalPlayerBoard;
            List<evolute_duel_Move> processedMoves = new List<evolute_duel_Move>();
            FieldElement lastMoveId = board.last_move_id switch
            {
                Option<FieldElement>.Some some => some.value,
                _ => null
            };
            SimpleStorage.SaveCurrentBoardId(board.id.Hex());
            Board.Initialize();
            if (lastMoveId != null)
            {
                evolute_duel_Move lastMove = DojoGameManager.Instance.GetMove(lastMoveId);
                int playerIndex = lastMove.player_side switch
                {
                    PlayerSide.Blue => 1,
                    PlayerSide.Red => 0,
                    _ => -1
                };
                CurrentTurnPlayer = Players[playerIndex];
                List<evolute_duel_Move> moves = DojoGameManager.Instance.GetMoves(new List<evolute_duel_Move>{lastMove});
                int moveNumber = 0;
                foreach (var move in moves)
                {
                    int owner = move.player_side switch
                    {
                        PlayerSide.Blue => 0,
                        PlayerSide.Red => 1,
                        _ => -1
                    };
                    string tileConfig = OnChainBoardDataConverter.GetTopTile(move.tile);
                    if (tileConfig == null) continue;
                    TileData tile = new TileData(tileConfig);
                    int rotation = move.rotation;
                    int x = move.col + 1;
                    int y = move.row + 1;

                    tile.Rotate((rotation + 3) % 4);
                    Board.PlaceTile(tile, x, y, owner);
                    processedMoves.Add(move);
                }

                GameObject[] allMoves = DojoGameManager.Instance.WorldManager.Entities<evolute_duel_Move>();
                foreach (var move in allMoves)
                {
                    evolute_duel_Move moveComponent = move.GetComponent<evolute_duel_Move>();
                    if (processedMoves.Contains(moveComponent)) continue;
                    IncomingModelsFilter.DestroyModel(moveComponent);
                }
            } 
        }

        private void InitializePlayers()
        {
            if (IsGameWithBot)
            {
                DojoGameManager.Instance.LocalBot.SessionStarted();
            }
            if(IsGameWithBotAsPlayer)
            {
                DojoGameManager.Instance.LocalBot.SessionStarted();
                DojoGameManager.Instance.LocalBotAsPlayer.SessionStarted();
            }
            
            Players = new Player[2];
            PlayersData = new PlayerData[2];

            
            Vector3[] leftCharacterPath = new Vector3[3];
            leftCharacterPath[0] = new Vector3(SpawnPoints[0].x, SpawnPoints[0].y + 15, 0); 
            leftCharacterPath[1] = new Vector3(SpawnPoints[0].x - 5, SpawnPoints[0].y + 7, 0); 
            leftCharacterPath[2] = SpawnPoints[0]; 

            
            Vector3[] rightCharacterPath = new Vector3[3];
            rightCharacterPath[0] = new Vector3(SpawnPoints[1].x, SpawnPoints[1].y + 15, 0);
            rightCharacterPath[1] = new Vector3(SpawnPoints[1].x + 5, SpawnPoints[1].y + 7, 0);
            rightCharacterPath[2] = SpawnPoints[1];
            
            evolute_duel_Board board = DojoGameManager.Instance.DojoSessionManager.LocalPlayerBoard;
            if (board == null)
            {
                CustomLogger.LogError("SessionManager.InitializePlayers() - board is null");
                CustomSceneManager.Instance.ForceLoadScene(CustomSceneManager.Instance.Menu);
                return;
            }

            evolute_duel_Player hostData = DojoGameManager.Instance.GetPlayerData(board.player1.Item1.Hex());
            evolute_duel_Player guestData = DojoGameManager.Instance.GetPlayerData(board.player2.Item1.Hex()); 

            GameObject hostPrefab = PrefabsManager.Instance.GetPlayer(hostData.active_skin); 
            GameObject guestPrefab = PrefabsManager.Instance.GetPlayer(guestData.active_skin);
            GameObject hostObject = Instantiate(hostPrefab, Vector3.zero, Quaternion.identity);
            GameObject guestObject = Instantiate(guestPrefab, Vector3.zero, Quaternion.identity);
            
            Players[0] = hostObject.GetComponent<Player>();
            Players[1] = guestObject.GetComponent<Player>();
            
            //Players[0].Initialize(board.player1.Item1, board.player1.Item2, board.player1.Item3);
            //Players[1].Initialize(board.player2.Item1, board.player2.Item2, board.player2.Item3);
            
            PlayersData[0] = new PlayerData(hostData);
            PlayersData[1] = new PlayerData(guestData);
            
            
            CurrentTurnPlayer = Players[0];
            
            LocalPlayer = Players[0].PlayerId == DojoGameManager.Instance.LocalAccount.Address.Hex() 
                ? Players[0] : Players[1];
            RemotePlayer = LocalPlayer == Players[0] ? Players[1] : Players[0];
            Players[0].SetAnimatorController(sessionUI.charactersObject.GetAnimatorController(PlayersData[0].skin_id));
            Players[1].SetAnimatorController(sessionUI.charactersObject.GetAnimatorController(PlayersData[1].skin_id));

            int hostIndex = SetLocalPlayerData.GetLocalIndex(0);
            int guestPlayerIndex = SetLocalPlayerData.GetLocalIndex(1);
            Players[hostIndex].transform.localScale = new Vector3(-0.7f, 0.7f, 1f);
            Players[hostIndex].transform.position = leftCharacterPath[0];
            Players[guestPlayerIndex].transform.position = rightCharacterPath[0];
            Players[hostIndex].transform
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
            sequence.Append(DOTween.To(() => camera.orthographicSize, x => camera.orthographicSize = x, endOrthographicSize, 3.5f));
            sequence.Play();
            

        }

        public void StartGame()
        {
            IsSessionStarting = false;
            CustomSceneManager.Instance.LoadingScreen.SetActive(false);
            int turnsCount = DojoGameManager.Instance.DojoSessionManager.GetTurnCount();
            ulong timeGone = (ulong)turnsCount * (ulong)DojoSessionManager.TurnDuration;
            GameUI.Instance.playerInfoUI.SessionTimerUI.StartTurnTimer(DojoGameManager.Instance.DojoSessionManager.LastMoveTimestamp + timeGone, IsLocalPlayerTurn);
            Invoke(nameof(StartTurn), 2f);

            DojoGameManager.Instance.DojoSessionManager.OnMoveReceived += HandleMove;
            DojoGameManager.Instance.DojoSessionManager.OnSkipMoveReceived += SkipMove;
        }

        public void StartTurn()
        {
            if (!CheckGameStatus())
            {
                FinishGame();
                return;
            }
            
            gameUI.SetActiveSkipButtonPulse(false);
            
            if (CurrentTurnPlayer == LocalPlayer)
            {
                StartLocalTurn();
            }
            else
            {
                StartRemoteTurn();
            }
        }

        private void StartLocalTurn()
        {
            isPlayerMakeMove = false;
            CustomLogger.LogDojoLoop("StartLocalTurn");
            if (IsGameWithBotAsPlayer)
            {
                DojoGameManager.Instance.LocalBotAsPlayer.MakeMove();
            }
            
            UpdateTile();
            LocalPlayer.StartSelecting();
            evolute_duel_Board board = DojoGameManager.Instance.WorldManager.Entities<evolute_duel_Board>().First().GetComponent<evolute_duel_Board>();
            Players[0].UpdateData(board.player1.Item3);
            Players[1].UpdateData(board.player2.Item3);
            
            gameUI.SetEndTurnButtonActive(false);
            gameUI.SetRotateButtonActive(false);
            gameUI.SetSkipTurnButtonActive(true);

            TileData currentTile = GetNextTile();
            currentTile.OwnerId = LocalPlayer.PlayerSide;
            TileSelector.SetCurrentTile(_nextTile);
            if (TileSelector.IsExistValidPlacement(currentTile))
            {
                TileSelector.StartTilePlacement(currentTile);
            }
            else
            {
                if (CurrentTurnPlayer.JokerCount > 0)
                {
                    gameUI.OnJokerButtonClicked();
                }
                else
                {
                    gameUI.SetActiveSkipButtonPulse(true);
                }
            }
            
            gameUI.SetActiveDeckContainer(true);
        }

        private void StartRemoteTurn()
        {
            CustomLogger.LogDojoLoop("StartRemoteTurn");
            if (IsGameWithBot || IsGameWithBotAsPlayer)
            {
                DojoGameManager.Instance.LocalBot.MakeMove();
            }
            
            UpdateTile();
            TileSelector.SetCurrentTile(GetNextTile());
            RemotePlayer.StartSelecting();
            evolute_duel_Board board = DojoGameManager.Instance.DojoSessionManager.LocalPlayerBoard;
            Players[0].UpdateData(board.player1.Item3);
            Players[1].UpdateData(board.player2.Item3);
            
            gameUI.SetEndTurnButtonActive(false);
            gameUI.SetRotateButtonActive(false);
            gameUI.SetSkipTurnButtonActive(false);
            gameUI.SetActiveDeckContainer(false);
        }

        private void HandleMove(string playerAddress, TileData tile, Vector2Int position, int rotation, bool isJoker)
        {
            evolute_duel_Player player = DojoGameManager.Instance.GetPlayerData(playerAddress);
            if(playerAddress == Players[0].PlayerId &&  player != null)
            {
                PlayersData[0].UpdatePlayerData(player);
            }
            else if(playerAddress == Players[1].PlayerId &&  player != null)
            {
                PlayersData[1].UpdatePlayerData(player);
            }
            if(IsGameWithBotAsPlayer)
            {
                StartCoroutine(HandleOpponentMoveCoroutine(playerAddress, tile, position, rotation));
            }
            else
            {
                if (playerAddress == LocalPlayer.PlayerId) CompleteEndTurn(playerAddress);
                else StartCoroutine(HandleOpponentMoveCoroutine(playerAddress, tile, position, rotation));
            }
            GameUI.Instance.playerInfoUI.SessionTimerUI.StartTurnTimer(DojoGameManager.Instance.DojoSessionManager.LastMoveTimestamp, playerAddress != LocalPlayer.PlayerId);
        }
        
        private void SkipMove(string playerAddress)
        {
            if (CurrentTurnPlayer.PlayerId != playerAddress) return;
            CustomLogger.LogDojoLoop("Skip for current player");
            foreach (var player in Players)
            {
                if(player.PlayerId == playerAddress)
                {
                    player.PlaySkippedBubbleAnimation();
                    break;
                }
            }
            GameUI.Instance.SetJokerMode(false);    
            TileSelector.EndTilePlacement();
            CurrentTurnPlayer.EndTurn();
            CompleteEndTurn(playerAddress, 5f);
            GameUI.Instance.playerInfoUI.SessionTimerUI.StartTurnTimer(DojoGameManager.Instance.DojoSessionManager.LastMoveTimestamp, playerAddress != LocalPlayer.PlayerId);
        }

        public void ClientLocalPlayerSkip()
        {
            CustomLogger.LogDojoLoop($"[ClientLocalPlayerSkip] {LocalPlayer.PlayerId}");
            DojoGameManager.Instance.DojoSessionManager.LocalSkipped(LocalPlayer.PlayerId);
            DojoGameManager.Instance.DojoSessionManager.SkipMove();
            TileSelector.tilePreview.ResetPosition();
        }
        
        public void ClientRemotePlayerSkip()
        {
            CustomLogger.LogDojoLoop($"[ClientRemotePlayerSkip] {RemotePlayer.PlayerId}");
            DojoGameManager.Instance.DojoSessionManager.LocalSkipped(RemotePlayer.PlayerId);
        }

        private IEnumerator HandleOpponentMoveCoroutine(string playerAddress, TileData tile, Vector2Int position, int rotation)
        {
            tile.Rotate(rotation);
            tile.OwnerId = RemotePlayer.PlayerSide;
            TileSelector.SetCurrentTile(tile);
            TileSelector.tilePreview.SetPosition(position.x + 1, position.y + 1);
            yield return new WaitForSeconds(0.3f);
            TileSelector.tilePreview.PlaceTile(RemotePlayer.PlayerSide,tile, () =>
            {
                Board.PlaceTile(tile, position.x + 1, position.y + 1, GetPlayerByAddress(playerAddress).PlayerSide);
            });
            yield return new WaitForSeconds(0.5f);
            CurrentTurnPlayer.EndTurn();
            yield return new WaitForSeconds(0.5f);
            TileSelector.tilePreview.ResetPosition();
            CompleteEndTurn(playerAddress);
        }

        private TileData _nextTile;
        public void UpdateTile()
        {
            _nextTile = GetNextTile();
            _nextTile.OwnerId = RemotePlayer.PlayerSide;
        }
        
        public TileData GetNextTile()
        {
            return _nextTile ??= DojoGameManager.Instance.DojoSessionManager.GetTopTile();
        }

        public void SetNextTile(TileData tile)
        {
            _nextTile = tile;
        }

        public void SetTilesInDeck(int count)
        {
            gameUI.playerInfoUI.SetDeckCount(count);
        }

        public void RotateCurrentTile()
        {
            TileSelector.RotateCurrentTile();
        }

        public void EndTurn()
        {
            if (TileSelector.CurrentTile != null && CurrentTurnPlayer == LocalPlayer)
            {
                TileSelector.PlaceCurrentTile();
                TileSelector.ClearHighlights();
            }
        }

        public void CompleteEndTurn(string lastMovePlayerAddress, float delay = 1f)
        {
            bool isLocalPlayer = lastMovePlayerAddress == LocalPlayer.PlayerId;
            CurrentTurnPlayer = isLocalPlayer ? RemotePlayer : LocalPlayer;
            gameUI.SetEndTurnButtonActive(false);
            Invoke(nameof(StartTurn), delay);
        }
        
        public Player GetPlayerByAddress(string address)
        {
            return Players.FirstOrDefault(player => player.PlayerId == address);
        }
    
        public int GetLocalIdByAddress(FieldElement address)
        {
            return Players.FirstOrDefault(player => player.PlayerId == address.Hex())?.PlayerSide ?? -1;
        }

        private void OnDestroy()
        {
            DojoGameManager.Instance.DojoSessionManager.OnMoveReceived -= HandleMove;
            DojoGameManager.Instance.DojoSessionManager.OnSkipMoveReceived -= SkipMove;
            GameUI.Instance.playerInfoUI.SessionTimerUI.OnClientLocalPlayerTurnEnd.RemoveListener(ClientLocalPlayerSkip);
            GameUI.Instance.playerInfoUI.SessionTimerUI.OnOpponentPlayerTurnEnd.RemoveListener(ClientRemotePlayerSkip);
        }

        public void OnGUI()
        {
            Bot bot = DojoGameManager.Instance.LocalBot;
            if (bot != null && bot.IsDebug && bot.DebugModule != null)
            {
                bot.DebugModule.OnGUI();
            }
        }
    }
}