using UnityEngine;
using TMPro;

[ExecuteInEditMode]
[RequireComponent(typeof(TextMeshProUGUI))]
public class CenterEllipsisText : MonoBehaviour
{
    [SerializeField] private string ellipsisString = "...";
    
    private TextMeshProUGUI textComponent;
    private string originalText;
    private RectTransform rectTransform;

    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
        originalText = textComponent.text;
    }

    private void OnEnable()
    {
        UpdateText();
    }

    private void Update()
    {
        if (textComponent.text != originalText)
        {
            originalText = textComponent.text;
            UpdateText();
        }
    }

    private void OnRectTransformDimensionsChange()
    {
        UpdateText();
    }

    private void UpdateText()
    {
        if (string.IsNullOrEmpty(originalText)) return;
        textComponent.text = TruncateMiddle(originalText, rectTransform.rect.width);
    }

    private string TruncateMiddle(string text, float maxWidth)
    {
        // Перевіряємо, чи потрібно скорочувати текст
        textComponent.text = text;
        if (textComponent.preferredWidth <= maxWidth)
            return text;

        // Знаходимо максимальну кількість символів, яка поміщається зліва
        int maxLeftChars = 0;
        textComponent.text = text.Substring(0, 1);
        while (textComponent.preferredWidth < maxWidth * 0.4f && maxLeftChars < text.Length / 2)
        {
            maxLeftChars++;
            textComponent.text = text.Substring(0, maxLeftChars);
        }
        maxLeftChars = Mathf.Max(1, maxLeftChars - 1);

        // Знаходимо максимальну кількість символів, яка поміщається справа
        int maxRightChars = 0;
        textComponent.text = text.Substring(text.Length - 1);
        while (textComponent.preferredWidth < maxWidth * 0.4f && maxRightChars < text.Length / 2)
        {
            maxRightChars++;
            textComponent.text = text.Substring(text.Length - maxRightChars);
        }
        maxRightChars = Mathf.Max(1, maxRightChars - 1);

        // Формуємо фінальний текст
        string leftPart = text.Substring(0, maxLeftChars);
        string rightPart = text.Substring(text.Length - maxRightChars);
        string result = leftPart + ellipsisString + rightPart;

        // Перевіряємо, чи поміщається результат
        textComponent.text = result;
        if (textComponent.preferredWidth > maxWidth)
        {
            // Якщо все ще не поміщається, зменшуємо праву частину
            while (textComponent.preferredWidth > maxWidth && maxRightChars > 1)
            {
                maxRightChars--;
                rightPart = text.Substring(text.Length - maxRightChars);
                result = leftPart + ellipsisString + rightPart;
                textComponent.text = result;
            }
        }

        return result;
    }
}