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
    
    public class SessionManager : MonoBehaviour
    {
        public static SessionManager Instance { get; private set; }

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
                    () => DojoConnector.CancelGame(DojoGameManager.Instance.LocalBurnerAccount), 
                    LoadingScreen.connectingText);
            }
        }


        public Board Board;
        [SerializeField] private GameUI gameUI;
        [SerializeField] private PlayerInfoUI sessionUI;
        [SerializeField] private DeckManager deckManager;
        public JokerManager JokerManager;
        public TileSelector TileSelector;

        public Vector3[] SpawnPoints;

        public Character[] Players;
        public PlayerData[] PlayersData;
        public Character CurrentTurnPlayer { get; private set; }
        public Character LocalPlayer { get; private set; }
        public Character RemotePlayer { get; private set; }

        public bool IsGameWithBot => DojoGameManager.Instance.SessionManager.IsGameWithBot;
        public bool IsLocalPlayerTurn => CurrentTurnPlayer == LocalPlayer;
        public bool IsLocalPlayerHost => LocalPlayer.LocalId == 0;
        
        
        
       

        public void Start()
        {
            Initialize();
            CustomSceneManager.Instance.LoadingScreen.SetActive(false);
        }

        public void Initialize()
        {
            InitializePlayers();
            InitializeBoard();
            DojoGameManager.Instance.SessionManager.UpdateBoardAfterRoadContest();
            DojoGameManager.Instance.SessionManager.UpdateBoardAfterCityContest();
            JokerManager = new JokerManager(this);
            gameUI.Initialize();
            sessionUI.Initialization();
            evolute_duel_Board board = DojoGameManager.Instance.SessionManager.LocalPlayerBoard;
            int cityScoreBlue = board.blue_score.Item1;
            int cartScoreBlue = board.blue_score.Item2;
            int cityScoreRed = board.red_score.Item1;
            int cartScoreRed = board.red_score.Item2;
            GameUI.Instance.playerInfoUI.SetCityScores(cityScoreBlue, cityScoreRed);
            GameUI.Instance.playerInfoUI.SetRoadScores(cartScoreBlue, cartScoreRed);
            GameUI.Instance.playerInfoUI.SetPlayerScores(cityScoreBlue + cartScoreBlue, cityScoreRed + cartScoreRed);
            GameUI.Instance.playerInfoUI.SessionTimerUI.OnLocalPlayerTurnEnd.AddListener(LocalSkipMove);
            GameUI.Instance.playerInfoUI.SessionTimerUI.OnOpponentPlayerTurnEnd.AddListener(LocalSkipMove);
            JokerManager.Initialize(board);
            SetTilesInDeck(board.available_tiles_in_deck.Length);
            StartGame();
        }

        private void InitializeBoard()
        {
            evolute_duel_Board board = DojoGameManager.Instance.SessionManager.LocalPlayerBoard;
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
                CustomLogger.LogImportant("SessionManager.Initialize() - lastMoveId != null");
                evolute_duel_Move lastMove = DojoGameManager.Instance.GetMove(lastMoveId);
                int playerIndex = lastMove.player_side switch
                {
                    PlayerSide.Blue => 1,
                    PlayerSide.Red => 0,
                    _ => -1
                };
                CurrentTurnPlayer = Players[playerIndex];
                List<evolute_duel_Move> moves = DojoGameManager.Instance.GetMoves(new List<evolute_duel_Move>{lastMove});
                CustomLogger.LogImportant("SessionManager.Initialize() - moves.Count: " + moves.Count);
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
            
            Players = new Character[2];
            PlayersData = new PlayerData[2];

            
            Vector3[] leftCharacterPath = new Vector3[3];
            leftCharacterPath[0] = new Vector3(SpawnPoints[0].x, SpawnPoints[0].y + 15, 0); 
            leftCharacterPath[1] = new Vector3(SpawnPoints[0].x - 5, SpawnPoints[0].y + 7, 0); 
            leftCharacterPath[2] = SpawnPoints[0]; 

            
            Vector3[] rightCharacterPath = new Vector3[3];
            rightCharacterPath[0] = new Vector3(SpawnPoints[1].x, SpawnPoints[1].y + 15, 0);
            rightCharacterPath[1] = new Vector3(SpawnPoints[1].x + 5, SpawnPoints[1].y + 7, 0);
            rightCharacterPath[2] = SpawnPoints[1];
            
            evolute_duel_Board board = DojoGameManager.Instance.SessionManager.LocalPlayerBoard;

            evolute_duel_Player hostData = DojoGameManager.Instance.GetPlayerData(board.player1.Item1.Hex());
            evolute_duel_Player guestData = DojoGameManager.Instance.GetPlayerData(board.player2.Item1.Hex()); 

            GameObject hostPrefab = PrefabsManager.Instance.GetPlayer(hostData.active_skin); 
            GameObject guestPrefab = PrefabsManager.Instance.GetPlayer(guestData.active_skin);
            GameObject hostObject = Instantiate(hostPrefab, Vector3.zero, Quaternion.identity);
            GameObject guestObject = Instantiate(guestPrefab, Vector3.zero, Quaternion.identity);
            
            Players[0] = hostObject.GetComponent<Character>();
            Players[1] = guestObject.GetComponent<Character>();
            
            Players[0].Initialize(board.player1.Item1, board.player1.Item2, board.player1.Item3);
            Players[1].Initialize(board.player2.Item1, board.player2.Item2, board.player2.Item3);
            
            PlayersData[0] = new PlayerData(hostData);
            PlayersData[1] = new PlayerData(guestData);
            
            
            CurrentTurnPlayer = Players[0];
            
            LocalPlayer = Players[0].Address.Hex() == DojoGameManager.Instance.LocalBurnerAccount.Address.Hex() 
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
                .DOPath(leftCharacterPath, 2.5f, PathType.CatmullRom)
                .SetEase(Ease.OutQuad);

            Players[guestPlayerIndex].transform
                .DOPath(rightCharacterPath, 2.5f, PathType.CatmullRom)
                .SetEase(Ease.OutQuad);
            
            Camera camera = Camera.main;
            float startOrthographicSize = 5.5f;
            float endOrthographicSize = 4f;
            camera.orthographicSize = startOrthographicSize;
            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(0.5f);
            sequence.Append(DOTween.To(() => camera.orthographicSize, x => camera.orthographicSize = x, endOrthographicSize, 2.5f));
            sequence.Play();
            

        }

        public void StartGame()
        {
            CustomSceneManager.Instance.LoadingScreen.SetActive(false);
            Invoke(nameof(StartTurn), 2f);

            DojoGameManager.Instance.SessionManager.OnMoveReceived += HandleMove;
            DojoGameManager.Instance.SessionManager.OnSkipMoveReceived += SkipMove;
        }

        private void StartTurn()
        {
            GameUI.Instance.playerInfoUI.SessionTimerUI.StartTurnTimer();
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
            UpdateTile();
            LocalPlayer.StartSelecting();
            evolute_duel_Board board = DojoGameManager.Instance.WorldManager.Entities<evolute_duel_Board>().First().GetComponent<evolute_duel_Board>();
            Players[0].UpdateData(board.player1.Item3);
            Players[1].UpdateData(board.player2.Item3);
            
            gameUI.SetEndTurnButtonActive(false);
            gameUI.SetRotateButtonActive(false);
            gameUI.SetSkipTurnButtonActive(true);

            TileData currentTile = DojoGameManager.Instance.SessionManager.GetTopTile();
            currentTile.OwnerId = LocalPlayer.LocalId;
            TileSelector.StartTilePlacement(currentTile);
            gameUI.SetActiveDeckContainer(true);
        }

        private void StartRemoteTurn()
        {
            if (IsGameWithBot)
            {
                DojoGameManager.Instance.LocalBot.MakeMove();
            }
            
            UpdateTile();
            RemotePlayer.StartSelecting();
            evolute_duel_Board board = DojoGameManager.Instance.SessionManager.LocalPlayerBoard;
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
            if(playerAddress == Players[0].Address.Hex() &&  player != null)
            {
                PlayersData[0].UpdatePlayerData(player);
            }
            else if(playerAddress == Players[1].Address.Hex() &&  player != null)
            {
                PlayersData[1].UpdatePlayerData(player);
            }
            if (playerAddress == LocalPlayer.Address.Hex()) CompleteEndTurn(playerAddress);
            else StartCoroutine(HandleOpponentMoveCoroutine(playerAddress, tile, position, rotation));
        }
        
        private void SkipMove(string playerAddress)
        {
            //if (CurrentTurnPlayer.Address.Hex() != playerAddress) return;
            GameUI.Instance.SetJokerMode(false);
            TileSelector.EndTilePlacement();
            CurrentTurnPlayer.EndTurn();
            CompleteEndTurn(playerAddress);
        }

        private IEnumerator HandleOpponentMoveCoroutine(string playerAddress, TileData tile, Vector2Int position, int rotation)
        {
            tile.Rotate(rotation);
            tile.OwnerId = RemotePlayer.LocalId;
            TileSelector.SetCurrentTile(tile);
            TileSelector.tilePreview.SetPosition(position.x + 1, position.y + 1);
            yield return new WaitForSeconds(0.3f);
            TileSelector.tilePreview.PlaceTile(() =>
            {
                Board.PlaceTile(tile, position.x + 1, position.y + 1, RemotePlayer.LocalId);
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
            _nextTile ??= DojoGameManager.Instance.SessionManager.GetTopTile();
            _nextTile.OwnerId = RemotePlayer.LocalId;
            TileSelector.SetCurrentTile(_nextTile);
            CustomLogger.LogImportant("UpdateTile. Tile: " + _nextTile.id);
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
        
        public void LocalSkipMove()
        {
            GameUI.Instance.SetEndTurnButtonActive(false);
            TileSelector.ClearHighlights();
            TileSelector.tilePreview.ResetPosition();
            if (IsLocalPlayerTurn)
            {
                DojoGameManager.Instance.SessionManager.SkipMove();
            }
            else
            {
                CompleteEndTurn(RemotePlayer.Address.Hex());
            }
        }

        public void CompleteEndTurn(string lastMovePlayerAddress)
        {
            bool isLocalPlayer = lastMovePlayerAddress == LocalPlayer.Address.Hex();
            CurrentTurnPlayer = isLocalPlayer ? RemotePlayer : LocalPlayer;
            gameUI.SetEndTurnButtonActive(false);
            Invoke(nameof(StartTurn), 1f);
        }
    
        public int GetLocalIdByAddress(FieldElement address)
        {
            return Players.FirstOrDefault(player => player.Address.Hex() == address.Hex())?.LocalId ?? -1;
        }

        private void OnDestroy()
        {
            DojoGameManager.Instance.SessionManager.OnMoveReceived -= HandleMove;
            DojoGameManager.Instance.SessionManager.OnSkipMoveReceived -= SkipMove;
            GameUI.Instance.playerInfoUI.SessionTimerUI.OnLocalPlayerTurnEnd.RemoveListener(LocalSkipMove);
            GameUI.Instance.playerInfoUI.SessionTimerUI.OnOpponentPlayerTurnEnd.RemoveListener(LocalSkipMove);
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