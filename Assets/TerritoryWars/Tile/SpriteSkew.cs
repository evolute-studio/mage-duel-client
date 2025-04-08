using UnityEngine;

namespace TerritoryWars.Tile
{
    public class SpriteSkew : MonoBehaviour
    {
        [SerializeField] private Vector3 leftPosition = Vector3.zero;
        [SerializeField] private Vector3 rightPosition = Vector3.zero;
        [SerializeField] private float maxAngleDegrees = 70f;
        [SerializeField] private float yOffset = 0f;
        [SerializeField] private float sidesOffsetX = 0f;
        private SpriteRenderer spriteRenderer;
        private Matrix4x4 skewMatrix = Matrix4x4.identity;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            UpdateSkew();
        }

        private void OnValidate()
        {
            UpdateSkew();
        }
        
        public void SetPositions(Vector3 first, Vector3 second, float offsetY = 0, float sidesOffset = 0)
        {
            leftPosition = first.x < second.x ? first : second;
            rightPosition = first.x < second.x ? second : first;
            yOffset = offsetY;
            sidesOffsetX = sidesOffset;
            UpdateSkew();
        }

        private void UpdateSkew()
        {
            if (spriteRenderer == null) return;
            
            // Розраховуємо кут на основі оригінальних позицій
            float heightDifference = rightPosition.y - leftPosition.y;
            float originalHorizontalDistance = Vector3.Distance(
                new Vector3(leftPosition.x, 0, leftPosition.z),
                new Vector3(rightPosition.x, 0, rightPosition.z)
            );
            
            // Розраховуємо кут перекосу за оригінальними позиціями
            float angleRadians = Mathf.Atan2(heightDifference, originalHorizontalDistance);
            float angleDegrees = angleRadians * Mathf.Rad2Deg;
            
            // Перевіряємо чи кут не перевищує максимально допустимий
            if (Mathf.Abs(angleDegrees) > maxAngleDegrees)
            {
                spriteRenderer.enabled = false;
                return;
            }
            
            // Включаємо рендерер, якщо він був вимкнений
            spriteRenderer.enabled = true;
            
            // Створюємо матрицю перекосу на основі оригінального кута
            skewMatrix = Matrix4x4.identity;
            skewMatrix.m10 = Mathf.Tan(angleRadians);
            
            // Застосовуємо матрицю до спрайту
            spriteRenderer.material.SetMatrix("_SkewMatrix", skewMatrix);
            
            // Застосовуємо зміщення до позицій для розрахунку фінальної позиції та розміру
            Vector3 adjustedLeftPos = leftPosition + new Vector3(sidesOffsetX, 0, 0);
            Vector3 adjustedRightPos = rightPosition + new Vector3(-sidesOffsetX, 0, 0);
            
            // Оновлюємо позицію об'єкта
            Vector3 midPoint = Vector3.Lerp(adjustedLeftPos, adjustedRightPos, 0.5f);
            midPoint.y += yOffset;
            transform.position = midPoint;
            
            // Встановлюємо розмір спрайту на основі зміщених позицій
            float distance = Vector3.Distance(adjustedLeftPos, adjustedRightPos);
            spriteRenderer.size = new Vector2(distance, spriteRenderer.size.y);
        }
    }
}