using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TerritoryWars.UI.CharacterSelector
{
    public class DescriptionPanel : MonoBehaviour
    {
        public TextMeshProUGUI CharacterNameText;
        public TextMeshProUGUI CharacterDescriptionText;
        public TextMeshProUGUI CharacterCostText;
        public GameObject EvoluteIcon;
        public Button ApplyButton;
        public TextMeshProUGUI ButtonText;
        public Image LockerButtonIcon;
        public Sprite LockerIcon;
        public Sprite EquipedIcon;

        public bool IsOwned;
        public bool IsEquipped;

        public void SetInfo(Character character, uint playerBalance, bool equipped)
        {
            CharacterNameText.text = character.CharacterName;
            CharacterDescriptionText.text = character.CharacterDescription;
            if (playerBalance >= character.CharacterCost)
            {
                EvoluteIcon.SetActive(false);
                CharacterCostText.text = "Owned";
                ApplyButton.interactable = true;
                ButtonText.text = "Equip";
                LockerButtonIcon.sprite = LockerIcon;
                LockerButtonIcon.gameObject.SetActive(false);
                if (equipped)
                {
                    ApplyButton.interactable = false;
                    ButtonText.text = "Equipped";
                    LockerButtonIcon.sprite = EquipedIcon;
                    LockerButtonIcon.gameObject.SetActive(true);
                    LockerButtonIcon.SetNativeSize();
                }
            }
            else
            {
                EvoluteIcon.SetActive(true);
                CharacterCostText.text = $" x {character.CharacterCost.ToString()} to unlock";
                ApplyButton.interactable = false;
                ButtonText.text = "Equip";
                LockerButtonIcon.sprite = LockerIcon;
                LockerButtonIcon.gameObject.SetActive(true);
                LockerButtonIcon.SetNativeSize();
            }
            
        }
        
        public void SetCharacterInfo(string name, string description, int cost)
        {
            CharacterNameText.text = name;
            CharacterDescriptionText.text = description;
            CharacterCostText.text = $" x {cost.ToString()} to unlock";
        }
        
        public void SetStatus(bool isOwned, bool isEquipped)
        {
            IsOwned = isOwned;
            IsEquipped = isEquipped;
            
            if (IsOwned)
            {
                ApplyButton.interactable = !IsEquipped;
                CharacterCostText.gameObject.SetActive(false);
            }
            else
            {
                ApplyButton.interactable = false;
                CharacterCostText.gameObject.SetActive(true);
            }
        }
    }
}