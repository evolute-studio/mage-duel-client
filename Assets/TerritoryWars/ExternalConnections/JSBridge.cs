using UnityEngine;

namespace TerritoryWars.ExternalConnections
{
    public static class JSBridge
    {
        public static void CopyValue(string value)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            Application.ExternalEval($"navigator.clipboard.writeText('{value}')");            
#endif
        }

        public static void ReloadPage()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            Application.ExternalEval("location.reload();");
#endif
        }
    }
}