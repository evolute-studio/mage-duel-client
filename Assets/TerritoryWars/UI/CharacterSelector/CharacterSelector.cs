using System;
using System.Collections.Generic;
using DG.Tweening;
using TerritoryWars.General;
using TerritoryWars.Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace TerritoryWars.UI.CharacterSelector
{
    public class CharacterSelector : MonoBehaviour
    {
        [Header("Data")]
        public List<Character> Characters;
        public List<CharacterSelectorItem> CharacterItems;
        public DescriptionPanel DescriptionPanel;
        public Vector2[] CharacterItemsPositions;

        [Header("References")]
        public SpriteAnimator CharacterAnimator;
        public GameObject CharacterSelectorObject;
        public CanvasGroup CanvasGroup;
        public CursorOnHover ForegroundCursorOnHover;
        
        private int _currentSelectedCharacterIndex = 1;
        private int _currentCharacterIndex = 1;
        private int _charactersCount => CharacterItems.Count;
        private List<int> _unlockedCharacters = new List<int>() { 0, 1 };
        private int _playerBalance = 0;

        public void Start()
        {
            Initialize();
            SetActive(false);
        }

        public void Initialize()
        {
            // if (MenuUIController.Instance._namePanelController.EvoluteBalance == 0)
            // {
            //     PlayerCharactersManager.ClearAvailableCharacters();
            // }
            
            //_currentSelectedCharacterIndex = PlayerCharactersManager.GetCurrentCharacterId();
            
            //List<int> unlockedCharacters = PlayerCharactersManager.GetAvailableCharacters();

            foreach (var character in Characters)
            {
                character.IsUnlocked = _unlockedCharacters.Contains(character.CharacterId);
            }
            
            // while(currentCharacterIndex != characters[1].CharacterId)
            // {
            //     ShiftCharacters(true);
            // }
            //
            //
            // UpdateButtons();
            // foreach (var character in characters)
            // {
            //     character.Initialize();
            // }
        }
        
        public void SetActive(bool active)
        {
            if (active)
            {
                CharacterSelectorObject.SetActive(true);
                InitializeCharacters();
                CanvasGroup.alpha = 0;
                CanvasGroup.interactable = true;
                CanvasGroup.blocksRaycasts = true;

                DOTween.To(() => CanvasGroup.alpha, x => CanvasGroup.alpha = x, 1, 0.5f);
                ForegroundCursorOnHover.onEnter?.Invoke();
                ForegroundCursorOnHover.enabled = false;
            }
            else
            {
                CanvasGroup.alpha = 1;
                CanvasGroup.interactable = false;
                CanvasGroup.blocksRaycasts = false;
                
                DOTween.To(() => CanvasGroup.alpha, x => CanvasGroup.alpha = x, 0, 0.5f)
                    .OnComplete(() =>
                    {
                        CharacterSelectorObject.SetActive(false);
                        ForegroundCursorOnHover.onExit?.Invoke();
                        ForegroundCursorOnHover.enabled = true;
                    });
                
            }
        }

        private void InitializeCharacters()
        {
            for (int i = 0; i < Characters.Count; i++)
            {
                CharacterItems[i].Initialize(Characters[i])
                    .SetLocked(!Characters[i].IsUnlocked)
                    .SetEquipped(Characters[i].CharacterId == _currentSelectedCharacterIndex);

            }
            var characterItem = CharacterItems[Characters.Count/2];
            characterItem.SetHighlight(true);
            DescriptionPanel.SetInfo(characterItem.character, _playerBalance, characterItem.character.CharacterId == _currentSelectedCharacterIndex);
            CharacterAnimator.Play(characterItem.character.IdleSprites);
        }

        public void ShiftCharacters(bool isRight)
        {
            int shift = isRight ? 1 : -1;
            _currentCharacterIndex = (_currentCharacterIndex + shift + _charactersCount) % _charactersCount;
            if (isRight)
            {
                Character temp = Characters[^1];
                for (int i = Characters.Count - 1; i > 0; i--)
                {
                    Characters[i] = Characters[i - 1];
                }

                Characters[0] = temp;
            }
            else
            {
                Character temp = Characters[0];
                for (int i = 0; i < Characters.Count - 1; i++)
                {
                    Characters[i] = Characters[i + 1];
                }

                Characters[^1] = temp;
            }

            foreach (var item in CharacterItems)
            {
                
                
            }

            InitializeCharacters();
        }
    }
}