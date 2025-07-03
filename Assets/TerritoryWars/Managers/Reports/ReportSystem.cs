using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using TerritoryWars.Dojo;
using TerritoryWars.ExternalConnections;
using TerritoryWars.Tools;
using UnityEngine;

namespace TerritoryWars.Managers.Reports
{
    public class ReportSystem : MonoBehaviour
    {
        public static ReportSystem Instance;
        
        public string webhookUrl;
        public ReportConfigs Configs;
        public bool SendInEditor = true;
        public List<string> KeywordsForAutoSend;
        private DiscordWebhook webhook;
        
        private readonly List<string> recentLogs = new List<string>();
        private const int MaxLogCount = 1000;
        
        public char divider = '-';
        public int count = 50;

        public void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            webhook = new DiscordWebhook(webhookUrl);
            
            Application.logMessageReceived += HandleLog;
        }
        
        [ContextMenu("Send Test Report")]
        public void SendTest()
        {
            CustomLogger.LogImportant("EditorAutoSendTest");
            
            //SendReportTest();
            // SendReport(ReportType.Feedback, "Feedback message for testing purposes.");
            // SendReport(ReportType.Bug, "Bug message for testing purposes.");
            // SendReport(ReportType.CriticalIssue, "Critical issue message for testing purposes.");
            // SendReport(ReportType.PossibleProblem, "Possible problem message for testing purposes.");
        }

        public void SendReport(ReportType type, string message)
        {
            if (string.IsNullOrEmpty(webhookUrl))
            {
                Debug.LogError("Webhook URL is not set.");
                return;
            }
            
            var config = Configs.GetReportConfig(type);
            
            StartCoroutine(webhook.SendEmbed(
                title: config.Title,
                description: message,
                color: ColorToDiscordInt(config.Color),
                imageData: config.IncludeScreenshots ? GetScreenshot() : null,
                imageFileName: config.IncludeScreenshots ? "screenshot.png" : null,
                fileData: config.IncludeLogs ? GetLogsFile() : null,
                fileName: config.IncludeLogs ? "logs.txt" : null,
                content: $"{new string(divider, count)}\n" + (config.TagUsers ? config.GetUsersToTagContent() : null),
                allowedMentionsUserIds: config.TagUsers ? config.UsersToTag : null
            ));
        }
        
        public void SendReportTest()
        {
            if (string.IsNullOrEmpty(webhookUrl))
            {
                Debug.LogError("Webhook URL is not set.");
                return;
            }
            
            var config = Configs.GetReportConfig(ReportType.Feedback);
            
            StartCoroutine(webhook.SendEmbed(
                title: config.Title,
                description: "message",
                color: config.Color.GetHashCode(),
                fields: new List<(string name, string value, bool inline)> { ("Field1", "Value1", true), ("Field2", "Value2", false) },
                imageData: GetScreenshot(),
                imageFileName: "screenshot.png",
                fileData: GetLogsFile(),
                fileName: "logs.txt",
                // add divider and count to the content
                content: $"{new string(divider, count)}\n" + config.GetUsersToTagContent(),
                allowedMentionsUserIds: config.UsersToTag
            ));
        }

        private void HandleLog(string logString, string stackTrace, UnityEngine.LogType type)
        {
            string formattedLog = $"[{System.DateTime.Now.ToUniversalTime():HH:mm:ss}] [{type}] {logString}";
            if (type == UnityEngine.LogType.Exception || type == UnityEngine.LogType.Error)
            {
                formattedLog += $"\nStackTrace:\n{stackTrace}";
            }
            else if (type == UnityEngine.LogType.Warning)
                return;
            
            lock (recentLogs)
            {
                recentLogs.Add(formattedLog);
                if (recentLogs.Count > MaxLogCount)
                {
                    recentLogs.RemoveAt(0);
                }
            }

            foreach (var keyword in KeywordsForAutoSend)
            {
                if (logString.Contains(keyword))
                {
                    SendReport(ReportType.PossibleProblem, $"Keyword '{keyword}' found in log: {logString}");
                    break;
                }
            }
            
        }

        

        private byte[] GetScreenshot()
        {
            var screenshot = ScreenCapture.CaptureScreenshotAsTexture();
            return screenshot.EncodeToPNG();
        }
        
        private byte[] GetLogsFile()
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("=== Mage Duel Logs ===");
            sb.AppendLine($"Time: {System.DateTime.Now.ToUniversalTime()}");
            sb.AppendLine();
            
            sb.AppendLine("=== System Info ===");
            sb.AppendLine($"Platform: {Application.platform}");
            sb.AppendLine($"Application Version: {Application.version}");
            sb.AppendLine();
            
            sb.AppendLine("=== Account Info ===");
            if (DojoGameManager.Instance?.LocalAccount == null)
            {
                sb.AppendLine("No account information available.");
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("Address: " + DojoGameManager.Instance.LocalAccount.Address.Hex());
                sb.AppendLine("IsController: " + DojoGameManager.Instance.LocalAccount.IsController);
                sb.AppendLine();
            }
            
            sb.AppendLine("=== Session Data ===");
            if (DojoGameManager.Instance?.GlobalContext?.SessionContext == null)
            {
                sb.AppendLine("No context information available.");
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("BoardId: " + DojoGameManager.Instance.GlobalContext.SessionContext.Board.Id);
                sb.AppendLine("GameWithBot: " + DojoGameManager.Instance.GlobalContext.SessionContext.IsGameWithBot);
                sb.AppendLine("BoardData: " +
                              JsonConvert.SerializeObject(DojoGameManager.Instance.GlobalContext.SessionContext.Board));
                sb.AppendLine("SessionPlayer 1: " +
                              JsonConvert.SerializeObject(DojoGameManager.Instance.GlobalContext.SessionContext.PlayersData[0]));
                sb.AppendLine("SessionPlayer 2: " + 
                              JsonConvert.SerializeObject(DojoGameManager.Instance.GlobalContext.SessionContext.PlayersData[1]));
                sb.AppendLine();
            }
            
            
            // // Додаємо інформацію про увімкнені типи логів
            // sb.AppendLine("=== Enabled Log Types ===");
            // foreach (LogType logType in System.Enum.GetValues(typeof(LogType)))
            // {
            //     if (CustomLogger.LogTypeEnabled[logType])
            //     {
            //         sb.AppendLine($"{logType} (Color: {CustomLogger.LogTypeColors[logType]})");
            //     }
            // }
            sb.AppendLine();
            
            sb.AppendLine("=== Recent Logs ===");
            lock (recentLogs)
            {
                foreach (var log in recentLogs)
                {
                    sb.AppendLine(log);
                }
            }
            
            return System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        }
        
        private int ColorToDiscordInt(Color color)
        {
            Color32 c = color;
            return (c.r << 16) | (c.g << 8) | c.b;
        }
        
        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
        }
    }
}