using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TerritoryWars.Tools
{
    [RequireComponent(typeof(CursorOnHover), typeof(Image))]
    public class CustomButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
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
        
        private Vector2 _defaultContentPosition;
        private Button _button;
        
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
            if(_button != null && _button.interactable == false)
            {
                return;
            }
            if (HoverOutlineSprite != null && HoverImage != null)
            {
                HoverImage.DOKill();
                HoverImage.sprite = HoverOutlineSprite;
                HoverImage.gameObject.SetActive(true);
                HoverImage.DOFade(1, HoverTransitionTime);
            }
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
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
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (_button != null && _button.interactable == false)
            {
                return;
            }
            if (PressedSprite != null && Image != null)
            {
                HoverImage.sprite = PressedHoverOutlineSprite;
                if(ButtonContent) ButtonContent.anchoredPosition = new Vector2(_defaultContentPosition.x, _defaultContentPosition.y + PressedContentOffsetY);
                Image.DOKill();
                Sequence seq = DOTween.Sequence();
                Image.sprite = PressedSprite;
                seq.AppendInterval(PressedDuration);
                seq.AppendCallback(() =>
                {
                    Image.sprite = DefaultSprite;
                    HoverImage.sprite = HoverOutlineSprite;
                    if(ButtonContent) ButtonContent.anchoredPosition = _defaultContentPosition;
                });
                seq.Play();
            }
        }
    }
}