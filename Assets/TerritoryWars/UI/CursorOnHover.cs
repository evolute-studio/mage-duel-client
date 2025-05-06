using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CursorOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    
    public string cursorType = "pointer";
    public UnityEvent onEnter;
    public UnityEvent onExit;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.SetCursor(cursorType);
        }
        onEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.SetCursor("default");
        }
        onExit?.Invoke();
    }
}