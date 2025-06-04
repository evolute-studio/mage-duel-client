using System.Collections.Generic;
using System.Linq;
using TerritoryWars.Dojo;
using TerritoryWars.Managers.SessionComponents;
using UnityEngine;

namespace TerritoryWars.General
{
    public class PlayerCharactersManager
    {
        private static int _currentCharacterId
        {
            get
            {
                bool isLocalBurnerAccountExist = DojoGameManager.Instance.LocalAccount != null;
                if (!isLocalBurnerAccountExist)
                    return 0;
                bool isPlayerDataExist = DojoGameManager.Instance.GetPlayerData(DojoGameManager.Instance.LocalAccount.Address.Hex()) != null;
                if (!isPlayerDataExist)
                    return 0;
                return DojoGameManager.Instance.GetPlayerData(DojoGameManager.Instance.LocalAccount.Address.Hex())
                    .active_skin;
            }
        }

        private static int _opponentCharacterId => SessionManager.Instance.SessionContext.IsLocalPlayerHost 
            ? SessionManager.Instance.SessionContext.PlayersData[1].ActiveSkin 
            : SessionManager.Instance.SessionContext.PlayersData[0].ActiveSkin;
        private const string _availableCharactersKey = "AvailableCharacters";
        private static List<int> _availableCharactersList = new List<int>();
        
        public static List<int> GetAvailableCharacters()
        {
            if (PlayerPrefs.HasKey(_availableCharactersKey))
            {
                string characters = PlayerPrefs.GetString(_availableCharactersKey);
                string[] charactersArray = characters.Split('/');
                var filteredArray = charactersArray.Where(item => !string.IsNullOrWhiteSpace(item)).ToArray();
                _availableCharactersList.Clear();
                foreach (string character in filteredArray)
                {
                    _availableCharactersList.Add(int.Parse(character));
                }
            }
            else
            {
                PlayerPrefs.SetString(_availableCharactersKey, "0/1/");
                string characters = PlayerPrefs.GetString(_availableCharactersKey);
                string[] charactersArray = characters.Split('/');
                var filteredArray = charactersArray.Where(item => !string.IsNullOrWhiteSpace(item)).ToArray();
                _availableCharactersList.Clear();
                foreach (string character in filteredArray)
                {
                    _availableCharactersList.Add(int.Parse(character));
                }
            }

            return _availableCharactersList;
        }

        public static void SaveCharacter(int id)
        {
            GetAvailableCharacters();
            
            if (_availableCharactersList.Contains(id))
                return;
            
            _availableCharactersList.Add(id);
            string characters = "";
            foreach (int character in _availableCharactersList)
            {
                characters += character + "/";
            }
            PlayerPrefs.SetString(_availableCharactersKey, characters);
        }
        
        public static void ClearAvailableCharacters()
        {
            if(PlayerPrefs.HasKey(_availableCharactersKey))
                PlayerPrefs.DeleteKey(_availableCharactersKey);
        }

        public static int GetCurrentCharacterId()
        {
            return _currentCharacterId;
        }
        
        public static int GetOpponentCurrentCharacterId()
        {
            return _opponentCharacterId;
        }
        
    }
}