using TerritoryWars.Tools;
using UnityEngine;

public class TileAnimator : MonoBehaviour
{
    [SerializeField] private GameObject _splashAnimationObject;
    
    public void PlaySplashAnimation()
    {
        _splashAnimationObject.SetActive(true);
        SpriteAnimator spriteAnimator = _splashAnimationObject.GetComponentInChildren<SpriteAnimator>();
        spriteAnimator.OnAnimationEnd = () =>
        {
            _splashAnimationObject.SetActive(false);
            spriteAnimator.OnAnimationEnd = null;
        };
    }
}
