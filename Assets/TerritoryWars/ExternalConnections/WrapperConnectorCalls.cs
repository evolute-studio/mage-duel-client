using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TerritoryWars.General;
using TerritoryWars.Tools;
using UnityEngine;

namespace TerritoryWars.ExternalConnections
{
    public static class WrapperConnectorCalls
    {

        public delegate void LoginCallback(int success);

        [DllImport("__Internal")]
        private static extern bool is_controller_logged_in();
        
        [DllImport("__Internal")]
        private static extern void controller_login();
        
        [DllImport("__Internal")]
        private static extern void controller_logout();
        
        [DllImport("__Internal")]
        private static extern void open_controller_profile();

        [DllImport("__Internal")]
        private static extern string get_controller_username();

        [DllImport("__Internal")]
        private static extern void execute_controller(string transaction);
        
        [DllImport("__Internal")]
        private static extern string get_connection_data();
        
        public static bool IsControllerLoggedIn()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            CustomLogger.LogDojoLoop("IsControllerLoggedIn");
            bool isLoggedIn = is_controller_logged_in();
            return isLoggedIn;
#else
            {
                CustomLogger.LogDojoLoop("IsControllerLoggedIn called in non-WebGL build");
                return false;
            }
#endif
        }
        
        public static void ControllerLogin()
        {   
        #if UNITY_WEBGL && !UNITY_EDITOR
            CustomLogger.LogDojoLoop("ControllerLogin");
            controller_login();
        #else
            CustomLogger.LogDojoLoop("ControllerLogin called in non-WebGL build");
        #endif
        }

        public static void ControllerLogout()
        {
        #if UNITY_WEBGL && !UNITY_EDITOR
            CustomLogger.LogDojoLoop("ControllerLogout");
            controller_logout();
#else
            CustomLogger.LogDojoLoop("ControllerLogout called in non-WebGL build");
#endif
        }

        public static void ControllerProfile()
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            CustomLogger.LogDojoLoop("ControllerProfile");
            open_controller_profile();
#else
            CustomLogger.LogDojoLoop("ControllerProfile called in non-WebGL build");
#endif
        }

        public static string GetUsername()
        {
            if(!ApplicationState.IsController) return null;
#if UNITY_WEBGL && !UNITY_EDITOR
            CustomLogger.LogDojoLoop("GetUsername");
            string username = get_controller_username();
            return username;
#else
            CustomLogger.LogDojoLoop("GetUsername called in non-WebGL build");
            return null;
#endif
        }

        public static void ExecuteController(string transaction)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            CustomLogger.LogDojoLoop("ExecuteController");
            execute_controller(transaction);
#else
            CustomLogger.LogDojoLoop("ExecuteController called in non-WebGL build");
#endif
        }

        public static ConnectionData GetConnectionData()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            string connectionData = get_connection_data();
            Debug.Log("Connection data: " + connectionData);
            CustomLogger.LogDojoLoop("GetConnectionData");
            ConnectionData data = JsonUtility.FromJson<ConnectionData>(connectionData);
            return data;
#else
            CustomLogger.LogDojoLoop("GetConnectionData called in non-WebGL build");
            return new ConnectionData();
#endif
            
        }
        
        public struct ConnectionData
        {
            public string rpcUrl;
            public string toriiUrl;
            public string gameAddress;
            public string playerProfileActionsAddress;
            public string worldAddress;
            public int slotDataVersion;
        }
        
        

    }
}