using System.Collections.Generic;
using TerritoryWars.General;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.U2D;

namespace TerritoryWars.Tile
{
    public class Territory : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private SpriteMask spriteMask;
        [SerializeField] public SpriteRenderer _spriteRenderer;

        [Header("Settings")]
        [SerializeField] private Texture2D texture;
        [SerializeField] private int textureSize = 512;
        [SerializeField] private float lineWidth = 2f;
        [SerializeField] private Color fillColor = Color.white;
        private int _splineDetail = 8; 
        
        private const float MIN_DISTANCE_BETWEEN_POINTS = 0.5f;

        public void SetLineRenderer(LineRenderer lineRenderer)
        {
            this.lineRenderer = lineRenderer;
        }
        
       

        public void GenerateTerritory()
        {
            if (lineRenderer == null || lineRenderer.positionCount < 3)
            {
                Debug.LogError("LineRenderer is missing or has less than 3 points");
                return;
            }

            int pointCount = lineRenderer.positionCount;
            
            Vector3[] positions = new Vector3[pointCount];
            lineRenderer.GetPositions(positions);

            SpriteShapeController spriteShapeController = gameObject.AddComponent<SpriteShapeController>();
            SpriteShapeRenderer spriteShapeRenderer = gameObject.GetComponent<SpriteShapeRenderer>();
            spriteShapeRenderer.sortingOrder = 13;

            Spline spline = spriteShapeController.spline;
            spline.Clear();
            
            for (int i = 0; i < pointCount - 1; i++)
            {
                Vector3 worldPos = positions[i];
                spline.InsertPointAt(i, worldPos);
            }

            
            spriteShapeController.spriteShape = PrefabsManager.Instance.MudSpriteShape;
            spriteShapeController.splineDetail = 16;
            spriteShapeController.RefreshSpriteShape();
        }
    }
}