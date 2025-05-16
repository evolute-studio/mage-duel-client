using UnityEngine;

namespace TerritoryWars.Tile
{
    public class Area : MonoBehaviour
    {
        // LineRenderer is used to draw the area
        public LineRenderer lineRenderer;

        public void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
        }
    }
}