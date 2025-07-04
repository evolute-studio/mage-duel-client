using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TerritoryWars.Tools.DevTools
{
    public class DevToolsManager : MonoBehaviour
    {
        private List<IDevTool> tools;
        private bool isPanelExpanded = false;
        private readonly float panelWidth = 250f;
        private readonly float collapseButtonSize = 30f;
        private GUIStyle buttonStyle;
        private GUIStyle panelStyle;
        private Vector2 scrollPosition;

        private void Start()
        {
            DontDestroyOnLoad(this);
            tools = GetComponents<IDevTool>().ToList();
        }

        private void OnGUI()
        {
            // if (!Application.isEditor) return;

            InitializeStyles();

            // Draw collapse/expand button
            DrawToggleButton();

            // Draw main panel if expanded
            if (isPanelExpanded)
            {
                DrawMainPanel();
            }
        }

        private void InitializeStyles()
        {
            if (buttonStyle == null)
            {
                buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 16,
                    fontStyle = FontStyle.Bold
                };
            }

            if (panelStyle == null)
            {
                panelStyle = new GUIStyle(GUI.skin.box)
                {
                    padding = new RectOffset(10, 10, 10, 10)
                };
            }
        }

        private void DrawToggleButton()
        {
            float buttonX = isPanelExpanded ? panelWidth - collapseButtonSize : 0;
            Rect buttonRect = new Rect(buttonX, 10, collapseButtonSize, collapseButtonSize);

            string buttonText = isPanelExpanded ? "◀" : "▶";

            if (GUI.Button(buttonRect, buttonText, buttonStyle))
            {
                isPanelExpanded = !isPanelExpanded;
            }
        }

        private void DrawMainPanel()
        {
            Rect panelRect = new Rect(0, 0, panelWidth, Screen.height);

            GUI.Box(panelRect, "", panelStyle);

            GUILayout.BeginArea(new Rect(5, 50, panelWidth - 10, Screen.height - 55));

            var boldStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 16 };
            GUILayout.Label("Dev Tools", boldStyle);
            GUILayout.Space(10);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            foreach (var tool in tools)
            {
                if (tool != null)
                {
                    GUILayout.BeginVertical(GUI.skin.box);
                    var toolStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
                    GUILayout.Label(tool.ToolName, toolStyle);
                    tool.DrawUI();
                    GUILayout.EndVertical();
                    GUILayout.Space(5);
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}