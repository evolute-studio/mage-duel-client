using System.Collections.Generic;
using TerritoryWars.Dojo;

namespace TerritoryWars.General
{
    public class PlayerCharactersManager
    {
        private static int _currentCharacterId
        {
            get
            {
                bool isLocalBurnerAccountExist = DojoGameManager.Instance.LocalBurnerAccount != null;
                if (!isLocalBurnerAccountExist)
                    return 0;
                bool isPlayerDataExist = DojoGameManager.Instance.GetPlayerData(DojoGameManager.Instance.LocalBurnerAccount.Address.Hex()) != null;
                if (!isPlayerDataExist)
                    return 0;
                return DojoGameManager.Instance.GetPlayerData(DojoGameManager.Instance.LocalBurnerAccount.Address.Hex())
                    .active_skin;
            }
        }

        private static int _opponentCharacterId => SessionManager.Instance.IsLocalPlayerHost ? SessionManager.Instance.PlayersData[1].skin_id : SessionManager.Instance.PlayersData[0].skin_id;
        private List<int> _availableCharacters = new List<int> { 0, 1 };

        public static int GetCurrentCharacterId()
        {
            return _currentCharacterId;
        }
        
        public static int GetOpponentCurrentCharacterId()
        {
            return _opponentCharacterId;
        }
        
        // public static void ChangeOpponentCurrentCharacterId(int id)
        // {
        //     _opponentCharacterId = id;
        // }
        
        public bool IsCharacterAvailable(int id)
        {
            return _availableCharacters.Contains(id);
        }
        
        public void AddCharacter(int id)
        {
            if(_availableCharacters.Contains(id))
                return;
            _availableCharacters.Add(id);
        }
    }
}