using Newtonsoft.Json;
using TerritoryWars.DataModels;
using UnityEngine;

<<<<<<<< HEAD:Assets/TerritoryWars/SaveStorage/SimpleStorage.cs
namespace TerritoryWars.SaveStorage
========
namespace TerritoryWars.Tools
>>>>>>>> development:Assets/TerritoryWars/Tools/SimpleStorage.cs
{
    public static class SimpleStorage
    {
        public static SessionSave SessionSave;
        
        public static void Initialize()
        {
            LoadSessionSave();
        }
        
        public static void SaveSessionSave()
        {
            string json = JsonConvert.SerializeObject(SessionSave, Formatting.None);
            PlayerPrefs.SetString("SessionSave", json);
        }
        
        public static SessionSave LoadSessionSave()
        {
            string json = PlayerPrefs.GetString("SessionSave", null);
            if (string.IsNullOrEmpty(json))
            {
                return new SessionSave();
            }
            SessionSave = JsonConvert.DeserializeObject<SessionSave>(json);
            return SessionSave;
        }
        
        public static void SaveCommitments(CommitmentsData commitments)
        {
            string json = JsonConvert.SerializeObject(commitments, Formatting.None);
            PlayerPrefs.SetString("Commitments", json);
        }
        
        public static CommitmentsData? LoadCommitments()
        {
            string json = PlayerPrefs.GetString("Commitments", null);
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }
            return JsonConvert.DeserializeObject<CommitmentsData>(json);
        }
        
        
        
        public static void SetDataVersion(int version)
        {
            PlayerPrefs.SetInt("DataVersion", version);
        }
        
        public static int LoadDataVersion()
        {
            return PlayerPrefs.GetInt("DataVersion", 0);
        }
        
        public static void SetPlayerAddress(string address)
        {
            PlayerPrefs.SetString("PlayerAddress", address);
        }
        
        public static string LoadPlayerAddress()
        {
            return PlayerPrefs.GetString("PlayerAddress", null);
        }
        
        public static void SetBotAddress(string address)
        {
            PlayerPrefs.SetString("BotAddress", address);
        }
        
        public static string LoadBotAddress()
        {
            return PlayerPrefs.GetString("BotAddress", null);
        }
        
        public static void SetIsGameWithBot(bool isGameWithBot)
        {
            PlayerPrefs.SetInt("IsGameWithBot", isGameWithBot ? 1 : 0);
        }
        
        public static bool LoadIsGameWithBot()
        {
            return PlayerPrefs.GetInt("IsGameWithBot", 0) == 1;
        }
        
        public static void SaveCurrentBoardId(string address)
        {
            PlayerPrefs.SetString("CurrentBoardId", address);
        }
        
        public static string GetCurrentBoardId()
        {
            return PlayerPrefs.GetString("CurrentBoardId", null);
        }
        
        public static void ClearCurrentBoardId()
        {
            PlayerPrefs.DeleteKey("CurrentBoardId");
        }

        public static void ClearAll()
        {
            PlayerPrefs.DeleteAll();
        }
    }
}