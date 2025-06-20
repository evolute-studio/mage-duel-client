using System.Collections;
using System.Collections.Generic;
using TerritoryWars.DataModels.ClientEvents;
using TerritoryWars.Dojo;
using TerritoryWars.ExternalConnections;
using TerritoryWars.Managers.SessionComponents;
using TerritoryWars.Tile;
using TerritoryWars.Tools;
using TerritoryWars.UI;
using TerritoryWars.UI.Session;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace TerritoryWars.General
{
    public class TileSelector : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BoardManager board;
        [SerializeField] private GameUI gameUI;
        [SerializeField] private Material highlightMaterial;
        [SerializeField] private LayerMask backgroundLayer;
        [SerializeField] private LayerMask hoverLayer;
        [SerializeField] private Sprite highlightSprite;
        [SerializeField] private Sprite highlightUpperBorderSprite;
        [SerializeField] public TilePreview tilePreview;
        [SerializeField] private TileJokerAnimator TileJokerAnimator;
        [SerializeField] private TileJokerAnimator TileJokerAnimatorUI;


        [Header("Colors")] 
        [SerializeField] private Color normalHighlightColor = new Color(1f, 1f, 0f, 0.3f);
        [SerializeField] private Color selectedHighlightColor = new Color(0f, 1f, 0f, 0.3f);
        [SerializeField] private Color invalidHighlightColor = new Color(1f, 0f, 0f, 0.3f);

        private TileData currentTile;
        private GameObject highlightedTiles;
        private List<int> currentValidRotations;
        public Vector2Int? selectedPosition;
        private bool isPlacingTile;
        private string initialTileConfig;
        private bool isJokerMode = false;
        private Vector2Int? jokerPosition;
        private List<ValidPlacement> _currentValidPlacements;
        private bool onEnterHoverObject = false;

        public TileData CurrentTile => currentTile;
        
        public UnityEvent OnTurnStarted = new UnityEvent();
        public UnityEvent OnTurnEnding = new UnityEvent();

        [SerializeField] private float highlightYOffset = -0.08f;
        public (TileData, Vector2Int) LastMove { get; private set; }

        private void Awake()
        {
            highlightedTiles = new GameObject("HighlightedTiles");
            highlightedTiles.transform.parent = transform;
        }

        public void Start()
        {
            EventBus.Subscribe<ClientInput>(OnClientInput);
        }

        private void OnClientInput(ClientInput input)
        {
            if (input.Type == ClientInput.InputType.RotateTile) 
            {
                RotateCurrentTile();
            }
        }

        private void Update()
        {
            // #if UNITY_EDITOR
            // if (Input.GetKeyDown(KeyCode.Escape))
            // {
            //     StartTilePlacement(new TileData("FFFF"));
            // }
            // #endif
            
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }


                if (isPlacingTile)// && SessionManager.Instance.IsLocalPlayerTurn)
                {
                    HandleTilePlacement();
                    
                }
            }
            HandleTileHover();
        }

        private void HandleTileHover()
        {
            if (CursorManager.Instance != null)
            {
               
                var hit = Physics2D.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition).origin,
                    Vector2.zero, Mathf.Infinity, hoverLayer);

                if (hit.Length > 0)
                {
                    onEnterHoverObject = true;
                    CursorManager.Instance.SetCursor("pointer");
                }
                else if (onEnterHoverObject)
                {
                    onEnterHoverObject = false;
                    CursorManager.Instance.SetCursor("default");
                }
            }
        }
    

        private void HandleTilePlacement()
        {
            var hit = Physics2D.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition).origin,
                Vector2.zero, Mathf.Infinity, backgroundLayer);

            if (hit.collider != null && hit.collider.name.StartsWith("Highlight_"))
            {
                string[] coordinates = hit.collider.name.Split('_');
                if (coordinates.Length == 3 &&
                    int.TryParse(coordinates[1], out int x) &&
                    int.TryParse(coordinates[2], out int y))
                {
                    OnTileClicked(x, y);
                }
            }
        }
        public void SetCurrentTile(TileData tile)
        {
            currentTile = tile;
            gameUI.UpdateUI();
            tilePreview.UpdatePreview(tile);
            currentTile.HouseSprites.AddRange(tilePreview.HouseSprites);
        }
        public bool IsExistValidPlacement(TileData tile)
        {
            return board.GetValidPlacements(tile).Count > 0;
        }
        
        public void StartTilePlacement(TileData tile)
        {
            GameUI.Instance.SetEndTurnButtonActive(false);
            tilePreview.ResetPosition();
            tilePreview.UpdatePreview(tile);
            currentTile = tile;
            initialTileConfig = tile.RotatedConfig;
            isPlacingTile = true;
            selectedPosition = null;

            _currentValidPlacements = board.GetValidPlacements(currentTile);
            ShowPossiblePlacements(_currentValidPlacements);
            gameUI.SetEndTurnButtonActive(false);
            gameUI.SetRotateButtonActive(false);
            gameUI.UpdateUI();
        }

        public void ShowPossiblePlacements(List<ValidPlacement> placements)
        {
            ClearHighlights();
            foreach (var placement in placements)
            {
                CreateHighlight(placement.x, placement.y);
            }
            SetHighlightColor(normalHighlightColor);
        }

        private void CreateHighlight(int x, int y)
        {
            var highlight = new GameObject($"Highlight_{x}_{y}");
            var highlightUpperBorder = new GameObject("HighlightUpperBorder");
            highlight.transform.SetParent(highlightedTiles.transform);
            highlightUpperBorder.transform.SetParent(highlight.transform);
            highlight.transform.position = BoardManager.GetTilePosition(x, y) + new Vector3(0f, highlightYOffset, 0f);
            highlightUpperBorder.transform.position = BoardManager.GetTilePosition(x, y) + new Vector3(0f, highlightYOffset, 0f);
            

            var spriteRenderer = highlight.AddComponent<SpriteRenderer>();
            var highlighUpperBorderSpriteRenderer = highlightUpperBorder.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = highlightSprite;
            spriteRenderer.material = highlightMaterial;
            spriteRenderer.sortingOrder = 7;
            highlighUpperBorderSpriteRenderer.sprite = highlightUpperBorderSprite;
            highlighUpperBorderSpriteRenderer.material = highlightMaterial;
            highlighUpperBorderSpriteRenderer.sortingOrder = 12;
            if (spriteRenderer.gameObject.layer != LayerMask.NameToLayer("BackgroundBoard"))
            {
                spriteRenderer.gameObject.layer = LayerMask.NameToLayer("BackgroundBoard");
            }
     
            // add this points
            //GenericPropertyJSON:{"name":"data","type":-1,"arraySize":6,"arrayType":"Vector2","children":[{"name":"Array","type":-1,"arraySize":6,"arrayType":"Vector2","children":[{"name":"size","type":12,"val":6},{"name":"data","type":8,"children":[{"name":"x","type":2,"val":0},{"name":"y","type":2,"val":0.344967842}]},{"name":"data","type":8,"children":[{"name":"x","type":2,"val":-0.632778168},{"name":"y","type":2,"val":0.008231878}]},{"name":"data","type":8,"children":[{"name":"x","type":2,"val":-0.634768546},{"name":"y","type":2,"val":-0.158878535}]},{"name":"data","type":8,"children":[{"name":"x","type":2,"val":0.0108075738},{"name":"y","type":2,"val":-0.484888434}]},{"name":"data","type":8,"children":[{"name":"x","type":2,"val":0.642678857},{"name":"y","type":2,"val":-0.176443338}]},{"name":"data","type":8,"children":[{"name":"x","type":2,"val":0.6466036},{"name":"y","type":2,"val":0.0137912631}]}]}]}
            highlight.AddComponent<PolygonCollider2D>();
            var points = new Vector2[6];
            points[0] = new Vector2(0, 0.404967842f);
            points[1] = new Vector2(-0.632778168f, 0.068231878f);
            points[2] = new Vector2(-0.634768546f, -0.098878535f);
            points[3] = new Vector2(0.0108075738f, -0.424888434f);
            points[4] = new Vector2(0.642678857f, -0.116443338f);
            points[5] = new Vector2(0.6466036f, 0.0737912631f);
            highlight.GetComponent<PolygonCollider2D>().points = points;
            var clickHandler = highlight.AddComponent<TileClickHandler>();
            clickHandler.Initialize(this, x, y);
        }
        
        private IEnumerator InvokeActionWithDelay(float delay, System.Action action)
        {
            yield return new WaitForSeconds(delay);
            action();
        }

        public void OnTileClicked(int x, int y)
        {
            if (isJokerMode)
            {
                if (IsValidJokerPosition(x, y))
                {
                    
                    if (selectedPosition.HasValue && selectedPosition.Value == new Vector2Int(x, y))
                    {
                        return;
                    }

                    
                    UpdateHighlights(x, y);
                    selectedPosition = new Vector2Int(x, y);
                    tilePreview.SetPosition(new Vector2Int(x, y));
                    
                    TileJokerAnimator.EvoluteTileDisappear();
                    TileJokerAnimatorUI.EvoluteTileDisappear();
                    gameUI.SetRotateButtonActive(true);
                    
                    
                    StartCoroutine(InvokeActionWithDelay(0.8f, () =>
                    {
                        TileJokerAnimator.JokerConfChanging(x, y);
                    }));
                    return;
                }
                return;
            }

            if (!isPlacingTile) return;

                if (selectedPosition.HasValue && selectedPosition.Value == new Vector2Int(x, y) && !IsPossiblePosition(x,y))
                {
                    return;
                }

                if(!IsPossiblePosition(x,y)) return;

                if (selectedPosition.HasValue)
                {
                    currentTile.UpdateData(initialTileConfig, new Vector2Int(x, y));
                    selectedPosition = null;
                    currentValidRotations = null;
                    SetHighlightColor(normalHighlightColor);
                }

                currentValidRotations = board.GetValidRotations(currentTile, x, y);
                if (currentValidRotations.Count == 0)
                {
                    return;
                }

                if (!selectedPosition.HasValue)
                {
                    OnTurnStarted.Invoke();
                }

                if (currentValidRotations.Count > 1)
                {
                    HashSet<string> uniqueConfigs = new HashSet<string>();
                    List<int> uniqueRotations = new List<int>();

                    foreach (int rotation in currentValidRotations)
                    {
                        string currentConfig = currentTile.RotatedConfig;

                        while (currentTile.Rotation != rotation)
                        {
                            currentTile.Rotate();
                        }

                        string config = currentTile.RotatedConfig;
                        if (!uniqueConfigs.Contains(config))
                        {
                            uniqueConfigs.Add(config);
                            uniqueRotations.Add(rotation);
                        }

                        currentTile.UpdateData(currentConfig, new Vector2Int(x, y));
                    }

                    currentValidRotations = uniqueRotations;
                }

                UpdateHighlights(x, y);
                selectedPosition = new Vector2Int(x, y);
                RotateToFirstValidRotation();
                
                EventBus.Publish(new TileSelected(x, y));
                gameUI.SetRotateButtonActive(currentValidRotations.Count > 1);
                gameUI.SetEndTurnButtonActive(true);
            
        }

        private bool IsPossiblePosition(int x, int y)
        {
            foreach (var position in _currentValidPlacements)
            {
                if (position.x == x && position.y == y)
                {
                    return true;
                }
            }

            return false;
        }

        private void UpdateHighlights(int selectedX, int selectedY)
        {
            foreach (Transform child in highlightedTiles.transform)
            {
                child.GetComponent<SpriteRenderer>().color =
                    child.name == $"Highlight_{selectedX}_{selectedY}"
                        ? selectedHighlightColor
                        : normalHighlightColor;
                
                child.GetChild(0).GetComponent<SpriteRenderer>().color =
                    child.name == $"Highlight_{selectedX}_{selectedY}"
                        ? selectedHighlightColor
                        : normalHighlightColor;
            }
        }

        private void RotateToFirstValidRotation()
        {
            if (currentValidRotations?.Count > 0)
            {
                while (currentTile.Rotation != currentValidRotations[0])
                {
                    currentTile.Rotate();
                }
                gameUI.UpdateUI();
            }
        }

        private void RecalculateValidRotations()
        {
            if (!selectedPosition.HasValue) return;

            currentValidRotations =
                board.GetValidRotations(currentTile, selectedPosition.Value.x, selectedPosition.Value.y);
            if (currentValidRotations.Count == 0)
            {
                SetHighlightColor(invalidHighlightColor);
                selectedPosition = null;
                gameUI.SetEndTurnButtonActive(false);
                gameUI.SetRotateButtonActive(false);
                return;
            }

            RotateToFirstValidRotation();
            gameUI.SetRotateButtonActive(currentValidRotations.Count > 1);
        }

        public void RotateCurrentTile()
        {
            if (isJokerMode)
            {
                if (!selectedPosition.HasValue) return;
                
                try
                {
                    
                    TileJokerAnimator.JokerConfChanging(selectedPosition.Value.x, selectedPosition.Value.y);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error in RotateCurrentTile for Joker: {e}");
                }
            }
            else
            {
                if (currentValidRotations?.Count <= 1) return;

                int currentIndex = currentValidRotations.IndexOf(currentTile.Rotation);

                if (currentIndex == -1)
                {
                    RecalculateValidRotations();
                    return;
                }

                int nextIndex = (currentIndex + 1) % currentValidRotations.Count;
                while (currentTile.Rotation != currentValidRotations[nextIndex])
                {
                    currentTile.Rotate();
                }

                if (!board.CanPlaceTile(currentTile, selectedPosition.Value.x, selectedPosition.Value.y))
                {
                    RecalculateValidRotations();
                    return;
                }

                gameUI.UpdateUI();
            }
            
        }

        public void PlaceCurrentTile()
        {
            if (!selectedPosition.HasValue) return;

            OnTurnEnding.Invoke();

            gameUI.SetRotateButtonActive(false);
            
            DojoGameManager.Instance.DojoSessionManager.MakeMove(currentTile, selectedPosition.Value.x, selectedPosition.Value.y, isJokerMode);
            tilePreview.PlaceTile(currentTile, CompleteTilePlacement);
            ClearHighlights();
        }

        public void CompleteTilePlacement()
        {            
            if (!selectedPosition.HasValue) return;
            if( currentTile.Position != selectedPosition.Value)
            {
                
                currentTile.UpdateData(currentTile.RotatedConfig, selectedPosition.Value, currentTile.PlayerSide);
            }
            if (board.PlaceTile(currentTile))//, selectedPosition.Value.x, selectedPosition.Value.y,SessionManagerOld.Instance.CurrentTurnPlayer.PlayerSide))
            {
                LastMove = (currentTile, selectedPosition.Value);
                isPlacingTile = false;
                if(isJokerMode) 
                {
                    SessionManager.Instance.ManagerContext.JokerManager.CompleteJokerPlacement();
                }
                isJokerMode = false;
                jokerPosition = null;
                ClearHighlights();
                EventBus.Publish(new TilePlaced());
                gameUI.SetEndTurnButtonActive(false);
                gameUI.SetRotateButtonActive(false);
                gameUI.SetSkipTurnButtonActive(true);
                selectedPosition = null;
            }
            
        }

        public void ClearHighlights()
        {
            if (highlightedTiles == null) return;

            foreach (Transform child in highlightedTiles.transform)
            {
                Destroy(child.gameObject);
            }
        }

        public void EndTilePlacement()
        {
            isPlacingTile = false;
            _currentValidPlacements = new List<ValidPlacement>();
            isJokerMode = false;
            selectedPosition = null;
            jokerPosition = null;
            ClearHighlights();
        }

        private void SetHighlightColor(Color color)
        {
            if (highlightMaterial != null)
            {
                highlightMaterial.color = color;
            }
        }

        public void StartJokerPlacement()
        {
            GameUI.Instance.SetEndTurnButtonActive(false);
            tilePreview.ResetPosition();
            
            isJokerMode = true;
            jokerPosition = null;
            isPlacingTile = true;
            selectedPosition = null;

            
            ShowJokerPlacements();
            
            gameUI.SetEndTurnButtonActive(false);
            gameUI.SetRotateButtonActive(false);
            gameUI.UpdateUI();
        }

        private void ShowJokerPlacements()
        {
            ClearHighlights();
            
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    if (IsValidJokerPosition(x, y))
                    {
                        CreateHighlight(x, y);
                    }
                }
            }
           
            SetHighlightColor(normalHighlightColor);
            
            
            if (selectedPosition.HasValue)
            {
                UpdateHighlights(selectedPosition.Value.x, selectedPosition.Value.y);
            }
        }

        private bool IsValidJokerPosition(int x, int y)
        {
            //CustomLogger.LogObject(board.GetTileData(x, y), "TileSelector: IsValidJokerPosition - Tile Data");
            if (board.GetTileData(x, y) != null) return false;

            bool hasValidNeighbor = false;
            foreach (Side side in System.Enum.GetValues(typeof(Side)))
            {
                if(side == Side.None) 
                    continue;
                int newX = x + board.GetXOffset(side);
                int newY = y + board.GetYOffset(side);

                if (board.IsValidPosition(newX, newY) && board.GetTileData(newX, newY) != null)
                {
                    if (!board.IsBorderTile(newX, newY))
                    {
                        
                        hasValidNeighbor = true;
                        break;
                    }
                    else
                    {
                        
                        TileData neighborTile = board.GetTileData(newX, newY);
                        Side oppositeSide = board.GetOppositeSide(side);
                        LandscapeType borderSide = neighborTile.GetSide(oppositeSide);
                        
                        if (borderSide != LandscapeType.Field)
                        {
                            
                            hasValidNeighbor = true;
                            break;
                        }
                    }
                }
            }

            return hasValidNeighbor;
        }

        public void StartJokerTilePlacement(TileData tile, int x, int y)
        {
            try
            {
                currentTile.UpdateData(tile.RotatedConfig, tile.Position, tile.PlayerSide);
                selectedPosition = new Vector2Int(x, y);
                isPlacingTile = true;

                gameUI.UpdateUI();
                gameUI.SetEndTurnButtonActive(true);
                
               
                UpdateHighlights(x, y);
                
                tilePreview.SetPosition(new Vector2Int(x, y));
            }
            catch (System.Exception e)
            {
                Debug.LogError(currentTile);
                Debug.LogError($"Error in StartJokerTilePlacement: {e}");
            }
        }

        public void CancelJokerMode()
        {
            if (!isJokerMode) return;
            
            isJokerMode = false;
            selectedPosition = null;
            jokerPosition = null;
            tilePreview.ResetPosition();
            ClearHighlights();

            var currentTile = SessionManager.Instance.SessionContext.Board.TopTile;
            TileData topTile = new TileData(currentTile, -Vector2Int.one,
                SessionManager.Instance.SessionContext.CurrentTurnPlayer.PlayerSide);
            if (currentTile != null)
            {
                StartTilePlacement(topTile);
            }
        }

        public void OnDestroy()
        {
            EventBus.Unsubscribe<ClientInput>(OnClientInput);
            ClearHighlights();
            Destroy(highlightedTiles);
            highlightedTiles = null;
            tilePreview.ResetPosition();
            isPlacingTile = false;
            isJokerMode = false;
            selectedPosition = null;
            jokerPosition = null;
        }
    }
}
