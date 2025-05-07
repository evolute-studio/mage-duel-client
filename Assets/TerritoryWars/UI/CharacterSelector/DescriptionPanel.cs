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

        public bool IsOwned;
        public bool IsEquipped;

        public void SetInfo(Character character, int playerBalance, bool equipped)
        {
            CharacterNameText.text = character.CharacterName;
            CharacterDescriptionText.text = character.CharacterDescription;
            if (playerBalance >= character.CharacterCost)
            {
                EvoluteIcon.SetActive(false);
                CharacterCostText.text = "Owned";
                ApplyButton.interactable = !equipped;
                ButtonText.text = equipped ? "Equipped" : "Apply";
            }
            else
            {
                EvoluteIcon.SetActive(true);
                CharacterCostText.text = $" x {character.CharacterCost.ToString()} to unlock";
                ApplyButton.interactable = false;
                ButtonText.text = "Locked";
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