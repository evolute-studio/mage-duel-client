using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public class WebGLBuilder
{
    [MenuItem("Build/Build WebGL")]
    public static void BuildWebGL()
    {
        string[] args = Environment.GetCommandLineArgs();
        
        string buildName = "DefaultBuild";
        string version = "1.0.0";
        string outputPath = "./Builds";
        
        // Парсинг аргументів
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-customArgs" && i + 1 < args.Length)
            {
                string customArgs = args[i + 1];
                ParseCustomArgs(customArgs, ref buildName, ref version, ref outputPath);
            }
        }
        
        // Розгортання тильди в повний шлях
        if (outputPath.StartsWith("~/"))
        {
            outputPath = outputPath.Replace("~/", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/");
        }
        
        // Налаштування білда
        PlayerSettings.productName = buildName;
        PlayerSettings.bundleVersion = version;
        
        string[] scenes = GetScenePaths();
        
        // ВАЖЛИВО: для WebGL buildName має бути назвою папки, а не файлу
        string buildPath;
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        
        if (target == BuildTarget.WebGL)
        {
            // Для WebGL створюємо папку з назвою гри
            buildPath = Path.Combine(outputPath, buildName);
        }
        else
        {
            // Для інших платформ - повний шлях до виконуваного файлу
            buildPath = Path.Combine(outputPath, buildName);
            
            // Додаємо розширення для різних платформ
            switch (target)
            {
                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneWindows:
                    buildPath += ".exe";
                    break;
                case BuildTarget.StandaloneLinux64:
                    // Linux файли зазвичай без розширення
                    break;
                case BuildTarget.Android:
                    buildPath += ".apk";
                    break;
            }
        }
        
        Debug.Log($"Building to: {buildPath}");
        
        // Створення папки, якщо не існує
        Directory.CreateDirectory(Path.GetDirectoryName(buildPath));
        
        BuildPipeline.BuildPlayer(scenes, buildPath, target, BuildOptions.None);
        
        Debug.Log($"Build completed: {buildName} v{version} at {buildPath}");
    }
    
    private static void ParseCustomArgs(string customArgs, ref string buildName, ref string version, ref string outputPath)
    {
        Debug.Log($"Parsing custom args: {customArgs}");
        
        string[] pairs = customArgs.Split(';');
        foreach (string pair in pairs)
        {
            if (string.IsNullOrEmpty(pair)) continue;
            
            string[] keyValue = pair.Split('=');
            if (keyValue.Length == 2)
            {
                string key = keyValue[0].Trim();
                string value = keyValue[1].Trim().Trim('"'); // Видаляємо лайшні лапки
                
                Debug.Log($"Processing: {key} = {value}");
                
                switch (key)
                {
                    case "buildName":
                        buildName = value;
                        break;
                    case "version":
                        version = value;
                        break;
                    case "outputPath":
                        outputPath = value;
                        break;
                }
            }
        }
        
        Debug.Log($"Final values - buildName: {buildName}, version: {version}, outputPath: {outputPath}");
    }
    
    private static string[] GetScenePaths()
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        string[] scenePaths = new string[scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            scenePaths[i] = scenes[i].path;
        }
        return scenePaths;
    }
}