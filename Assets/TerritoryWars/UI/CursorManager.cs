using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using Cursor = UnityEngine.Cursor;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;
    [SerializeField]private GameObject _cursorObject;
    [SerializeField]private Image _cursorImage;
    [SerializeField]private RectTransform _cursorRectTransform;
    [SerializeField]private Canvas _canvas;
    [SerializeField]private RectTransform _canvasRectTransform;
    
    // Different cursor types you might want
    public Sprite defaultCursor;
    public Sprite pointerCursor;
    // etc.

    // Corresponding hotspots
    public Vector2 defaultHotspot;
    public Vector2 pointerHotspot;
    private Vector2 hotspot;

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
        Cursor.visible = false;
        SetCursor("default");
        
        // Отримуємо посилання на Canvas, якщо воно не встановлено
        if (_canvas == null)
        {
            _canvas = GetComponent<Canvas>();
        }
        hotspot = defaultHotspot;
    }

    public void Update()
    {
        Cursor.visible = false;
        Vector2 mousePosition = Input.mousePosition;
        
        // Конвертуємо позицію миші в локальні координати Canvas
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRectTransform,
            mousePosition,
            _canvas.worldCamera,
            out Vector2 localPoint
        );
        
        _cursorRectTransform.anchoredPosition = localPoint + hotspot;
    }

    public void SetCursor(string cursorType)
    {
        switch(cursorType.ToLower())
        {
            case "pointer":
                _cursorImage.sprite = pointerCursor;
                hotspot = pointerHotspot;
                break;
            default:
                _cursorImage.sprite = defaultCursor;
                hotspot = defaultHotspot;
                break;
        }
    }
}