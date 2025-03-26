using UnityEngine;

namespace TerritoryWars
{
    public static class SimpleStorage
    {
        public static void SetPlayerAddress(string address)
        {
            PlayerPrefs.SetString("PlayerAddress", address);
        }
        
        public static string LoadPlayerAddress()
        {
            return PlayerPrefs.GetString("PlayerAddress", null);
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
    }
}