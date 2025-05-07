using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TerritoryWars.UI.Menu
{
    public class MenuBackground : MonoBehaviour
    {
        public Image DefaultImage;
        public Image DarkenImage;
        
        public Sprite DefaultBackgroundSprite;
        public Sprite DarkenBackgroundSprite;
        
        public void SetBackground(bool darken)
        {
            if (darken)
            {
                DarkenImage.DOFade(1, 0.5f);
            }
            else
            {
                DarkenImage.DOFade(0, 0.5f);
            }
        }
    }
}