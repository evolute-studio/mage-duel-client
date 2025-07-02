using System.Collections.Generic;
using System.Numerics;
using TerritoryWars.DataModels;
using TerritoryWars.Dojo;
using TerritoryWars.Managers.SessionComponents;
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
        private Structure currentStructure;
        private HashSet<(Vector2Int Position, Side Side)> _hoveredTiles = new HashSet<(Vector2Int, Side)>();
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
            structureHoverPanel.SetScores(currentStructure.Points[0], currentStructure.Points[1]);
            
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
            BoardManager board = SessionManager.Instance.BoardManager;
            
            Transform parent = objTransform.transform.parent.parent.parent;
            _hoveredObjects.Add(parent.gameObject);
            tilePosition = board.GetPositionByObject(parent.gameObject);
            if(board.IsEdgeTile(tilePosition.x, tilePosition.y))
            {
                (tilePosition, _) = board.GetNeighborPositionAndSideToEdgeTile(tilePosition.x, tilePosition.y);
            }
            if(tilePosition == new Vector2Int(-1, -1)) return;
            //var cityDict = DojoGameManager.Instance.DojoSessionManager.GetCityByPosition(tilePosition);
            var structureOption =
                SessionManager.Instance.SessionContext.UnionFind.GetStructureByPosition(tilePosition,
                    StructureType.City);
            if (!structureOption.HasValue) return;
            var structure = structureOption.Value;
            currentStructure = structure;
            foreach (var city in structure.Nodes)
            {
                var item = (city.Position, city.Side);
                if (_hoveredTiles.Contains(item)) continue;
                
                _hoveredTiles.Add(item);
                Vector2Int edgeTile = board.GetEdgeNeighbors(city.Position.x, city.Position.y, city.Side);
                if(edgeTile == new Vector2Int(-1, -1)) continue;
                item = (edgeTile, city.Side);
                if (_hoveredTiles.Contains(item)) continue;
                _hoveredTiles.Add(item);
            }
            if (structure.Nodes.Count > 0)
            {
                isCity = true;
            }
        }
        
        private void RoadHover(Transform objTransform)
        {
            BoardManager board = SessionManager.Instance.BoardManager;
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
            
            var structureOption = SessionManager.Instance.SessionContext.UnionFind.GetStructureByNode(tilePosition, hoveredSide, StructureType.Road);
            if (!structureOption.HasValue) return;
            var structure = structureOption.Value;
            currentStructure = structure;
            foreach (var road in structure.Nodes)
            {
                var item = (road.Position, road.Side);
                if (_hoveredTiles.Contains(item)) continue;
                
                _hoveredTiles.Add(item);
                
                Vector2Int edgeTile = board.GetEdgeNeighbors(road.Position.x, road.Position.y, road.Side);
                if(edgeTile == new Vector2Int(-1, -1)) continue;
                item = (edgeTile, board.GetOppositeSide(road.Side));
                if (_hoveredTiles.Contains(item)) continue;
                _hoveredTiles.Add(item);
            }
            if (structure.Nodes.Count > 0)
            {
                isCity = false;
            }
        }
        
        public HashSet<(TileParts, Side)> GetRoadTilePartsForHighlight(Vector2Int position, Side side)
        {
            BoardManager board = SessionManager.Instance.BoardManager;
            HashSet<(TileParts, Side)> results = new HashSet<(TileParts, Side)>();
            
            if (board.IsEdgeTile(position.x, position.y))
            {
                (position, side) = board.GetNeighborPositionAndSideToEdgeTile(position.x, position.y);
            }
            if(position == new Vector2Int(-1, -1)) return results;
            
            var structureOption = SessionManager.Instance.SessionContext.UnionFind.GetStructureByNode(position, side, StructureType.Road);
            if (!structureOption.HasValue) return results;
            var structure = structureOption.Value;
            foreach (var road in structure.Nodes)
            {
                TileParts roadTilePart = board.GetTileObject(road.Position.x, road.Position.y).GetComponentInChildren<TileParts>();
                var item = (roadTilePart, road.Side);
                if (results.Contains(item)) continue;
                
                results.Add(item);
                
                Vector2Int edgeTile = board.GetEdgeNeighbors(road.Position.x, road.Position.y, road.Side);
                if(edgeTile == new Vector2Int(-1, -1)) continue;
                TileParts edgeTileParts = board.GetTileObject(edgeTile.x, edgeTile.y).GetComponentInChildren<TileParts>();
                item = (edgeTileParts, board.GetOppositeSide(road.Side));
                if (results.Contains(item)) continue;
                results.Add(item);
            }
            return results;
        }
        
        public List<TileParts> GetCityTilePartsForHighlight(Vector2Int position)
        {
            BoardManager board = SessionManager.Instance.BoardManager;
            List<TileParts> results = new List<TileParts>();
            
            if(board.IsEdgeTile(position.x, position.y))
            {
                (position, _) = board.GetNeighborPositionAndSideToEdgeTile(position.x, position.y);
            }
            if(position == new Vector2Int(-1, -1)) return results;
            //var cityDict = DojoGameManager.Instance.DojoSessionManager.GetCityByPosition(tilePosition);
            var structureOption =
                SessionManager.Instance.SessionContext.UnionFind.GetStructureByPosition(position,
                    StructureType.City);
            if (!structureOption.HasValue) return results;
            var structure = structureOption.Value;
            foreach (var city in structure.Nodes)
            {
                TileParts cityTileParts = board.GetTileObject(city.Position.x, city.Position.y).GetComponentInChildren<TileParts>();
                TileParts item = cityTileParts;
                if (!results.Contains(item))
                {
                    results.Add(item);
                }
                
                Vector2Int edgeTile = board.GetEdgeNeighbors(city.Position.x, city.Position.y, city.Side);
                if(edgeTile == new Vector2Int(-1, -1)) continue;
                TileParts edgeTileParts = board.GetTileObject(edgeTile.x, edgeTile.y).GetComponentInChildren<TileParts>();
                item = edgeTileParts;
                if (results.Contains(item)) continue;
                results.Add(item);
            }
            return results;
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
            BoardManager board = SessionManager.Instance.BoardManager;
            foreach (var position in _hoveredTiles)
            {
                GameObject tile = board.GetTileObject(position.Position.x, position.Position.y);
                if (tile != null)
                {
                    TileParts tileParts = tile.GetComponentInChildren<TileParts>();
                    if (isCity)
                    {
                        tileParts.CityOutline(true);
                    }
                    else
                    {
                        tileParts.RoadOutline(true, position.Side);
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

        public void SetActivePanel(bool active)
        {
            structureHoverPanel.gameObject.SetActive(active);
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