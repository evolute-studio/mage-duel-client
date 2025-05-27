using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace TerritoryWars.Tools
{
    public enum LogType
    {
        Info,
        Warning,
        Execution,
        Error,
        Important,
        DojoLoop,
        Analytics,
        Filtering,
    }
    
    public static class CustomLogger
    {
        public static Dictionary<LogType, string> LogTypeColors = new Dictionary<LogType, string>
        {
            {LogType.Info, "#808080"},      
            {LogType.Warning, "#FFA500"},
            {LogType.Execution, "#32CD32"}, 
            {LogType.Error, "#DC143C"},        
            {LogType.Important, "#9441e0"},
            {LogType.DojoLoop, "#FFD700"},
            {LogType.Analytics, "#FF4500"},
            {LogType.Filtering, "#808080"},
        };
        
        public static Dictionary<LogType, bool> LogTypeEnabled = new Dictionary<LogType, bool>
        {
            {LogType.Info, false},
            {LogType.Warning, false},
            {LogType.Execution, true},
            {LogType.Error, true},
            {LogType.Important, true},
            {LogType.DojoLoop, true},
            {LogType.Analytics, true},
            {LogType.Filtering, false},
            
        };
        
        public static void Log(LogType logType, string message, Exception exception = null)
        {
            if (!LogTypeEnabled[logType]) return;
            
            string color = LogTypeColors[logType];
            string logMessage = $"<color={color}>[{logType}]: {message}</color>";

            switch (logType)
            {
                case LogType.Error:
                    if (exception != null)
                    {
                        exception.Data.Add("LogMessage", logMessage);
                        Debug.LogException(exception);
                    }
                    else
                    {
                        Debug.LogError(logMessage);
                    }
                    break;
                default:
                    Debug.Log(logMessage);
                    break;
            }
        }
        
        public static void LogObject(object obj, string label = null)
        {
            if (obj == null)
            {
                Debug.LogWarning("LogObject: null");
                return;
            }

            string output;

            try
            {
                output = JsonConvert.SerializeObject(obj, Formatting.Indented);
                // if (output.Length > 10000)
                // {
                //     string objType = obj.GetType().Name;
                //     string fileName = $"{objType}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                //     System.IO.File.WriteAllText(fileName, output);
                // }
            }
            catch (Exception e)
            {
                Debug.LogError("JsonConvert failed: " + e.Message);
                output = obj.ToString();
            }
            Debug.Log(string.IsNullOrEmpty(label) ? output : $"{label}:\n{output}");
        }
        
        public static void LogInfo(string message)
        {
            Log(LogType.Info, message);
        }
        
        public static void LogWarning(string message)
        {
            Log(LogType.Warning, message);
        }
        
        public static void LogExecution(string message)
        {
            Log(LogType.Execution, message);
        }
        
        public static void LogError(string message, Exception exception = null)
        {
            Log(LogType.Error, $"{message}", exception);
        }
        
        public static void LogImportant(string message)
        {
            Log(LogType.Important, message);
        }
        
        public static void LogDojoLoop(string message)
        {
            Log(LogType.DojoLoop, message);
        }

        public static void LogAnalytics(string message)
        {
            Log(LogType.Analytics, message);
        }
        
        public static void LogFiltering(string message)
        {
            Log(LogType.Filtering, message);
        }


    }
}