using System;
using Dojo.Starknet;
using TerritoryWars.General;
using UnityEngine;
using UnityEngine.Serialization;

namespace TerritoryWars.Dojo
{
    public class DojoGameController : MonoBehaviour
    {
        public Game gameSystem;
        [FormerlySerializedAs("gameManagerDojo")] public DojoGameManager dojoGameManager;
        
        private GeneralAccount localPlayer => dojoGameManager.LocalAccount;

        private string logMessages = "";
        private Vector2 scrollPosition;
        private GUIStyle buttonStyle;
        private GUIStyle textFieldStyle;
        private GUIStyle logStyle;
        private bool stylesInitialized;

        public FieldElement SnapshotGameId;
        
        private void Awake()
        {
            // subscribe to logging events
            Application.logMessageReceived += HandleLog;
        }
        
        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
        }
        
        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            string prefix = type == LogType.Error ? "ERROR: " : 
                           type == LogType.Warning ? "WARNING: " : "INFO: ";
            logMessages = $"{prefix}{logString}\n{logMessages}";
        }

        public async void CreateGame() { }
        
        public async void CancelGame()
        {
        }

        public async void JoinGame(string hostPlayer)
        {
        }
        
        private void InitStyles()
        {
            if (!stylesInitialized)
            {
                buttonStyle = new GUIStyle(GUI.skin.button);
                buttonStyle.fontSize = (int)(GUI.skin.button.fontSize * 1.5f);
                
                textFieldStyle = new GUIStyle(GUI.skin.textField);
                textFieldStyle.fontSize = (int)(GUI.skin.textField.fontSize * 1.5f);
                
                logStyle = new GUIStyle(GUI.skin.textArea);
                logStyle.fontSize = (int)(GUI.skin.textArea.fontSize * 1.5f);
                
                stylesInitialized = true;
            }
        }

        private void OnGUI()
        {
            InitStyles();
            
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            
            if (GUILayout.Button("Create Game", buttonStyle))
            {
                CreateGame();
            }
            
            if (GUILayout.Button("Cancel Game", buttonStyle))
            {
                CancelGame();
            }
            
            GUILayout.Label("Host Address:", buttonStyle);
            hostAddress = GUILayout.TextField(hostAddress, textFieldStyle, GUILayout.Width(280));
            
            if (GUILayout.Button("Join Game", buttonStyle))
            {
                if (!string.IsNullOrEmpty(hostAddress))
                {
                    JoinGame(hostAddress);
                }
                else
                {
                    Debug.LogWarning("Please enter the host address");
                }
            }
            
            if(localPlayer != null)
            {
                GUILayout.Label($"Local Player: {localPlayer.Address.Hex()}", buttonStyle);
                GUILayout.TextField(localPlayer.Address.Hex(), textFieldStyle);
            }
            
            GUILayout.EndArea();
            
            
            
            
         
        }
        
        private string hostAddress = "";
    }
}