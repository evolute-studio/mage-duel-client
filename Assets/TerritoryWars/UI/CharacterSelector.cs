using System;
using System.Collections.Generic;
using DG.Tweening;
using TerritoryWars.Dojo;
using TerritoryWars.General;
using TerritoryWars.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TerritoryWars.UI
{
    public class CharacterSelector : MonoBehaviour
    {
        public Character[] characters;

        public Vector3[] positions;
        public float[] scales;
        public Color[] brightness;
        public int[] order;
        public float animationDuration = 0.5f;
        public Button ApplyButton;
        public TextMeshProUGUI ApplyButtonText;
        private int currentCharacterIndex = 0;
        public GameObject NotActiveButton;
        public GameObject CostTextParent;
        public TextMeshProUGUI CostText;
        public GameObject AppliedText;
        public Image ApplyButtonImage;
        private bool isAnimating = false;

        public Sprite ActiveButtonSprite;
        public Sprite DisabledButtonSprite;

        public void Initialize()
        {
            if (MenuUIController.Instance._namePanelController.EvoluteBalance == 0)
            {
                PlayerCharactersManager.ClearAvailableCharacters();
            }
            
            ApplyButton.onClick.AddListener(ApplyButtonClicked);
            
            currentCharacterIndex = PlayerCharactersManager.GetCurrentCharacterId();
            
            List<int> unlockedCharacters = PlayerCharactersManager.GetAvailableCharacters();

            foreach (var character in characters)
            {
                if(character.Locker == null) continue;
                
                if (unlockedCharacters.Contains(character.CharacterId))
                {
                    character.Locker?.FastUnlock();
                }
                else
                {
                    character.Locker?.gameObject.SetActive(true);
                }
            }
            
            while(currentCharacterIndex != characters[1].CharacterId)
            {
                ShiftCharacters(true);
            }
            
            
            UpdateButtons();
            foreach (var character in characters)
            {
                character.Initialize();
            }
        }

        public void ShiftCharacters(bool isRight)
        {
            if (isRight)
            {
                Character temp = characters[characters.Length - 1];
                for (int i = characters.Length - 1; i > 0; i--)
                {
                    characters[i] = characters[i - 1];
                }
                characters[0] = temp;
            }
            else
            {
                Character temp = characters[0];
                for (int i = 0; i < characters.Length - 1; i++)
                {
                    characters[i] = characters[i + 1];
                }
                characters[characters.Length - 1] = temp;
            }

            foreach (var character in characters)
            {
                character.Initialize();
            }

            for (int i = 0; i < characters.Length; i++)
            {
                characters[i].CharacterRenderer.sortingOrder = order[i];
                characters[i].RockRenderer.sortingOrder = order[i] - 1;
                var locker = characters[i].Locker;
                if (locker != null)
                {
                    locker.GetComponent<Canvas>().sortingOrder = order[i] + 1;
                    locker.IconRenderer.color = brightness[i];
                }
                if (i == 1)
                {
                    Sequence sequence = DOTween.Sequence();
                    sequence.AppendInterval(animationDuration * 0.5f);
                    sequence.AppendCallback(() =>
                    {
                        characters[1].CharacterAnimator.PlaySpecial(characters[1].SelectedSprites);
                    });
                    sequence.Play();
                }

                UpdateButtons();


                characters[i].MainObject.transform
                    .DOLocalMove(positions[i], animationDuration)
                    .SetEase(Ease.InOutExpo);

                characters[i].MainObject.transform
                    .DOScale(new Vector3(scales[i], scales[i], 1), animationDuration)
                    .SetEase(Ease.InOutExpo);

                characters[i].CharacterRenderer
                    .DOColor(brightness[i], animationDuration)
                    .SetEase(Ease.InOutExpo);

                characters[i].RockRenderer
                    .DOColor(brightness[i], animationDuration)
                    .SetEase(Ease.InOutExpo);
            }
        }

        public void ApplyButtonClicked()
        {
            currentCharacterIndex = characters[1].CharacterId;
            characters[1].Locker?.Unlock();
            PlayerCharactersManager.SaveCharacter(characters[1].CharacterId);
            DojoGameManager.Instance.ChangePlayerSkin(characters[1].CharacterId);
            UpdateButtons();
        }

        public void UpdateButtons()
        {
            Character character = characters[1];
            if (!character.Locker || character.Locker.isUnlocked)
            {
                ApplyButton.gameObject.SetActive(true);
                NotActiveButton.SetActive(false);
                CostTextParent.SetActive(false);
                if (currentCharacterIndex == character.CharacterId)
                {
                    ApplyButton.interactable = false;
                    ApplyButtonText.text = "APPLY";
                    ApplyButton.GetComponent<Image>().sprite = DisabledButtonSprite;
                    ApplyButton.GetComponent<Image>().color = new Color(127f / 255f, 127f / 255f, 127f / 255f, 1);
                    ApplyButton.GetComponent<CanvasGroup>().alpha = 0.74f;
                }
                else
                {
                    ApplyButton.GetComponent<Image>().sprite = ActiveButtonSprite;
                    ApplyButton.GetComponent<Image>().color = new Color(1, 1, 1, 1);
                    ApplyButton.GetComponent<CanvasGroup>().alpha = 1;
                    ApplyButton.interactable = true;
                    ApplyButtonText.text = "APPLY";
                }
            }
            else
            {
                int evoluteBalance = MenuUIController.Instance._namePanelController.EvoluteBalance;
                if (evoluteBalance >= character.Locker.cost)
                {
                    character.Locker.Unlock();
                }

                ApplyButton.gameObject.SetActive(false);
                NotActiveButton.SetActive(true);
                CostTextParent.SetActive(true);
                CostText.text = "x " + character.Locker.cost.ToString();
            }
            
        }
    }

    [Serializable]
    public class Character
    {
        public GameObject MainObject;

        public GameObject CharacterObject;
        
        public int CharacterId;
        [HideInInspector]
        public SpriteRenderer CharacterRenderer;
        [HideInInspector]
        public SpriteAnimator CharacterAnimator;

        public GameObject RockObject;
        [HideInInspector]
        public SpriteRenderer RockRenderer;
        
        public Sprite[] IdleSprites;
        public Sprite[] SelectedSprites;
        public Locker Locker;

        public void Initialize()
        {
            CharacterRenderer = CharacterObject.GetComponent<SpriteRenderer>();
            RockRenderer = RockObject.GetComponent<SpriteRenderer>();
            CharacterAnimator = CharacterObject.GetComponent<SpriteAnimator>();
        }
    }
}