using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TerritoryWars.Tools
{
    [RequireComponent(typeof(CursorOnHover), typeof(Image))]
    public class CustomButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Custom Button Settings")]
        [SerializeField] public Sprite DefaultSprite;
        [SerializeField] public Sprite HoverOutlineSprite;
        [SerializeField] public Sprite PressedSprite;
        [SerializeField] public Sprite PressedHoverOutlineSprite;
        [SerializeField] public Sprite DisabledSprite;
        public RectTransform ButtonContent;
        
        [SerializeField] public float HoverTransitionTime = 0.2f;
        [SerializeField] public float PressedDuration = 0.5f;
        public float PressedContentOffsetY = 0.15f;
        
        [SerializeField] public Image Image;
        [SerializeField] public Image HoverImage;

        public bool WithHoverAnimation = true;
        public bool WithPressedAnimation = true;
        
        private Vector2 _defaultContentPosition;
        private Button _button;
        
        private bool _isPressed;
        
        protected void Awake()
        { ;
            Image = GetComponent<Image>();
            if (!Image)
            {
                Debug.LogError("CustomButton requires an Image component.");
            }
            _button = GetComponent<Button>();
            if (!_button)
            {
                Debug.LogError("CustomButton requires a Button component.");
            }
            if(ButtonContent)
            {
                _defaultContentPosition = ButtonContent.anchoredPosition;
            }
            
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if(_button != null && _button.interactable == false || !WithHoverAnimation)
            {
                return;
            }
            if (HoverOutlineSprite != null && HoverImage != null)
            {
                HoverImage.DOKill();
                HoverImage.sprite = _isPressed ? PressedHoverOutlineSprite : HoverOutlineSprite;
                HoverImage.gameObject.SetActive(true);
                HoverImage.DOFade(1, HoverTransitionTime);
            }
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!WithHoverAnimation) return;
            
            if (HoverOutlineSprite != null && HoverImage != null)
            {
                HoverImage.DOKill();
                HoverImage.DOFade(0, HoverTransitionTime).OnComplete(() =>
                {
                    HoverImage.sprite = DefaultSprite;
                    HoverImage.gameObject.SetActive(false);
                });
            }
        }

        public void SetPressed(bool isPressed)
        {
            if (isPressed)
            {
                _isPressed = true;
                HoverImage.sprite = PressedHoverOutlineSprite;
                Image.sprite = PressedSprite;
            }
            else
            {
                _isPressed = false;
                HoverImage.sprite = HoverOutlineSprite;
                Image.sprite = DefaultSprite;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!WithPressedAnimation) return;
            
            if (_button != null && _button.interactable == false && PressedSprite != null && Image != null)
            {
                return;
            }
            _isPressed = true;
            HoverImage.sprite = PressedHoverOutlineSprite;
            if(ButtonContent) ButtonContent.anchoredPosition = new Vector2(_defaultContentPosition.x, _defaultContentPosition.y + PressedContentOffsetY);
            Image.sprite = PressedSprite;
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!WithPressedAnimation) return;
            
            if (_button != null && _button.interactable == false)
            {
                return;
            }
            Image.DOKill();
            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(PressedDuration);
            seq.AppendCallback(() =>
            {
                Image.sprite = DefaultSprite;
                HoverImage.sprite = HoverOutlineSprite;
                if(ButtonContent) ButtonContent.anchoredPosition = _defaultContentPosition;
                _isPressed = false;
            });
            seq.Play();
        }
    }
}