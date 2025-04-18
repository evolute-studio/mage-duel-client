using System.Collections.Generic;
using System.Numerics;
using TerritoryWars.Dojo;
using TerritoryWars.Models;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.Tile;
using TerritoryWars.Tools;
using TerritoryWars.UI;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace TerritoryWars.General
{
    public class StructureHoverManager : MonoBehaviour
    {
        public bool IsGameFinished = false;
        public Transform Canvas;
        public RawImage FillImage;
        public RawImage OutlineImage;
        private INode _structureRoot;
        private HashSet<KeyValuePair<Vector2Int, Side>> _hoveredTiles = new HashSet<KeyValuePair<Vector2Int, Side>>();
        private HashSet<GameObject> _hoveredObjects = new HashSet<GameObject>();
        private List<TileParts> _hoveredTilesParts = new List<TileParts>();
        private bool isCity = false;
        private bool isHovered = false;
        
        private StructureHoverPanel structureHoverPanel;
        [SerializeField] private float _offsetY = 0;
        private float _defaultOrthoSize = 5f;
        private float _initialPanelScale = 1.2f;
        
        public void Start()
        {
            structureHoverPanel = Instantiate(PrefabsManager.Instance.StructureHoverPrefab, Canvas).GetComponent<StructureHoverPanel>();
            _initialPanelScale = structureHoverPanel.transform.localScale.x;
            structureHoverPanel.gameObject.SetActive(false);
        }
        
        public void Update()
        {
            if (!IsGameFinished)
            {
                HandleTileHover();
                UpdateInfoPanel();
            }
        }
        
        private void HandleTileHover()
        {
            if (CursorManager.Instance != null)
            {
               
                var hit = Physics2D.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition).origin,
                    Vector2.zero, Mathf.Infinity);
                
                if (hit.Length > 0)
                {
                    HoverEnter(hit);
                }
                else if (isHovered)
                {
                    HoverExit();
                }
            }
        }

        private void UpdateInfoPanel()
        {
            if (!isHovered)
            {
                structureHoverPanel.gameObject.SetActive(false);
                return;
            }
            structureHoverPanel.gameObject.SetActive(true);
            structureHoverPanel.SetScores(_structureRoot.GetBluePoints(), _structureRoot.GetRedPoints());
            
            Vector3 mousePosition = Input.mousePosition;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            float ratio = Camera.main.orthographicSize / _defaultOrthoSize * 1.2f;
            float y = worldPosition.y + _offsetY * ratio;
            Vector3 panelPosition = new Vector3(worldPosition.x, y, 0);
            structureHoverPanel.transform.position = panelPosition;
            Vector3 localScale = Vector3.one  / (_initialPanelScale * ratio);
            structureHoverPanel.transform.localScale = localScale.x < _initialPanelScale ? Vector3.one * _initialPanelScale : localScale;
        }

        private bool IsNewStructure(RaycastHit2D[] hits)
        {
            foreach (var hit in hits)
            {
                if(!hit.transform.gameObject.CompareTag("City") ||
                   !hit.transform.gameObject.CompareTag("Road")) continue;
                Transform cityParent = hit.transform.parent.parent.parent;
                Transform roadParent = hit.transform.parent.parent;
                if (_hoveredObjects.Contains(cityParent.gameObject) || _hoveredObjects.Contains(roadParent.gameObject))
                {
                    return false;
                }
            }
            return true;
        }

        private void SetStructureData(RaycastHit2D[] hits)
        {
            foreach (var hit in hits)
            {
                if (hit.transform.gameObject.CompareTag("City"))
                {
                    CityHover(hit.transform);
                    return;
                }
                else if (hit.transform.gameObject.CompareTag("Road"))
                {
                    RoadHover(hit.transform);
                    return;
                }
            }
        }

        private void CityHover(Transform objTransform)
        {
            Board board = SessionManager.Instance.Board;
            
            Transform parent = objTransform.transform.parent.parent.parent;
            _hoveredObjects.Add(parent.gameObject);
            tilePosition = board.GetPositionByObject(parent.gameObject);
            if(board.IsEdgeTile(tilePosition.x, tilePosition.y))
            {
                (tilePosition, _) = board.GetNeighborPositionAndSideToEdgeTile(tilePosition.x, tilePosition.y);
            }
            if(tilePosition == new Vector2Int(-1, -1)) return;
            var cityDict = DojoGameManager.Instance.DojoSessionManager.GetCityByPosition(tilePosition);
            if (cityDict.Key == null) return;
            _structureRoot = cityDict.Key;
            foreach (var city in cityDict.Value)
            {
                (Vector2Int structurePosition, Side side) = OnChainBoardDataConverter.GetPositionAndSide(city.position);
                KeyValuePair<Vector2Int, Side> keyValuePair = new KeyValuePair<Vector2Int, Side>(structurePosition, side);
                if (_hoveredTiles.Contains(keyValuePair)) continue;
                
                _hoveredTiles.Add(keyValuePair);
                Vector2Int edgeTile = board.GetEdgeNeighbors(structurePosition.x, structurePosition.y, side);
                if(edgeTile == new Vector2Int(-1, -1)) continue;
                KeyValuePair<Vector2Int, Side> edgeKeyValuePair = new KeyValuePair<Vector2Int, Side>(edgeTile, side);
                if (_hoveredTiles.Contains(edgeKeyValuePair)) continue;
                _hoveredTiles.Add(edgeKeyValuePair);
            }
            if (cityDict.Value.Count > 0)
            {
                isCity = true;
            }
        }
        
        private void RoadHover(Transform objTransform)
        {
            Board board = SessionManager.Instance.Board;
            Transform parent = objTransform.transform.parent.parent;
            _hoveredObjects.Add(parent.gameObject);
            TileParts tileParts = objTransform.transform.parent.GetComponent<TileParts>();
            Side hoveredSide = tileParts.GetRoadSideByObject(objTransform.gameObject);
            tilePosition = board.GetPositionByObject(parent.gameObject);
            if (board.IsEdgeTile(tilePosition.x, tilePosition.y))
            {
                (tilePosition, hoveredSide) = board.GetNeighborPositionAndSideToEdgeTile(tilePosition.x, tilePosition.y);
            }
            if(tilePosition == new Vector2Int(-1, -1)) return;
            
            byte rootPosition = OnChainBoardDataConverter.GetRootByPositionAndSide(tilePosition, hoveredSide);
            
            var roadDict = DojoGameManager.Instance.DojoSessionManager.GetRoadByPosition(rootPosition);
            if (roadDict.Key == null) return;
            _structureRoot = roadDict.Key;
            foreach (var road in roadDict.Value)
            {
                (Vector2Int structurePosition, Side side) = OnChainBoardDataConverter.GetPositionAndSide(road.position);
                KeyValuePair<Vector2Int, Side> keyValuePair = new KeyValuePair<Vector2Int, Side>(structurePosition, side);
                if (_hoveredTiles.Contains(keyValuePair)) continue;
                
                _hoveredTiles.Add(keyValuePair);
                Vector2Int edgeTile = board.GetEdgeNeighbors(structurePosition.x, structurePosition.y, side);
                if(edgeTile == new Vector2Int(-1, -1)) continue;
                KeyValuePair<Vector2Int, Side> edgeKeyValuePair = new KeyValuePair<Vector2Int, Side>(edgeTile, board.GetOppositeSide(side));
                if (_hoveredTiles.Contains(edgeKeyValuePair)) continue;
                _hoveredTiles.Add(edgeKeyValuePair);
            }
            if (roadDict.Value.Count > 0)
            {
                isCity = false;
            }
        }

        private void HoverEnter(RaycastHit2D[] hits)
        {
            bool isNew = IsNewStructure(hits);
            if (isNew)
            {
                HoverExit();
                SetStructureData(hits);
            }
            if (_hoveredTiles.Count == 0) return;
            
            if (isHovered) return;
            Board board = SessionManager.Instance.Board;
            foreach (var position in _hoveredTiles)
            {
                GameObject tile = board.GetTileObject(position.Key.x, position.Key.y);
                if (tile != null)
                {
                    TileParts tileParts = tile.GetComponentInChildren<TileParts>();
                    if (isCity)
                    {
                        tileParts.CityOutline(true);
                    }
                    else
                    {
                        tileParts.RoadOutline(true, position.Value);
                    }
                    _hoveredTilesParts.Add(tileParts);
                }
            }
            isHovered = true;
            CursorManager.Instance.SetCursor("pointer");
        }

        public HashSet<KeyValuePair<TileParts, Side>> GetRoadTilePartsForHighlight(Transform tileParent, TileParts tileParts, byte rootPosition = 0)
        {
            Board board = SessionManager.Instance.Board;
            HashSet<KeyValuePair<TileParts, Side>> roadTileParts = new HashSet<KeyValuePair<TileParts, Side>>();
            HashSet<KeyValuePair<Vector2Int, Side>> hoveredTiles = new HashSet<KeyValuePair<Vector2Int, Side>>();
            
            var roadDict = DojoGameManager.Instance.DojoSessionManager.GetRoadByPosition(rootPosition);
            if (roadDict.Key == null) return new HashSet<KeyValuePair<TileParts, Side>>();
            foreach (var road in roadDict.Value)
            {
                (Vector2Int structurePosition, Side side) = OnChainBoardDataConverter.GetPositionAndSide(road.position);
                KeyValuePair<Vector2Int, Side> keyValuePair = new KeyValuePair<Vector2Int, Side>(structurePosition, side);
                if (hoveredTiles.Contains(keyValuePair)) continue;
                
                hoveredTiles.Add(keyValuePair);
                Vector2Int edgeTile = board.GetEdgeNeighbors(structurePosition.x, structurePosition.y, side);
                if(edgeTile == new Vector2Int(-1, -1)) continue;
                KeyValuePair<Vector2Int, Side> edgeKeyValuePair = new KeyValuePair<Vector2Int, Side>(edgeTile, board.GetOppositeSide(side));
                if (hoveredTiles.Contains(edgeKeyValuePair)) continue;
                hoveredTiles.Add(edgeKeyValuePair);
            }

            foreach (var hoveredTile in hoveredTiles)
            {
                GameObject tile = board.GetTileObject(hoveredTile.Key.x, hoveredTile.Key.y);
                if (tile != null)
                {
                    KeyValuePair<TileParts, Side> tilePart = new KeyValuePair<TileParts, Side>(tile.GetComponentInChildren<TileParts>(), hoveredTile.Value);
                    roadTileParts.Add(tilePart);
                }
            }
            
            return roadTileParts;
        }

        public List<TileParts> GetCityTilePartsForHighlight(Transform tileParent)
        {
            Board board = SessionManager.Instance.Board;
            List<TileParts> cityTileParts = new List<TileParts>();
            HashSet<KeyValuePair<Vector2Int, Side>> hoveredTiles = new HashSet<KeyValuePair<Vector2Int, Side>>();
            
            Vector2Int position = board.GetPositionByObject(tileParent.gameObject);
            if (board.IsEdgeTile(position.x, position.y))
            {
                (position, _) = board.GetNeighborPositionAndSideToEdgeTile(position.x, position.y);
            }
            
            if(position == new Vector2Int(-1, -1)) return new List<TileParts>();
            var cityDict = DojoGameManager.Instance.DojoSessionManager.GetCityByPosition(position);
            if (cityDict.Key == null) return new List<TileParts>();
            foreach (var city in cityDict.Value)
            {
                (Vector2Int structurePosition, Side side) = OnChainBoardDataConverter.GetPositionAndSide(city.position);
                KeyValuePair<Vector2Int, Side> keyValuePair = new KeyValuePair<Vector2Int, Side>(structurePosition, side);
                if (hoveredTiles.Contains(keyValuePair)) continue;
                
                hoveredTiles.Add(keyValuePair);
                Vector2Int edgeTile = board.GetEdgeNeighbors(structurePosition.x, structurePosition.y, side);
                if(edgeTile == new Vector2Int(-1, -1)) continue;
                KeyValuePair<Vector2Int, Side> edgeKeyValuePair = new KeyValuePair<Vector2Int, Side>(edgeTile, side);
                if (hoveredTiles.Contains(edgeKeyValuePair)) continue;
                hoveredTiles.Add(edgeKeyValuePair);
            }

            foreach (var hoveredTile in hoveredTiles)
            {
                GameObject tile = board.GetTileObject(hoveredTile.Key.x, hoveredTile.Key.y);
                if (tile != null)
                {
                    cityTileParts.Add(tile.GetComponentInChildren<TileParts>());
                }
            }
            
            return cityTileParts;
        }

        private void HoverExit()
        {
            if (!isHovered) return;
            foreach (var tilePart in _hoveredTilesParts)
            {
                if (isCity)
                {
                    tilePart.CityOutline(false);
                }
                else
                {
                    tilePart.RoadOutline(false);
                }
            }

            isHovered = false;
            _hoveredTiles.Clear();
            _hoveredTilesParts.Clear();
            CursorManager.Instance.SetCursor("default");
        }

        private Vector2Int tilePosition;

        // private void OnGUI()
        // {
        //     GUIStyle style = new GUIStyle();
        //     style.fontSize = 20;
        //     style.normal.textColor = Color.white;
        //     
        //     Vector3 mousePosition = Input.mousePosition;
        //     Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        //     Vector2Int boardPosition = tilePosition;
        //     
        //     float screenHeight = Screen.height;
        //     float startY = screenHeight / 2 - 120; // Починаємо з центру мінус половина висоти тексту
        //     
        //     GUI.Label(new Rect(10, startY, 300, 30), $"Mouse Screen: {mousePosition.x:F0}, {mousePosition.y:F0}", style);
        //     GUI.Label(new Rect(10, startY + 30, 300, 30), $"Mouse World: {worldPosition.x:F2}, {worldPosition.y:F2}", style);
        //     GUI.Label(new Rect(10, startY + 60, 300, 30), $"Board Position: {boardPosition.x}, {boardPosition.y}", style);
        //     GUI.Label(new Rect(10, startY + 90, 300, 30), $"Hovered: {isHovered}", style);
        //     GUI.Label(new Rect(10, startY + 120, 300, 30), $"Is City: {isCity}", style);
        //     GUI.Label(new Rect(10, startY + 150, 300, 30), $"Hovered Tiles Count: {_hoveredTiles.Count}", style);
        //     
        //     float yOffset = startY + 180;
        //     foreach (var tile in _hoveredTiles)
        //     {
        //         GUI.Label(new Rect(10, yOffset, 300, 30), $"Tile: {tile.Key.x}, {tile.Key.y} Side: {tile.Value}", style);
        //         yOffset += 30;
        //     }
        // }
    }
}