using System;
using UnityEngine;

namespace TerritoryWars
{
    public class GridDrawer : MonoBehaviour
    {
        public float cellSize = 1f;
        public int rows = 3;
        public int cols = 3;
        public Vector2 offset = Vector2.zero;
        public Color gridColor = Color.white;
        public float lineThickness = 1f;
        
        public void OnDrawGizmos()
        {
            IsometricGrid();
        }
        
        private void IsometricGrid()
        {
            Gizmos.color = gridColor;
            
            // Конвертуємо ізометричні координати в декартові
            for (int x = -cols; x <= cols; x++)
            {
                for (int y = -rows; y <= rows; y++)
                {
                    // Конвертуємо ізометричні координати в декартові
                    float isoX = (x - y) * cellSize;
                    float isoY = (x + y) * cellSize * 0.5f;
                    
                    // Додаємо позицію transform та offset
                    Vector3 basePosition = transform.position;
                    isoX += basePosition.x + offset.x;
                    isoY += basePosition.y + offset.y;
                    
                    // Малюємо горизонтальні лінії
                    if (y < rows)
                    {
                        Vector3 start = new Vector3(isoX, isoY, 0);
                        Vector3 end = new Vector3(isoX + cellSize, isoY + cellSize * 0.5f, 0);
                        DrawThickLine(start, end);
                    }
                    
                    // Малюємо вертикальні лінії
                    if (x < cols)
                    {
                        Vector3 start = new Vector3(isoX, isoY, 0);
                        Vector3 end = new Vector3(isoX - cellSize, isoY + cellSize * 0.5f, 0);
                        DrawThickLine(start, end);
                    }
                }
            }
        }

        private void DrawThickLine(Vector3 start, Vector3 end)
        {
            if (lineThickness <= 1f)
            {
                Gizmos.DrawLine(start, end);
                return;
            }

            Vector3 direction = (end - start).normalized;
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.forward).normalized;
            float halfThickness = lineThickness * 0.5f;

            Vector3 p1 = start + perpendicular * halfThickness;
            Vector3 p2 = start - perpendicular * halfThickness;
            Vector3 p3 = end + perpendicular * halfThickness;
            Vector3 p4 = end - perpendicular * halfThickness;

            Gizmos.DrawLine(p1, p3);
            Gizmos.DrawLine(p2, p4);
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p3, p4);
        }
    }
}