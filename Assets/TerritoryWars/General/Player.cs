using DG.Tweening;
using Dojo.Starknet;
using TerritoryWars.DataModels;
using TerritoryWars.General;
using TerritoryWars.Tools;
using TerritoryWars.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace TerritoryWars.General
{
    public class Player : MonoBehaviour
    {
        public string PlayerId;
        public int PlayerSide;
        public int JokerCount;
        public int ActiveSkin;


        private Animator _animator;
        private CharacterAnimator _characterAnimator;



        public SpriteRenderer GFX;

        public void Initialize(SessionPlayer player)
        {
            SetData(player);
            _animator = GetComponent<Animator>();
            _characterAnimator = new CharacterAnimator(_animator);
        }

        public void SetData(SessionPlayer player)
        {
            PlayerId = player.PlayerId;
            PlayerSide = player.PlayerSide;
            JokerCount = player.JokerCount;
            ActiveSkin = ActiveSkin != 0 && player.ActiveSkin == 0 ? ActiveSkin : player.ActiveSkin;
        }

        public void UpdateData(int jokerCount)
        {
            JokerCount = jokerCount;
        }

        public void StartSelectingAnimation()
        {
            _characterAnimator.PlayCast(true);
        }

        public void EndTurnAnimation()
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
                topCenterPosition, Quaternion.identity, GameUI.Instance.playerInfoUI.gameObject.transform);

            bubble.transform.DOMove(bubble.transform.position + new Vector3(0, 1, 0), 1f)
                .OnComplete(() => { Destroy(bubble); });
        }
    }
}