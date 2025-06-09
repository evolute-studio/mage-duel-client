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
            GameModel game = await DojoLayer.Instance.GetGameByBoardId(boardId);
            if (game.IsNull)
            {
                CustomLogger.LogError("Game is null");
                return;
            }

            //DojoGameManager.Instance.GlobalContext.GameInProgress = game;
            //DojoGameManager.Instance.LoadGame();
        }
    }
}