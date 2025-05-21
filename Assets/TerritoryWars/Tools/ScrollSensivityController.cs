using UnityEngine;
using UnityEngine.UI;

public class ScrollSensivityController : MonoBehaviour
{
    private const float MOBILE_SCROLL_SENSITIVITY = 0.12f;
    private const float DESKTOP_SCROLL_SENSITIVITY = 13f;
    
    private ScrollRect scrollRect;
    
    void Start()
    {
        scrollRect = GetComponent<ScrollRect>();
        SetSensitivityByPlatform();
    }
    
    private void SetSensitivityByPlatform()
    {
        if (Application.isMobilePlatform)
        {
            scrollRect.scrollSensitivity = MOBILE_SCROLL_SENSITIVITY;
        }
        else
        {
            scrollRect.scrollSensitivity = DESKTOP_SCROLL_SENSITIVITY;      
        }
    }

    
}
