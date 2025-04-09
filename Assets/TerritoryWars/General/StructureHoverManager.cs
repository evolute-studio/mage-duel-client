using System.Collections.Generic;
using TerritoryWars.Dojo;
using TerritoryWars.Models;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.Tools;
using TerritoryWars.UI;
using UnityEngine;

namespace TerritoryWars.General
{
    public class StructureHoverManager : MonoBehaviour
    {
        public Transform Canvas;
        private INode _structureRoot;
        private HashSet<KeyValuePair<Vector2Int, Side>> _hoveredTiles = new HashSet<KeyValuePair<Vector2Int, Side>>();
        private List<TileParts> _hoveredTilesParts = new List<TileParts>();
        private bool isCity = false;
        private bool isHovered = false;
        
        private StructureHoverPanel structureHoverPanel;
        [SerializeField] private float _offsetY = 0;
        
        public void Start()
        {
            structureHoverPanel = Instantiate(PrefabsManager.Instance.StructureHoverPrefab, Canvas).GetComponent<StructureHoverPanel>();
            structureHoverPanel.gameObject.SetActive(false);
        }
        
        public void Update()
        {
            HandleTileHover();
            UpdateInfoPanel();
        }
        
        private void HandleTileHover()
        {
            if (CursorManager.Instance != null)
            {
               
                var hit = Physics2D.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition).origin,
                    Vector2.zero, Mathf.Infinity);
                
                if (hit.Length > 0)
                {
                    CustomLogger.LogInfo("Hit count: " + hit.Length);
                    HoverEnter(hit);
                }
                else if (isHovered)
                {
                    CustomLogger.LogInfo("Hit count: 0");
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
            Vector3 panelPosition = new Vector3(worldPosition.x, worldPosition.y + _offsetY, 0);
            structureHoverPanel.transform.position = panelPosition;
        }

        private bool IsNewStructure(RaycastHit2D[] hits)
        {
            // foreach (var hit in hits)
            // {
            //     Vector2Int tilePosition = Board.GetPositionByWorld(hit.transform.position);
            //     if (_hoveredTiles.Contains(tilePosition)) return false;
            // }
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
            Transform parent = objTransform.transform.parent.parent.parent;
            CustomLogger.LogInfo("Parent: " + parent.name);
            tilePosition = SessionManager.Instance.Board.GetPositionByObject(parent.gameObject);
            CustomLogger.LogInfo("Tile Position: " + tilePosition);
            var cityDict = DojoGameManager.Instance.SessionManager.GetCityByPosition(tilePosition);
            if (cityDict.Key == null) return;
            _structureRoot = cityDict.Key;
            foreach (var city in cityDict.Value)
            {
                (Vector2Int structurePosition, Side side) = OnChainBoardDataConverter.GetPositionAndSide(city.position);
                KeyValuePair<Vector2Int, Side> keyValuePair = new KeyValuePair<Vector2Int, Side>(structurePosition, side);
                if (_hoveredTiles.Contains(keyValuePair)) continue;
                
                _hoveredTiles.Add(keyValuePair);
                List<Vector2Int> edgeTiles = SessionManager.Instance.Board.GetEdgeNeighbors(structurePosition.x, structurePosition.y);
                foreach (var edgeTile in edgeTiles)
                {
                    KeyValuePair<Vector2Int, Side> edgeKeyValuePair = new KeyValuePair<Vector2Int, Side>(edgeTile, Side.None);
                    if (_hoveredTiles.Contains(edgeKeyValuePair)) continue;
                    _hoveredTiles.Add(edgeKeyValuePair);
                }
            }
            if (cityDict.Value.Count > 0)
            {
                isCity = true;
            }
        }
        
        private void RoadHover(Transform objTransform)
        {
            Transform parent = objTransform.transform.parent.parent;
            tilePosition = SessionManager.Instance.Board.GetPositionByObject(parent.gameObject);
            var roadDict = DojoGameManager.Instance.SessionManager.GetRoadByPosition(tilePosition);
            if (roadDict.Key == null) return;
            _structureRoot = roadDict.Key;
            foreach (var road in roadDict.Value)
            {
                Vector2Int structurePosition = OnChainBoardDataConverter.GetPositionByRoot(road.position);
                KeyValuePair<Vector2Int, Side> keyValuePair = new KeyValuePair<Vector2Int, Side>(structurePosition, Side.None);
                if (_hoveredTiles.Contains(keyValuePair)) continue;
                
                _hoveredTiles.Add(keyValuePair);
                List<Vector2Int> edgeTiles = SessionManager.Instance.Board.GetEdgeNeighbors(structurePosition.x, structurePosition.y);
                foreach (var edgeTile in edgeTiles)
                {
                    KeyValuePair<Vector2Int, Side> edgeKeyValuePair = new KeyValuePair<Vector2Int, Side>(edgeTile, Side.None);
                    if (_hoveredTiles.Contains(edgeKeyValuePair)) continue;
                    _hoveredTiles.Add(edgeKeyValuePair);
                }
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
                        tileParts.RoadOutline(true);
                    }
                    _hoveredTilesParts.Add(tileParts);
                }
            }
            isHovered = true;
            CursorManager.Instance.SetCursor("pointer");
        }

        private void HoverExit()
        {
            if (!isHovered) return;
            Board board = SessionManager.Instance.Board;
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
        //         GUI.Label(new Rect(10, yOffset, 300, 30), $"Tile: {tile.x}, {tile.y}", style);
        //         yOffset += 30;
        //     }
        // }
    }
}