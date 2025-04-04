using UnityEngine;

namespace TerritoryWars
{
    public static class SimpleStorage
    {
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