using System;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BookLightAnimation : MonoBehaviour
{
    private float _speed = 1f;
    private float _size = 1.05f;
    private float _materialWidth = 0.03f;
    [SerializeField] private Image _image;
    [SerializeField] private Image _bookImage;
    private const string RULES_READED_KEY = "RULES_READED";
    private const string ANIMATION_ID = "BookLightAnimation";
    
    public void Awake()
    {
        bool isRulesRead = PlayerPrefs.HasKey(RULES_READED_KEY) && PlayerPrefs.GetInt(RULES_READED_KEY) == 1;
        if (isRulesRead)
        {
            SetActiveBookLightAnimation(false);
        }
        else
        {
            SetActiveBookLightAnimation(true);
        }
    }

    public void PlayAnimation()
    {
        DOTween.Kill(ANIMATION_ID);
        _image.transform.DOScale(new Vector3(_image.transform.localScale.x * _size, _image.transform.localScale.y * _size,
            _image.transform.localScale.z * _size), _speed).SetLoops(-1, LoopType.Yoyo).SetId(ANIMATION_ID);
        _image.DOFade(0.1f, _speed).SetLoops(-1, LoopType.Yoyo).SetId(ANIMATION_ID);
        _bookImage.material.SetFloat("_Width", _materialWidth);
        DOTween.To(() => _bookImage.material.GetFloat("_Width"),x =>
            _bookImage.material.SetFloat("_Width", x), 0.01f, _speed).SetLoops(-1, LoopType.Yoyo).SetId(ANIMATION_ID);
    }
    
    public void SetActiveBookLightAnimation(bool isActive)
    {
        if (isActive)
        {
            PlayAnimation();
        }
        else
        {
            DOTween.Kill(ANIMATION_ID);
            _image.color = new Color(1, 1, 1, 0);
            _bookImage.material = default;
        }
    }

    public void BookButtonPressed()
    {
        PlayerPrefs.SetInt(RULES_READED_KEY, 1);
        SetActiveBookLightAnimation(false);
    }
}
