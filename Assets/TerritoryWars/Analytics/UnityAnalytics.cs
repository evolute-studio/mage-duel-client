using System.Collections.Generic;
using TerritoryWars.Tools;
using Unity.Services.Analytics;
using Unity.Services.CloudDiagnostics;
using Unity.Services.Core;
using UnityEngine;

namespace TerritoryWars.Analytics
{
    public class UnityAnalytics: MonoBehaviour
    {
        public static UnityAnalytics Instance { get; private set; }
        // Analytics Sample
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        
        private async void Initialize()
        {
            try 
            {
                await UnityServices.InitializeAsync();
                
                // Ініціалізація аналітики
                AnalyticsService.Instance.StartDataCollection();
                
                // Ініціалізація крашлітики
                //await CrashReportingService.Instance.InitializeAsync();
                
                CustomLogger.LogAnalytics($"Started UGS Analytics Sample with user ID: {AnalyticsService.Instance.GetAnalyticsUserID()}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to initialize analytics: {e.Message}");
            }
        }

        private void OnApplicationQuit()
        {
            // Відправка даних про завершення сесії
            AnalyticsService.Instance.Flush();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // Відправка даних при паузі (особливо важливо для WebGL)
                AnalyticsService.Instance.Flush();
            }
        }
    }
}