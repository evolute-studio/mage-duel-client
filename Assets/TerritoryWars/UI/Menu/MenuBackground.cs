using UnityEngine;
using UnityEngine.UI;

namespace TerritoryWars.UI.Menu
{
    public class MenuBackground : MonoBehaviour
    {
        public Image Image;
        
        public Sprite DefaultBackgroundSprite;
        public Sprite DarkenBackgroundSprite;
        
        public void SetBackground(bool darken)
        {
            if (darken)
            {
                Image.sprite = DarkenBackgroundSprite;
            }
            else
            {
                Image.sprite = DefaultBackgroundSprite;
            }
        }
    }
}