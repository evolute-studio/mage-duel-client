using UnityEngine;
using TMPro;

[ExecuteInEditMode]
[RequireComponent(typeof(TextMeshProUGUI))]
public class SimpleTextOutline : MonoBehaviour
{
    [Header("Outline Settings")]
    [SerializeField] private Color outlineColor = Color.black;
    [SerializeField] private float outlineThickness = 1.2f; // Товщина аутлайну
    [SerializeField] private Vector2 outlineOffset = Vector2.zero; // Додатковий зсув, якщо потрібно
    
    private TextMeshProUGUI mainText;
    private GameObject outlineObject;
    private TextMeshProUGUI outlineText;
    private RectTransform outlineRectTransform;
    
    private void OnEnable()
    {
        mainText = GetComponent<TextMeshProUGUI>();
        SetupOutlineObject();
        UpdateOutline();
    }
    
    private void OnDisable()
    {
        if (outlineObject != null)
        {
            if (Application.isPlaying)
                Destroy(outlineObject);
            else
                DestroyImmediate(outlineObject);
        }
    }
    
    private void SetupOutlineObject()
    {
        // Перевіряємо, чи існує об'єкт аутлайну
        if (outlineObject == null)
        {
            // Створюємо об'єкт для аутлайну
            outlineObject = new GameObject(gameObject.name + "_Outline");
            outlineObject.transform.SetParent(transform.parent);
            
            // Додаємо компоненти
            outlineRectTransform = outlineObject.AddComponent<RectTransform>();
            outlineText = outlineObject.AddComponent<TextMeshProUGUI>();
            
            // Копіюємо налаштування RectTransform
            CopyRectTransformValues();
            
            // Налаштовуємо текст аутлайну
            SetupOutlineText();
            
            // Розміщуємо об'єкт аутлайну перед основним текстом в ієрархії
            outlineObject.transform.SetSiblingIndex(transform.GetSiblingIndex());
        }
    }
    
    private void SetupOutlineText()
    {
        if (outlineText != null && mainText != null)
        {
            // Копіюємо всі важливі налаштування з основного тексту
            outlineText.font = mainText.font;
            outlineText.fontSize = mainText.fontSize;
            outlineText.fontStyle = mainText.fontStyle;
            outlineText.alignment = mainText.alignment;
            outlineText.text = mainText.text;
            outlineText.enableWordWrapping = mainText.enableWordWrapping;
            outlineText.overflowMode = mainText.overflowMode;
            outlineText.color = outlineColor;
            
            // Збільшуємо розмір тексту для створення ефекту аутлайну
            outlineText.fontSize = mainText.fontSize + outlineThickness;
            
            // Вимикаємо взаємодію з аутлайном
            outlineText.raycastTarget = false;
        }
    }
    
    private void CopyRectTransformValues()
    {
        if (outlineRectTransform != null && transform is RectTransform mainRectTransform)
        {
            // Копіюємо всі важливі значення RectTransform
            outlineRectTransform.anchorMin = mainRectTransform.anchorMin;
            outlineRectTransform.anchorMax = mainRectTransform.anchorMax;
            outlineRectTransform.pivot = mainRectTransform.pivot;
            outlineRectTransform.sizeDelta = mainRectTransform.sizeDelta;
            outlineRectTransform.anchoredPosition = mainRectTransform.anchoredPosition + outlineOffset;
            outlineRectTransform.localRotation = mainRectTransform.localRotation;
            outlineRectTransform.localScale = mainRectTransform.localScale;
        }
    }
    
    private void Update()
    {
        if (mainText == null || outlineText == null) return;
        
        // Оновлюємо текст і налаштування, якщо вони змінилися
        if (outlineText.text != mainText.text)
        {
            outlineText.text = mainText.text;
        }
        
        if (outlineText.fontSize != mainText.fontSize + outlineThickness)
        {
            outlineText.fontSize = mainText.fontSize + outlineThickness;
        }
        
        // Оновлюємо позицію і розмір
        if (transform is RectTransform mainRectTransform)
        {
            outlineRectTransform.anchoredPosition = mainRectTransform.anchoredPosition + outlineOffset;
            outlineRectTransform.sizeDelta = mainRectTransform.sizeDelta;
        }
        
        // Підтримуємо правильний порядок в ієрархії
        if (outlineObject.transform.GetSiblingIndex() >= transform.GetSiblingIndex())
        {
            outlineObject.transform.SetSiblingIndex(transform.GetSiblingIndex());
        }
    }
    
    public void UpdateOutline()
    {
        if (outlineObject != null)
        {
            SetupOutlineText();
            CopyRectTransformValues();
        }
        else
        {
            SetupOutlineObject();
        }
    }
    
    private void OnValidate()
    {
        if (enabled && gameObject.activeInHierarchy)
        {
            UpdateOutline();
        }
    }
    
    // Публічні методи для зміни налаштувань
    public void SetOutlineColor(Color color)
    {
        outlineColor = color;
        if (outlineText != null)
        {
            outlineText.color = color;
        }
    }
    
    public void SetOutlineThickness(float thickness)
    {
        outlineThickness = thickness;
        UpdateOutline();
    }
    
    public void SetOutlineOffset(Vector2 offset)
    {
        outlineOffset = offset;
        CopyRectTransformValues();
    }
}