using TerritoryWars.General;
using UnityEngine;

namespace TerritoryWars.Tile
{
    public class ForestArea : Area
    {
        public GameObject prefab;
        public float treeDensity = 0.5f;
        public Vector2 boundsY = new Vector2(-5, 5);

        private void Start()
        {
            Debug.Log("ForestArea: Start called");
            GenerateForest();
        }

        private bool ShouldSpawnNorthernTree(float y)
        {
            // Нормалізуємо позицію Y відносно меж (0 = нижня межа, 1 = верхня межа)
            float normalizedY = Mathf.InverseLerp(boundsY.x, boundsY.y, y);
            
            // Використовуємо нормалізовану позицію як базову ймовірність
            float northernProbability = normalizedY;
            
            // Генеруємо випадкове число від 0 до 1
            float random = Random.value;
            
            Debug.Log($"ForestArea: Tree at Y={y} (normalized={normalizedY:F2}), probability={northernProbability:F2}, random={random:F2}");
            
            return random < northernProbability;
        }

        private float CalculatePolygonArea(Vector3[] points)
        {
            float area = 0;
            int j = points.Length - 1;

            for (int i = 0; i < points.Length; i++)
            {
                area += (points[j].x + points[i].x) * (points[j].y - points[i].y);
                j = i;
            }

            area = Mathf.Abs(area / 2);
            Debug.Log($"ForestArea: Calculated area: {area} square units");
            return area;
        }

        private void GenerateForest()
        {
            if (lineRenderer == null)
            {
                Debug.LogError("ForestArea: LineRenderer is null!");
                return;
            }
            if (prefab == null)
            {
                Debug.LogError("ForestArea: Tree prefab is not assigned!");
                return;
            }

            Debug.Log($"ForestArea: LineRenderer position count: {lineRenderer.positionCount}");

            Vector3[] positions = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(positions);

            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = transform.TransformPoint(positions[i]);
                Debug.Log($"ForestArea: Point {i} - Local: {lineRenderer.GetPosition(i)}, Global: {positions[i]}");
            }

            float area = CalculatePolygonArea(positions);
            int targetTreeCount = Mathf.Max(1, Mathf.RoundToInt(area * treeDensity));
            
            Debug.Log($"ForestArea: Target tree count based on density: {targetTreeCount} (density: {treeDensity}, area: {area})");

            float minX = positions[0].x;
            float maxX = positions[0].x;
            float minY = positions[0].y;
            float maxY = positions[0].y;

            for (int i = 1; i < positions.Length; i++)
            {
                minX = Mathf.Min(minX, positions[i].x);
                maxX = Mathf.Max(maxX, positions[i].x);
                minY = Mathf.Min(minY, positions[i].y);
                maxY = Mathf.Max(maxY, positions[i].y);
            }

            float padding = 0.1f;
            minX -= padding;
            maxX += padding;
            minY -= padding;
            maxY += padding;

            Debug.Log($"ForestArea: Bounds - X: {minX} to {maxX}, Y: {minY} to {maxY}");

            int treesPlaced = 0;
            int maxAttempts = targetTreeCount * 3;
            int attempts = 0;

            while (treesPlaced < targetTreeCount && attempts < maxAttempts)
            {
                attempts++;
                float randomX = Random.Range(minX, maxX);
                float randomY = Random.Range(minY, maxY);
                Vector3 randomPos = new Vector3(randomX, randomY, 0);

                if (IsPointInPolygon(randomPos, positions))
                {
                    GameObject tree = Instantiate(prefab, randomPos, Quaternion.identity);
                    bool isNorthern = ShouldSpawnNorthernTree(randomY);
                    tree.GetComponent<SpriteRenderer>().sprite =
                        PrefabsManager.Instance.TileAssetsObject.GetTree(isNorthern);
                    tree.transform.parent = transform;
                    treesPlaced++;
                    Debug.Log($"ForestArea: Successfully placed {(isNorthern ? "northern" : "southern")} tree at {randomPos}");
                }
            }

            Debug.Log($"ForestArea: Generation complete. Placed {treesPlaced} trees out of {attempts} attempts");
        }

        private bool IsPointInPolygon(Vector3 point, Vector3[] polygon)
        {
            bool inside = false;
            int j = polygon.Length - 1;

            for (int i = 0; i < polygon.Length; i++)
            {
                if (((polygon[i].y <= point.y && point.y < polygon[j].y) || 
                     (polygon[j].y <= point.y && point.y < polygon[i].y)) &&
                    (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / 
                     (polygon[j].y - polygon[i].y) + polygon[i].x))
                {
                    inside = !inside;
                }
                j = i;
            }

            Debug.Log($"ForestArea: Point {point} is {(inside ? "inside" : "outside")} polygon");
            return inside;
        }
    }
}