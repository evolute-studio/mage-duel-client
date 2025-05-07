using UnityEngine;
using UnityEngine.UI;

namespace TerritoryWars.UI.CharacterSelector
{
    public class CharacterSelectorItem : MonoBehaviour
    {
        public RectTransform rectTransform;
        
        public Character character;
        public Image CharacterIconSprite;
        public GameObject HighlightObject;
        public GameObject EquippedIconObject;
        public GameObject LockedIconObject;
        public CanvasGroup canvasGroup;

        public CharacterSelectorItem Initialize(Character character)
        {
            this.character = character;
            CharacterIconSprite.sprite = character.CharacterIcon;
            return this;
        }

        public void SetActive(bool active)
        {
            
        }
        
        public CharacterSelectorItem SetCharacterIcon(Sprite sprite)
        {
            CharacterIconSprite.sprite = sprite;
            return this;
        }
        
        public CharacterSelectorItem SetEquipped(bool equipped)
        {
            EquippedIconObject.SetActive(equipped);
            return this;
        }
        
        public CharacterSelectorItem SetLocked(bool locked)
        {
            LockedIconObject.SetActive(locked);
            return this;
        }
        
        public CharacterSelectorItem SetHighlight(bool highlight)
        {
            HighlightObject.SetActive(highlight);
            return this;
        }
        
    }
}