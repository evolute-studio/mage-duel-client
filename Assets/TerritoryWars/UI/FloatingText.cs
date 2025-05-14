using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TerritoryWars.UI
{
    public class FloatingText
    {
        public bool active;
        public GameObject go;
        public Vector3 startPosition;
        public TextMeshProUGUI messageText;
        public Image icon;
        public Vector3 motion;
        public float duration;
        public float lastShown;

        public void Show()
        {
            active = true;
            lastShown = Time.time;
            go.SetActive(active);
        }

        public void Hide()
        {
            active = false;
            go.SetActive(active);
        }

        public void UpdateFloatingText()
        {
            if (!active) return;
        
            if(Time.time - lastShown > duration) 
                Hide();
            messageText.color = new Color(messageText.color.r, messageText.color.g, messageText.color.b, 1 - ((Time.time - lastShown) / duration));
            go.transform.position += motion * Time.deltaTime;
        
        }
    }
}