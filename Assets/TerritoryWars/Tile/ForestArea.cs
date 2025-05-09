using TerritoryWars.General;
using UnityEngine;

namespace TerritoryWars.Tile
{
    public class ForestArea : Area
    {
        public GameObject prefab;
        public float treeDensity = 0.5f;
        public Vector2 boundsY = new Vector2(0, 9);

        private void Start()
        {
            GenerateForest();
        }

        private bool ShouldSpawnNorthernTree(float y)
        {
            float normalizedY = Mathf.InverseLerp(boundsY.x, boundsY.y, y);
            
            float northernProbability = normalizedY;

            float random = Random.value;
            
            return random < northernProbability;
        }

        private bool ShouldSpawnSouthernTree(float y)
        {
            float normalizedY = Mathf.InverseLerp(boundsY.x, boundsY.y, y);
            
            float southernProbability = 1 - normalizedY;
            
            float random = Random.value;
            
            return random < southernProbability;
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
            return area;
        }

        private void GenerateForest()
        {
            if (lineRenderer == null)
            {
                return;
            }
            if (prefab == null)
            {
                return;
            }

            Vector3[] positions = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(positions);

            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = transform.TransformPoint(positions[i]);
            }

            float area = CalculatePolygonArea(positions);
            int targetTreeCount = Mathf.Max(1, Mathf.RoundToInt(area * treeDensity));

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
                    SpriteRenderer treeRenderer = tree.GetComponent<SpriteRenderer>();
                    bool isNorthern = ShouldSpawnNorthernTree(randomY);
                    bool isSouthern = ShouldSpawnSouthernTree(randomY);
                    treeRenderer.sprite = Random.Range(0, 10) < 3
                        ? PrefabsManager.Instance.TileAssetsObject.GetRandomBush()
                        : PrefabsManager.Instance.TileAssetsObject.GetTree(isNorthern, isSouthern);
                    tree.transform.parent = transform;
                    treesPlaced++;
                }
            }
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
            return inside;
        }
    }
}