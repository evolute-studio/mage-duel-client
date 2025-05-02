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
        private static extern string get_controller_username();
        
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
        
    }
}