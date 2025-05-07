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
        public bool IsEquipped;
        
        public float IdleAnimationDuration = 1f;
        public Sprite[] IdleSprites;
        public float SelectedAnimationDuration = 0.5f;
        public Sprite[] SelectedSprites;
    }
}