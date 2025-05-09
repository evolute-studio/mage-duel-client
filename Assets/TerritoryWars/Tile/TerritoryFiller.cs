using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerritoryWars.Tile
{
    [RequireComponent(typeof(LineRenderer))]
    public class TerritoryFiller : MonoBehaviour
    {
        [Tooltip("Previously, this script was used for sleeping fence and filling the formed territory by texture, " +
                 "now he performs only the second part")]
        
        [Header("Fence Sprites")]
        [SerializeField] private Sprite fenceTop;        // ─
        [SerializeField] private Sprite fenceTopRight;   // ╲
        [SerializeField] private Sprite fenceRight;      // │
        [SerializeField] private Sprite fenceBottomRight;// ╱
        [SerializeField] private Sprite fenceBottom;     // ─
        [SerializeField] private Sprite fenceBottomLeft; // ╲
        [SerializeField] private Sprite fenceLeft;       // │
        [SerializeField] private Sprite fenceTopLeft;    // ╱
        [SerializeField] private Sprite arcSprite;       

        [Header("Settings")]
        [SerializeField] private GameObject fencePrefab;
        [SerializeField] private float spacing = 0.5f;
        [SerializeField] private float fenceWidth = 1f; 
        [SerializeField] private int arcIndex = 2;       
        [SerializeField] private List<int> skipVertices; 

        [Header("Animation")]
        [SerializeField] private float spawnDelay = 0.5f; 
        [SerializeField] private bool showDebugInfo = false; 

        [Header("Territory")]
        [SerializeField] private GameObject territoryPrefab;
        public Territory currentTerritory;
        
        public SpriteRenderer TerritorySpriteRenderer;
        
        public LineRenderer lineRenderer;
        private bool isArcSpawned = false;

        private int _currentMask = 0;

        [ContextMenu("Place Fence")]
        public void PlaceTerritory(bool isContested)
        {
            if (fencePrefab == null || lineRenderer == null || lineRenderer.positionCount < 2)
            {
                return;
            }

            if (currentTerritory != null)
            {
                _currentMask = currentTerritory.gameObject.layer;
            }

            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }

            StartCoroutine(PlaceTerritoryRoutine(isContested));
        }

        private IEnumerator PlaceTerritoryRoutine(bool isContested)
        {
            Vector3[] points = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(points);

            
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = transform.InverseTransformPoint(points[i]);
            }

            
            if (currentTerritory != null)
            {
                DestroyImmediate(currentTerritory.gameObject);
            }

            if (territoryPrefab == null)
            {
                Debug.LogError("Territory prefab is not assigned!");
                yield break;
            }
            
            currentTerritory = Instantiate(territoryPrefab, transform).GetComponent<Territory>();
            currentTerritory.gameObject.layer = _currentMask;
            TerritorySpriteRenderer = currentTerritory.GetComponent<SpriteRenderer>();
            currentTerritory.SetLineRenderer(lineRenderer);
            currentTerritory.GenerateMask();
            currentTerritory.SetTexture(isContested);
            
            if (arcIndex >= 0 && arcIndex < points.Length - 1 && !isArcSpawned)
            {
                Vector3 arcPosition = Vector3.Lerp(points[arcIndex], points[arcIndex + 1], 0.5f);
                GameObject arc = Instantiate(fencePrefab, transform.parent);
                arc.name = "Arc";
                arc.transform.localPosition = arcPosition;
                arc.transform.position += transform.position;

                var spriteRenderer = arc.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = arcSprite;
                }
                isArcSpawned = true;
                //transform.parent.GetComponentInParent<TileRotator>().MirrorRotationObjects.Add(arc.transform);
            }
            
            // Додаємо PolygonCollider2D з використанням світових координат
            var polygonCollider = currentTerritory.gameObject.AddComponent<PolygonCollider2D>();
            Vector2[] colliderPoints = new Vector2[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                // Конвертуємо локальні координати в світові
                Vector3 scale = transform.localScale;
                Vector3 point = new Vector3(points[i].x / scale.x, points[i].y / scale.y, 0);
                Vector3 worldPoint = transform.TransformPoint(point);
                colliderPoints[i] = new Vector2(worldPoint.x, worldPoint.y);
            }
            polygonCollider.points = colliderPoints;
            polygonCollider.isTrigger = true;
            polygonCollider.offset = new Vector2( -currentTerritory.transform.localPosition.x, -currentTerritory.transform.localPosition.y);
            polygonCollider.gameObject.tag = "City";

           
        }

        private int GetIsometricDirectionIndex(Vector2 direction)
        {
            


            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360;

            
            int index = Mathf.RoundToInt(angle / 45f) % 8;

            return index;
        }

        private Sprite GetFenceSprite(int directionIndex)
        {
            return directionIndex switch
            {
                0 => fenceTop,
                1 => fenceTopRight,
                2 => fenceRight,
                3 => fenceBottomRight,
                4 => fenceBottom,
                5 => fenceBottomLeft,
                6 => fenceLeft,
                7 => fenceTopLeft,
                _ => fenceTop 
            };
        }
        
        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (lineRenderer == null || lineRenderer.positionCount < 2) return;

            Vector3[] points = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(points);

            for (int i = 0; i < points.Length; i++)
            {
                points[i] += transform.position;
            }

            Gizmos.color = Color.yellow;
            for (int i = 0; i < points.Length - 1; i++)
            {
                Vector3 worldStart = points[i];
                Vector3 worldEnd = points[i + 1];

               
                Gizmos.DrawLine(worldStart, worldEnd);

                Vector3 localStart = transform.InverseTransformPoint(worldStart);
                Vector3 localEnd = transform.InverseTransformPoint(worldEnd);
                Vector2 direction = (localEnd - localStart).normalized;

                if (!float.IsNaN(direction.x) && !float.IsNaN(direction.y))
                {
                    int spriteIndex = GetIsometricDirectionIndex(direction);
                    float distance = Vector2.Distance(localStart, localEnd);
                    int fenceCount = Mathf.Max(1, Mathf.FloorToInt(distance / fenceWidth));

                    Gizmos.color = Color.green;
                    for (int j = 0; j < fenceCount; j++)
                    {
                        float t = (j + 0.5f) / fenceCount;
                        Vector3 spawnPoint = Vector3.Lerp(worldStart, worldEnd, t);
                        Gizmos.DrawWireSphere(spawnPoint, 0.01f);

                        if (showDebugInfo) 
                        {
                            
                            string info = $"Spawn Point {j}:\n" +
                                        $"Start: P{i}({worldStart.x:F2}, {worldStart.y:F2})\n" +
                                        $"End: P{i + 1}({worldEnd.x:F2}, {worldEnd.y:F2})\n" +
                                        $"Angle: {GetAngleFromDirection(direction):F1}°";

                           
                            Vector3 labelPos = spawnPoint + new Vector3(0.05f, 0.05f, 0);
                            UnityEditor.Handles.Label(labelPos, info);
                        }
                    }
                    Gizmos.color = Color.yellow;
                }
            }
        }
        #endif

        private float GetAngleFromDirection(Vector2 direction)
        {
            Vector2 isoDirection = new Vector2(
                direction.x - direction.y,
                (direction.x + direction.y)
            ).normalized;

            float angle = Mathf.Atan2(isoDirection.y, isoDirection.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360;
            return angle;
        }

        private void OnValidate()
        {
            
            if (skipVertices != null)
            {
                for (int i = 0; i < skipVertices.Count; i++)
                {
                    if (skipVertices[i] < 0)
                    {
                        Debug.LogWarning($"Skip vertex index {skipVertices[i]} is negative, setting to 0");
                        skipVertices[i] = 0;
                    }
                }
            }
        }
    }
}