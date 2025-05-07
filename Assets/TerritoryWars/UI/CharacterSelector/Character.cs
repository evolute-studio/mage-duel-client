using System;
using TerritoryWars.Tools;
using UnityEngine;

namespace TerritoryWars.UI.CharacterSelector
{
    [Serializable]
    public class Character
    {
        public int CharacterId;
        public Sprite CharacterIcon;
        public string CharacterName;
        public string CharacterDescription;
        public int CharacterCost;
        public bool IsUnlocked;
        
        public Sprite[] IdleSprites;
        public Sprite[] SelectedSprites;
    }
}