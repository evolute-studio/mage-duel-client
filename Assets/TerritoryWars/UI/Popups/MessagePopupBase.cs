using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TerritoryWars.UI.Popups
{
    public class MessagePopupBase : MonoBehaviour
    {
        public Transform Popup;
        public TextMeshProUGUI MessageText;

        public TextMeshProUGUI FirstOptionText;
        public TextMeshProUGUI SecondOptionText;
        public Button FirstOptionButton;
        public Button SecondOptionButton;
        
        public void SetActive(bool active)
        {
            if (active)
            {
                SetActiveTrueLogic();
                SetActiveTrueView();
            }
            else
            {
                SetActiveFalseView();
            }
        }

        protected virtual void SetActiveTrueLogic()
        {
            Popup.gameObject.SetActive(true);
        }

        protected virtual void SetActiveTrueView()
        {
            Popup.localScale = Vector3.zero;
            Sequence showSequence = DOTween.Sequence();
            showSequence.Append(Popup.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack))
                .Join(Popup.DOLocalMoveY(Popup.localPosition.y + 30f, 0.3f).From())
                .Join(Popup.GetComponent<CanvasGroup>().DOFade(1f, 0.3f).From(0f));
        }

        protected virtual void SetActiveFalseLogic()
        {
            Reset();
        }

        protected virtual void SetActiveFalseView()
        {
            Sequence hideSequence = DOTween.Sequence();
            hideSequence.Append(Popup.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack))
                .Join(Popup.GetComponent<CanvasGroup>().DOFade(0f, 0.2f))
                .OnComplete(SetActiveFalseLogic);
        }

        public void Setup(PopupConfig config)
        {
            SetMessage(config.Text);
            SetFirstOption(config.FirstOptionText, config.FirstOptionAction);
            SetSecondOption(config.SecondOptionText, config.SecondOptionAction);
        }
        
        public void SetMessage(string message)
        {
            MessageText.text = message;
        }
        
        public void SetFirstOption(string text, UnityAction action)
        {
            if(String.IsNullOrEmpty(text) || action == null)
            {
                FirstOptionButton.gameObject.SetActive(false);
                return;
            }
            FirstOptionButton.gameObject.SetActive(true);
            FirstOptionText.text = text;
            FirstOptionButton.onClick.AddListener(() =>
            {
                action?.Invoke();
                SetActive(false);
                
            });
        }
        
        public void SetSecondOption(string text, UnityAction action)
        {
            if(String.IsNullOrEmpty(text) || action == null)
            {
                SecondOptionButton.gameObject.SetActive(false);
                return;
            }
            SecondOptionButton.gameObject.SetActive(true);
            SecondOptionText.text = text;
            SecondOptionButton.onClick.AddListener(() =>
            {
                action?.Invoke();
                SetActive(false);
            });
        }
        
        public void Reset()
        {
            SetMessage("");
            SetFirstOption("", null);
            SetSecondOption("", null);
            Popup.gameObject.SetActive(false);
            FirstOptionButton.onClick.RemoveAllListeners();
            SecondOptionButton.onClick.RemoveAllListeners();
            FirstOptionButton.gameObject.SetActive(false);
            SecondOptionButton.gameObject.SetActive(false);
        }
    }
}