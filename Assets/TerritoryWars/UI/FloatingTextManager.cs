using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TerritoryWars.UI
{
    public class FloatingTextManager : MonoBehaviour
    {
            public static FloatingTextManager Instance;
            public GameObject textContainer;
            public GameObject textPrefab;
    
    
            private List<FloatingText> floatingTexts = new List<FloatingText>();

            private void Awake()
            {
                Instance = this;
            }
            private void Update()
            {
                foreach (FloatingText txt in floatingTexts)
                {
                    txt.UpdateFloatingText();
                }
            }
    
            public void Show(string msg, int fontSize, Color color, Vector3 position, Vector3 motion, float duration)
            {
                FloatingText floatingText = GetFloatingText();
                floatingText.messageText.text = msg;
                floatingText.messageText.fontSize = fontSize;
                floatingText.messageText.color = color;

                floatingText.go.transform.position = position;
                floatingText.go.transform.localScale = Vector3.one;
                floatingText.motion = motion;
                floatingText.duration = duration;
        
                floatingText.Show();
            }
    
        
    

            private FloatingText GetFloatingText()
            {
                FloatingText txt = floatingTexts.Find(t => !t.active);
        
                if (txt == null)
                {
                    txt = new FloatingText();
                    txt.go = Instantiate(textPrefab);
                    txt.go.transform.SetParent(textContainer.transform);
                    txt.messageText = txt.go.GetComponentInChildren<TextMeshProUGUI>();
                    txt.icon = txt.go.GetComponentInChildren<Image>();
            
                    floatingTexts.Add(txt);
                }

                return txt;
            }
        }
    }
