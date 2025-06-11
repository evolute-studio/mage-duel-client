using System;
using TerritoryWars.DataModels;

namespace TerritoryWars.SaveStorage
{
    [Serializable]
    public struct SessionSave
    {
        public bool IsNull => string.IsNullOrEmpty(BoardId) || Commitments.IsNull;
        
        public string BoardId;
        public bool GameWithBot;
        public CommitmentsData Commitments;
        public CommitmentsData BotCommitments;

        public SessionSave(string boardId, bool gameWithBot, CommitmentsData commitments, CommitmentsData botCommitments)
        {
            BoardId = boardId;
            GameWithBot = gameWithBot;
            Commitments = commitments;
            BotCommitments = botCommitments;
        }
        
        
        public void Save()
        {
            SimpleStorage.SessionSave = this;
            SimpleStorage.SaveSessionSave();
        }
        
        public void SaveBoardId(string boardId)
        {
            BoardId = boardId;
            SimpleStorage.SaveSessionSave();
        }
        
        public void SaveGameWithBot(bool gameWithBot){
            GameWithBot = gameWithBot;
            SimpleStorage.SaveSessionSave();
        }
        
        public void SaveCommitments(CommitmentsData commitments)
        {
            Commitments = commitments;
            SimpleStorage.SaveSessionSave();
        }
        
        public void SaveBotCommitments(CommitmentsData commitments)
        {
            BotCommitments = commitments;
            SimpleStorage.SaveSessionSave();
        }
        
        public void Clear()
        {
            BoardId = string.Empty;
            GameWithBot = false;
            Commitments = new CommitmentsData();
            BotCommitments = new CommitmentsData();
            SimpleStorage.SaveSessionSave();
        }
    }
}