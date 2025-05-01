using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TerritoryWars.General;
using TerritoryWars.Tools;
using UnityEngine;

namespace TerritoryWars.ExternalConnections
{
    public static class WrapperConnectorTest
    {
        // [DllImport("__Internal")]
        // private static extern void CallAutomaticControllerLogin();

        [DllImport("__Internal")]
        private static extern void CallControllerLogin();

        [DllImport("__Internal")]
        private static extern void GetControllerUsername();

//         public static void TryAutomaticControllerLogin()
//         {
// #if UNITY_WEBGL && !UNITY_EDITOR
//             CustomLogger.LogDojoLoop("TryAutomaticControllerLogin");    
//             CallControllerLogin();
// #else
//             CustomLogger.LogDojoLoop("ControllerLogin called in non-WebGL build");
// #endif
//         }

        public static void ControllerLogin()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            CustomLogger.LogDojoLoop("ControllerLogin");
            CallControllerLogin();
#else
            CustomLogger.LogDojoLoop("ControllerLogin called in non-WebGL build");
#endif
        }

        public static void GetUsername()
        {
            if(!ApplicationState.IsController) return;
#if UNITY_WEBGL && !UNITY_EDITOR
            CustomLogger.LogDojoLoop("GetUsername");
            GetControllerUsername();
#else
            CustomLogger.LogDojoLoop("GetUsername called in non-WebGL build");
#endif
        }
    }
}