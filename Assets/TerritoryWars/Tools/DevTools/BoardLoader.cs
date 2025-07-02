using TerritoryWars.ConnectorLayers.Dojo;
using TerritoryWars.DataModels;
using TerritoryWars.Dojo;
using UnityEngine;

namespace TerritoryWars.Tools.DevTools
{
    public class BoardLoader: MonoBehaviour, IDevTool
    {
        public string ToolName { get; } = "Board Loader";
        private string _boardId;
        public void DrawUI()
        {
            _boardId = GUILayout.TextField(_boardId, GUILayout.Width(200));
            if (GUILayout.Button("Load Board", GUILayout.Width(200)))
            {
                LoadBoard(_boardId);
            }
        }

        public async void LoadBoard(string boardId)
        {
            Board board = await DojoModels.GetBoard(boardId);
            if (board.IsNull)
            {
                CustomLogger.LogError("Board is null");
                return;
            }

            DojoGameManager.Instance.GlobalContext.BoardForLoad = board;
            DojoGameManager.Instance.LoadSession();
        }
    }
}