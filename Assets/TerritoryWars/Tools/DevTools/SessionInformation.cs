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
                GUILayout.Label($"{SessionManager.Instance.SessionContext.PlayersData[0].Username}");
                GUILayout.Label($"{SessionManager.Instance.SessionContext.PlayersData[0].PlayerId}");
                GUILayout.Label($"{SessionManager.Instance.SessionContext.PlayersData[0].Score.TotalScore}");
                
                GUILayout.Label("Player 2");
                GUILayout.Label($"{SessionManager.Instance.SessionContext.PlayersData[1].Username}");
                GUILayout.Label($"{SessionManager.Instance.SessionContext.PlayersData[1].PlayerId}");
                GUILayout.Label($"{SessionManager.Instance.SessionContext.PlayersData[1].Score.TotalScore}");
                
                GUILayout.EndVertical();
            }
        }
    }
}