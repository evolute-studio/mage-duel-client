using TerritoryWars.Managers.SessionComponents;
using UnityEngine;

namespace TerritoryWars.Tools.DevTools
{
    public class SessionInformation : MonoBehaviour, IDevTool
    {
        public string ToolName { get; } = "Session Information";
        public void DrawUI()
        {
            if (SessionManager.Instance != null && SessionManager.Instance.IsInitialized)
            {
                GUILayout.BeginVertical();
                GUILayout.Label($"Player 1");
                GUILayout.Label($"{SessionManager.Instance.SessionContext.Board.Player1.Username}");
                GUILayout.Label($"{SessionManager.Instance.SessionContext.Board.Player1.PlayerId}");
                GUILayout.Label($"{SessionManager.Instance.SessionContext.Board.Player1.Score.TotalScore}");
                
                GUILayout.Label("Player 2");
                GUILayout.Label($"{SessionManager.Instance.SessionContext.Board.Player2.Username}");
                GUILayout.Label($"{SessionManager.Instance.SessionContext.Board.Player2.PlayerId}");
                GUILayout.Label($"{SessionManager.Instance.SessionContext.Board.Player2.Score.TotalScore}");
                
                GUILayout.EndVertical();
            }
        }
    }
}