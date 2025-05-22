using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;
using UnityEngine.SceneManagement;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;
    private Camera MainCamera;
    [SerializeField]private GameObject _cursorObject;
    [SerializeField]private SpriteRenderer _cursorSpriteRenderer;
    
    // Different cursor types you might want
    public Sprite defaultCursor;
    public Sprite pointerCursor;
    // etc.

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
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
        
        UpdateMainCamera();
        Cursor.visible = false;
        SetCursor("default");
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateMainCamera();
    }

    private void UpdateMainCamera()
    {
        MainCamera = Camera.main;
        if (MainCamera == null)
        {
            Debug.LogWarning("Main camera not found! Cursor position might be incorrect.");
        }
    }

    public void Update()
    {
        if (MainCamera != null)
        {
            Vector2 cursorPos = MainCamera.ScreenToWorldPoint(Input.mousePosition);
            float spriteHeight = _cursorSpriteRenderer.bounds.size.y;
            float spriteWidth = _cursorSpriteRenderer.bounds.size.x;
            cursorPos.y -= spriteHeight / 2;
            cursorPos.x += spriteWidth / 2;
            _cursorObject.transform.position = cursorPos;
        }
    }

    public void SetCursor(string cursorType)
    {
        switch(cursorType.ToLower())
        {
            case "pointer":
                _cursorSpriteRenderer.sprite = pointerCursor;
                break;
            default:
                _cursorSpriteRenderer.sprite = defaultCursor;
                break;
        }
    }
}