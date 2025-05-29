using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TerritoryWars.Dojo;
using TerritoryWars.ExternalConnections;
using TerritoryWars.General;
using TerritoryWars.Tools;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

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
        public Image CharacterShadow;
        public GameObject CharacterSelectorObject;
        public CanvasGroup CanvasGroup;
        public CursorOnHover ForegroundCursorOnHover;
        public GameObject Hint;
        [SerializeField] private List<GameObject> ObjectsToHide;
        
        private int _currentSelectedCharacterId = 1;
        private int _currentCharacterId = 1;
        private List<int> _unlockedCharacters = new List<int>();
        private uint _playerBalance => MenuUIController.Instance.NamePanelController.EvoluteBalance;

        // public void Start()
        // {
        //     Initialize();
        //     SetActive(false);
        // }

        public void Initialize()
        {
            _currentSelectedCharacterId = PlayerCharactersManager.GetCurrentCharacterId();
            _currentCharacterId = _currentSelectedCharacterId;
            InitializeCharacter(Characters.First(x => x.CharacterId == _currentSelectedCharacterId));

            foreach (var character in Characters)
            {
                character.IsUnlocked = _playerBalance >= character.CharacterCost;
                _unlockedCharacters.Add(character.CharacterId);
            }
            
            while(_currentSelectedCharacterId != Characters[2].CharacterId)
            {
                ShiftCharacters(true);
            }
        }
        
        public void SetActive(bool active)
        {
            InitializeCharacter(Characters.First(x => x.CharacterId == _currentSelectedCharacterId));
            while(_currentSelectedCharacterId != Characters[2].CharacterId)
            {
                ShiftCharacters(true);
            }
            if (active)
            {
                CharacterSelectorObject.SetActive(true);
                InitializeCharacters();
                CanvasGroup.alpha = 0;
                CanvasGroup.interactable = true;
                CanvasGroup.blocksRaycasts = true;

                DOTween.To(() => CanvasGroup.alpha, x => CanvasGroup.alpha = x, 1, 0.25f);
                ForegroundCursorOnHover.onEnter?.Invoke();
                ForegroundCursorOnHover.enabled = false;
                Hint.SetActive(false);
                RectTransform rectTransform = Hint.GetComponent<RectTransform>();
                Vector3 position = rectTransform.anchoredPosition;
                position.y = Characters.First(x => x.CharacterId == _currentSelectedCharacterId).HintPositionY;
                rectTransform.anchoredPosition = position;
                for (int i = 0; i < CharacterSelectorObject.transform.childCount; i++)
                {
                    Transform child = CharacterSelectorObject.transform.GetChild(i);
                    child.transform.localScale = Vector3.one * 0.75f;
                    child.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);
                }
                
                foreach (var objectToHide in ObjectsToHide)
                {
                    if (!objectToHide.TryGetComponent(out CanvasGroup objectCanvasGroup))
                    {
                        objectCanvasGroup = objectToHide.AddComponent<CanvasGroup>();
                    }
                    objectCanvasGroup.alpha = 1;
                    objectCanvasGroup.DOFade(0, 0.25f).SetEase(Ease.OutBack);
                }
            }
            else
            {
                InitializeCharacter(Characters.First(x => x.CharacterId == _currentSelectedCharacterId));
                CanvasGroup.alpha = 1;
                CanvasGroup.interactable = false;
                CanvasGroup.blocksRaycasts = false;
                
                DOTween.To(() => CanvasGroup.alpha, x => CanvasGroup.alpha = x, 0, 0.25f)
                    .OnComplete(() =>
                    {
                        CharacterSelectorObject.SetActive(false);
                        ForegroundCursorOnHover.onExit?.Invoke();
                        ForegroundCursorOnHover.enabled = true;
                        Hint.SetActive(true);
                    });
                for (int i = 0; i < CharacterSelectorObject.transform.childCount; i++)
                {
                    Transform child = CharacterSelectorObject.transform.GetChild(i);
                    child.transform.DOKill();
                    child.transform.DOScale(Vector3.one * 0.75f, 0.25f).SetEase(Ease.InBack);
                }
                
                foreach (var objectToHide in ObjectsToHide)
                {
                    if (!objectToHide.TryGetComponent(out CanvasGroup objectCanvasGroup))
                    {
                        objectCanvasGroup = objectToHide.AddComponent<CanvasGroup>();
                    }
                    objectCanvasGroup.alpha = 0;
                    objectCanvasGroup.DOFade(1, 0.25f).SetEase(Ease.OutBack);
                }
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
            _currentCharacterId = characterItem.character.CharacterId;
            characterItem.SetHighlight(true);
            DescriptionPanel.SetInfo(characterItem.character, _playerBalance, characterItem.character.CharacterId == _currentSelectedCharacterId);
            InitializeCharacter(characterItem.character);
        }

        private void InitializeCharacter(Character character)
        {
            CharacterAnimator.Play(character.IdleSprites, character.IdleAnimationDuration);
            CharacterAnimator.PlaySpecial(character.SelectedSprites, character.SelectedAnimationDuration);
            CharacterShadow.sprite = character.ShadowSprite;
            CharacterShadow.GetComponent<RectTransform>().anchoredPosition =
                character.CharacterShadowPosition;
        }

        public void ShiftCharacters(bool isRight)
        {
            int shift = isRight ? 1 : -1;
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
            Character currentCharacter = Characters.First(x => x.CharacterId == _currentCharacterId);
            if (!currentCharacter.IsUnlocked || currentCharacter.CharacterId == _currentSelectedCharacterId)
            {
                return;
            }
            _currentSelectedCharacterId = currentCharacter.CharacterId;
            InitializeCharacters();
            DojoConnector.ChangeSkin(DojoGameManager.Instance.LocalAccount, currentCharacter.CharacterId);
            SetActive(false);
            
        }
    }
}