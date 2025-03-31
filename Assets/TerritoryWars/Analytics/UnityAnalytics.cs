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
            await UnityServices.InitializeAsync();
            AnalyticsService.Instance.StartDataCollection();
            CustomLogger.LogAnalytics($"Started UGS Analytics Sample with user ID: {AnalyticsService.Instance.GetAnalyticsUserID()}");
        }
        
        
    }
}