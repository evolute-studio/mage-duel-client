using UnityEngine;

namespace TerritoryWars.Tools.DevTools
{
    public class FpsCounter : MonoBehaviour, IDevTool
    {
        public bool isEnabled = false;
        private int frameCount = 0;
        private float elapsedTime = 0f;
        private float displayedFps = 0f;

        public string ToolName => "FPS Counter";

        void Update()
        {
            frameCount++;
            elapsedTime += Time.unscaledDeltaTime;
            if (elapsedTime >= 0.25f)
            {
                displayedFps = frameCount / elapsedTime;
                frameCount = 0;
                elapsedTime = 0f;
            }
        }

        public void DrawUI()
        {
            isEnabled = GUILayout.Toggle(isEnabled, "FPS Counter");
        }

        public void OnGUI()
        {
            if (isEnabled)
            {
                // Створюємо стиль для FPS по центру
                GUIStyle centerStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 20,
                    fontStyle = FontStyle.Bold
                };

                // Розраховуємо позицію по центру зверху
                string fpsText = $"FPS: {Mathf.RoundToInt(displayedFps)}";
                Vector2 textSize = centerStyle.CalcSize(new GUIContent(fpsText));
                float padding = 10f;
                float x = (Screen.width - textSize.x - padding * 2) / 2f;
                float y = 10f; // Відступ зверху

                // Створюємо фоновий прямокутник
                Rect backgroundRect = new Rect(x, y, textSize.x + padding * 2, textSize.y + padding);

                // Малюємо напівпрозорий фон
                Color originalColor = GUI.color;
                GUI.color = new Color(0f, 0f, 0f, 0.7f); // Чорний з прозорістю 70%
                GUI.DrawTexture(backgroundRect, Texture2D.whiteTexture);
                GUI.color = originalColor;

                // Малюємо текст FPS
                Rect fpsRect = new Rect(x + padding, y + padding / 2, textSize.x, textSize.y);
                centerStyle.normal.textColor = Color.white; // Білий текст для контрасту
                GUI.Label(fpsRect, fpsText, centerStyle);
            }
        }
    }
}