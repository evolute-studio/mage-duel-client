using System.Collections;
using UnityEngine;
using System.Text;
using TerritoryWars.Tools;
using System.Collections.Generic;
using Newtonsoft.Json;
using TerritoryWars.Dojo;
using TerritoryWars.ExternalConnections;
using LogType = TerritoryWars.Tools.LogType;

namespace TerritoryWars.Managers
{
    public class ReportSystem : MonoBehaviour
    {
        public static ReportSystem Instance;
        public string webhookUrl;
        private DiscordWebhook webhook;
        
        private readonly List<string> recentLogs = new List<string>();
        private const int MaxLogCount = 1000; // Зберігаємо останні 100 логів

        public void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Ініціалізуємо вебхук
            webhook = new DiscordWebhook(webhookUrl);
            
            // Підписуємось на логи
            Application.logMessageReceived += HandleLog;
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void HandleLog(string logString, string stackTrace, UnityEngine.LogType type)
        {
            string formattedLog = $"[{System.DateTime.Now:HH:mm:ss}] [{type}] {logString}";
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
        }

        [ContextMenu("Send Test Report")]
        public void SendTest()
        {
            SendReport("Test");
        }
        
        private void SendReport(string message)
        {
            if (string.IsNullOrEmpty(webhookUrl))
            {
                Debug.LogError("Webhook URL is not set.");
                return;
            }

            var fields = new List<(string name, string value, bool inline)>
            {
                ("Text File", "Here is a text file with some data.", true)
            };

            StartCoroutine(webhook.SendEmbed(
                title: "Report",
                description: "<@464742638738997250>",
                color: 0xFF0000, // червоний
                fields: fields,
                imageData: GetScreenshot(),
                imageFileName: "screenshot.png",
                fileData: GetLogsFile(),
                fileName: "logs.txt"
            ));
        }

        private byte[] GetScreenshot()
        {
            var screenshot = ScreenCapture.CaptureScreenshotAsTexture();
            return screenshot.EncodeToPNG();
        }
        
        private byte[] GetLogsFile()
        {
            var sb = new StringBuilder();
            
            // Додаємо заголовок
            sb.AppendLine("=== Territory Wars Logs ===");
            sb.AppendLine($"Time: {(System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1)))}");
            sb.AppendLine();
            
            // Додаємо системну інформацію
            sb.AppendLine("=== System Info ===");
            sb.AppendLine($"Platform: {Application.platform}");
            sb.AppendLine($"Application Version: {Application.version}");
            sb.AppendLine();
            
            // Account information
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
            
            // Session data
            sb.AppendLine("=== Session Data ===");
            if (DojoGameManager.Instance?.GlobalContext?.SessionContext == null)
            {
                sb.AppendLine("No context information available.");
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("BoardId: " + DojoGameManager.Instance.GlobalContext.SessionContext.Board.Id);
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
            
            // Додаємо останні логи
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
    }
}