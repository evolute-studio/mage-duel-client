using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BookLightAnimation : MonoBehaviour
{
    private float _speed = 1f;
    private float _size = 1.05f;
    private float _materialWidth = 0.03f;
    [SerializeField] private Image _image;
    [SerializeField] private Image _bookImage;
    
    

    public void Awake()
    {
        PlayAnimation();
    }

    public void OnEnable()
    {
        PlayAnimation();
    }

    public void PlayAnimation()
    {
        // _image.transform.DOKill();
        // _image.transform.DOScale(new Vector3(_image.transform.localScale.x * _size, _image.transform.localScale.y * _size,
        //     _image.transform.localScale.z * _size), _speed).SetLoops(-1, LoopType.Yoyo);
        // _image.DOFade(0.1f, _speed).SetLoops(-1, LoopType.Yoyo);
        DOTween.Kill(this);
        _bookImage.material.SetFloat("_Width", _materialWidth);
        DOTween.To(() => _bookImage.material.GetFloat("_Width"),x =>
            _bookImage.material.SetFloat("_Width", x), 0.01f, _speed).SetLoops(-1, LoopType.Yoyo);
        
    }

    public void OnDisable()
    {
        _image.transform.DOKill();
    }
}
