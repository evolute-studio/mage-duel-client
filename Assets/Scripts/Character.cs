using DG.Tweening;
using Dojo.Starknet;
using TerritoryWars.General;
using TerritoryWars.Tools;
using UnityEngine;
using UnityEngine.Serialization;

public class Character : MonoBehaviour
{
    private Animator _animator;
    private CharacterAnimator _characterAnimator;
    public FieldElement Address;
    public int LocalId;
    public PlayerSide Side;
    public int JokerCount;
    public SpriteRenderer GFX;
    
    public void Initialize(FieldElement address,PlayerSide side, int jokerCount)
    {
        Address = address;
        Side = side;
        LocalId = side switch
        {
            PlayerSide.Blue => 0,
            PlayerSide.Red => 1,
            _ => 0
        };
        JokerCount = jokerCount;
        _animator = GetComponent<Animator>();
        _characterAnimator = new CharacterAnimator(_animator);
    }

    public void UpdateData(int jokerCount)
    {
        JokerCount = jokerCount;
    }
    public void StartSelecting()
    {
        _characterAnimator.PlayCast(true);
    }
    
    public void EndTurn()
    {
        _characterAnimator.PlayCast(false);
        _characterAnimator.PlayHit();
    }
    
    public void SetAnimatorController(RuntimeAnimatorController controller)
    {
        _animator = GetComponent<Animator>();
        _animator.runtimeAnimatorController = controller;
    }

    public void PlaySkippedBubbleAnimation()
    {
        Vector3 topCenterPosition = new Vector3(GFX.bounds.center.x, GFX.bounds.center.y, GFX.bounds.center.z);
        GameObject bubble = Instantiate(
            PrefabsManager.Instance.InstantiateObject(PrefabsManager.Instance.SkipBubblePrefab),
            topCenterPosition, Quaternion.identity, SessionManager.Instance.sessionUI.gameObject.transform);
        
        bubble.transform.DOMove(bubble.transform.position + new Vector3(0, 1, 0), 1f)
            .OnComplete(() =>
            {
                Destroy(bubble);
            });
    }
    
    
}
