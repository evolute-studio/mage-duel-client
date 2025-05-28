using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;
    [SerializeField] Sprite _defaultCursorSprite;
    [SerializeField] Sprite _pointerCursorSprite;
    [SerializeField] RectTransform _cursorRect;
    [SerializeField] RectTransform _canvasRect;
    [SerializeField] Image _cursorImage;
    [SerializeField] Canvas _canvas;
    private bool _isCursorDisabled = false;
    
    private float baseWidth = 1920f;
    private float baseHeight = 1080f;
    private Vector2 _baseCursorSize = new Vector2(34f, 50f);
    private float _currentScale = 1f;
    private Vector2 _currentCursorSize;
    

    private Vector2 _hotspot;

    // Corresponding hotspots
    public Vector2 defaultHotspot;
    public Vector2 pointerHotspot;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        if (_canvas == null)
        {
            _canvas = GetComponent<Canvas>();
        }
        Cursor.visible = false;
        _currentCursorSize = _baseCursorSize;
        _hotspot = defaultHotspot;
        SetCursor("default");
        UpdateCursorScale();
    }

    public void LateUpdate()
    {
        if (Input.touchCount == 1 || Input.touchCount == 2 && !_isCursorDisabled)
        {
            DisableCursor();
        }
        
        Vector2 mousePosition = Input.mousePosition;
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect,
            mousePosition, _canvas.worldCamera, out Vector2 localPoint);
        
        _cursorRect.anchoredPosition = localPoint + _hotspot;
    }
    
    public void DisableCursor()
    {
        _cursorImage.color = new Color(1f, 1f, 1f, 0f);
        _isCursorDisabled = true;
    }

    public void UpdateCursorScale()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float scaleX = screenWidth / baseWidth;
        float scaleY = screenHeight / baseHeight;
        
        _currentScale = Mathf.Min(scaleX, scaleY);
        _cursorRect.sizeDelta = _currentCursorSize * _currentScale;
    }

    public void SetCursor(string cursorType)
    {
        switch(cursorType.ToLower())
        {
            case "pointer":
                _cursorImage.sprite = _pointerCursorSprite;
                _hotspot = pointerHotspot;
                break;
            default:
                _cursorImage.sprite = _defaultCursorSprite;
                _hotspot = defaultHotspot;
                break;
        }
    }

    public void OnRectTransformDimensionsChange()
    {
        UpdateCursorScale();
    }
}