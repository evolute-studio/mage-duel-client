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
        
        private int _currentSelectedCharacterId = 1;
        private int _currentCharacterIndex = 1;
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
            
            while(_currentSelectedCharacterId != Characters[2].CharacterId)
            {
                ShiftCharacters(true);
            }
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
                InitializeCharacters();
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
            for (int i = 0; i < CharacterItems.Count; i++)
            {
                int cIndex = (i + Characters.Count) % Characters.Count;
                CharacterItems[i].Initialize(Characters[cIndex])
                    .SetLocked(!Characters[cIndex].IsUnlocked)
                    .SetEquipped(Characters[cIndex].CharacterId == _currentSelectedCharacterId);
            }
            var characterItem = CharacterItems[Characters.Count/2];
            characterItem.SetHighlight(true);
            DescriptionPanel.SetInfo(characterItem.character, _playerBalance, characterItem.character.CharacterId == _currentSelectedCharacterId);
            CharacterAnimator.Play(characterItem.character.IdleSprites, characterItem.character.IdleAnimationDuration);
            CharacterAnimator.PlaySpecial(characterItem.character.SelectedSprites, characterItem.character.SelectedAnimationDuration);
        }

        public void ShiftCharacters(bool isRight)
        {
            int shift = isRight ? 1 : -1;
            _currentCharacterIndex = (_currentCharacterIndex + shift + Characters.Count) % Characters.Count;
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
            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < CharacterItems.Count; i++)
            {
                var item = CharacterItems[i];
                int newIndex = (i + shift + CharacterItems.Count) % CharacterItems.Count;
                int targetAlpha = newIndex == 0 || newIndex == 4 ? 0 : 1;
                item.DOKill();
                sequence.Join(item.rectTransform.DOAnchorPos(CharacterItemsPositions[newIndex], 0.5f));
                sequence.Join(item.canvasGroup.DOFade(targetAlpha, 0.5f));
            }
            sequence.OnComplete(() =>
            {
                for (int i = 0; i < CharacterItems.Count; i++)
                {
                    if(i == 0 || i == 4)
                        CharacterItems[i].canvasGroup.alpha = 0;
                    else
                        CharacterItems[i].canvasGroup.alpha = 1;
                    CharacterItems[i].rectTransform.anchoredPosition = CharacterItemsPositions[i];
                }
                InitializeCharacters();
            });
            sequence.Play();
        }

        public void EquipCurrentCharacter()
        {
            Character currentCharacter = Characters[_currentSelectedCharacterId];
            if (!currentCharacter.IsUnlocked || currentCharacter.CharacterId == _currentSelectedCharacterId)
            {
                return;
            }
            
            _currentSelectedCharacterId = currentCharacter.CharacterId;
            InitializeCharacters();
        }
    }
}