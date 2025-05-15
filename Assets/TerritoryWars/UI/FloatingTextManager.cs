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
            public List<FloatingTextIcon> icons = new List<FloatingTextIcon>();
            
            public AnimationCurve animationCurve;
            public AnimationCurve positionCurve;
            public float BaseCameraOrthographicSize = 5f;

            private void Awake()
            {
                Instance = this;
            }
            private void Update()
            {
                foreach (FloatingText txt in floatingTexts)
                {
                    txt.UpdateFloatingText(animationCurve, positionCurve, BaseCameraOrthographicSize);
                }
            }
    
            public void Show(string msg, Vector3 position, Vector3 motion, float duration, string iconName = null)
            {
                FloatingText floatingText = GetFloatingText();
                floatingText.messageText.text = msg;
                //floatingText.messageText.fontSize = fontSize;
                //floatingText.messageText.color = color;
                floatingText.iconImage.sprite = icons.Find(i => i.iconName == iconName)?.icon;

                floatingText.go.transform.position = position;
                floatingText.go.transform.localScale = Vector3.one;
                floatingText.motion = motion;
                floatingText.duration = duration;
                floatingText.startPos = position;
        
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
                    txt.iconImage = txt.go.GetComponentInChildren<Image>();
            
                    floatingTexts.Add(txt);
                }

                return txt;
            }
        }
    }
