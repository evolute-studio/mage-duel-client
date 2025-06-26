using System.Collections.Generic;
using TerritoryWars.ConnectorLayers.Dojo;
using TerritoryWars.DataModels;
using TerritoryWars.Dojo;
using TerritoryWars.General;
using UnityEngine;

namespace TerritoryWars.Tools.DevTools
{
    public class GamesInProgress: MonoBehaviour, IDevTool
    {
        public string ToolName { get; } = "Games In Progress";
        private List<string> boardIds = new List<string>();

        public void DrawUI()
        {
            if (GUILayout.Button("Refresh Games", GUILayout.Width(200)))
            {
                RefreshGames();
            }

            if (GUILayout.Button("Clear", GUILayout.Width(200)))
            {
                ClearBoards();
            }
            
            GUILayout.BeginVertical("box");
            foreach (var boardId in boardIds)
            {
                if (GUILayout.Button(boardId))
                {
                    LoadBoard(boardId);
                }
            }
            
            GUILayout.EndVertical();
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

        public async void RefreshGames()
        {
            boardIds.Clear();
            
            await DojoGameManager.Instance.SyncInProgressGames();
            GameModel[] gameModels = await DojoModels.GetAllGameInProgress();

            foreach (var game in gameModels)
            {
                string status = game.Status switch
                {
                    TerritoryWars.DataModels.GameStatus.Created => "Created",
                    TerritoryWars.DataModels.GameStatus.InProgress => "In Progress",
                    TerritoryWars.DataModels.GameStatus.Finished => "Finished",
                    TerritoryWars.DataModels.GameStatus.Canceled => "Canceled",
                    _ => "Unknown"
                };

                switch (status)
                {
                    case "In Progress":
                        if(!boardIds.Contains(game.BoardId))
                            boardIds.Add(game.BoardId);
                        break;
                }
                
            }
            
        }

        public void ClearBoards()
        {
            boardIds.Clear();
        }
    }
}