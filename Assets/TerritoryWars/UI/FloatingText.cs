using System;
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
        public Image iconImage;
        public Vector3 motion;
        public Vector3 startPos;
        public float duration;
        public float lastShown;
        
        
        private Vector3 endPosition => startPos + motion;

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

        public void UpdateFloatingText(AnimationCurve opacityCurve, AnimationCurve positionCurve, float baseCameraSize)
        {
            if (!active) return;
        
            if(Time.time - lastShown > duration) 
                Hide();
            float t = (Time.time - lastShown) / duration;
            messageText.color = new Color(messageText.color.r, messageText.color.g, messageText.color.b, opacityCurve.Evaluate(t));
            iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, opacityCurve.Evaluate(t));
            
            go.transform.position = Vector3.Lerp(startPos, endPosition, positionCurve.Evaluate(t));
            float currentCameraSize = Camera.main.orthographicSize;
            float scale = baseCameraSize / currentCameraSize; 
            go.transform.localScale = Vector3.one * scale;
        
        }
    }
    
    [Serializable]
    public class FloatingTextIcon
    {
        public string iconName;
        public Sprite icon;
    }
}