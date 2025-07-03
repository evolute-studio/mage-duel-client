using System;
using System.Collections.Generic;
using UnityEngine;

namespace TerritoryWars.Managers.Reports
{
    [CreateAssetMenu(menuName = "Data/Discord/Reports", fileName = "Reports")]
    public class ReportConfigs : ScriptableObject
    {
        public List<ReportConfig> Configs;
        
        public ReportConfig GetReportConfig(ReportType type)
        {
            foreach (var config in Configs)
            {
                if (config.Type == type)
                {
                    return config;
                }
            }
            return default;
        }
        
        public void OnValidate(){
            foreach (var config in Configs)
            {
                if (string.IsNullOrEmpty(config.Name))
                {
                    config.SetName();
                }
            }
        }
        
    }

    [Serializable]
    public struct ReportConfig
    {
        [HideInInspector] public string Name;
        
        public ReportType Type;
        public string WebhookUrl;
        public string WebhookUsername;
        public string Title;
        public string Message;
        public bool TagUsers;
        public List<string> UsersToTag;
        public Color Color;
        public bool IncludeLogs;
        public bool IncludeScreenshots;
        
        public void SetName()
        {
            Name = Type.ToString();
        }
        
        public string GetUsersToTagContent()
        {
            // format "<@464742638738997250>"
            if (UsersToTag == null || UsersToTag.Count == 0)
            {
                return string.Empty;
            }
            var formattedUsers = new List<string>();
            foreach (var userId in UsersToTag)
            {
                if (ulong.TryParse(userId, out var id))
                {
                    formattedUsers.Add($"<@{id}>");
                }
                else
                {
                    Debug.LogWarning($"Invalid user ID: {userId}");
                }
            }
            return string.Join(" ", formattedUsers);
        }
    }
    
    public enum ReportType
    {
        Feedback,
        Bug,
        CriticalIssue,
        
        PossibleProblem
    }
}