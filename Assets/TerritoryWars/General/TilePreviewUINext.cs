using System.Collections.Generic;
using TerritoryWars.Tile;
using UnityEngine;

namespace TerritoryWars.General
{
    public class TilePreviewUINext : MonoBehaviour
    {
        [SerializeField] private TileGenerator _tileGenerator;
        
        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }
        
        public void UpdatePreview(TileData currentTile)
        {
            _tileGenerator.gameObject.SetActive(true);
            if (currentTile != null)
            {
                gameObject.SetActive(true);
                _tileGenerator.Generate(currentTile);
                if (_tileGenerator.CurrentTileGO != null)
                {
                    TileParts tileParts = _tileGenerator.CurrentTileGO.GetComponent<TileParts>();
                    tileParts.HideForestAreas();
                }
            }
            
            if (_tileGenerator.CurrentTileGO != null)
            {
                void SetLayerRecursively(Transform root)
                {
                    root.gameObject.layer = LayerMask.NameToLayer("TilePreview");
                    foreach(Transform child in root)
                    {
                        SetLayerRecursively(child);
                    }
                }
                    
                SetLayerRecursively(_tileGenerator.CurrentTileGO.transform);
                    
            }
            
        }
    }
}