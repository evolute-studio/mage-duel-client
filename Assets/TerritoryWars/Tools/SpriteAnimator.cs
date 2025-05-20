using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

namespace TerritoryWars.Tools
{

    public class SpriteAnimator : MonoBehaviour
    {
        public Sprite[] sprites = new Sprite[0];
        public float duration = 1f;
        public bool loop = true;
        public float delay = 0f;
        public bool randomizeStart = false;
        public bool randomSpriteStart = false;
        private int _randomSpriteIndex = 0;
        public float maxRandomDelay = 0.5f;
        public float waitBetweenLoops = 0f;
        public bool playOnAwake = true;
        public Action OnAnimationEnd;
        public Action OnCompleteAction;

        [SerializeField] private SpriteRenderer _spriteRenderer;
        private Image _image;
        private bool _isUI;

        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _image = GetComponent<Image>();
            _isUI = _image != null;

            if (playOnAwake)
            {
                Play();
            }
        }

        public void Validate()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _image = GetComponent<Image>();
            _isUI = _image != null;
        }

        public void ChangeSprites(Sprite[] AnimationSprites)
        {
            sprites = AnimationSprites;
        }

        public void Play()
        {
            if(!gameObject.activeSelf) return;
            Stop();
            if (sprites == null || sprites.Length == 0 || (_spriteRenderer == null && _image == null))
            {
                Debug.LogWarning("No sprites to animate or no renderer/image component found");
                return;
            }

            if (sprites.Length == 1)
            {
                SetSprite(sprites[0]);
                return;
            }

            StartCoroutine(Animate());
        }

        public SpriteAnimator Play(Sprite[] animation, float duration = default)
        {
            Stop();
            sprites = animation;
            if (duration > 0)
            {
                this.duration = duration;
            }
            Play();
            return this;
        }
        
        public void OnComplete(Action action)
        {
            OnAnimationEnd = action;
        }
        

        public void Stop()
        {
            StopAllCoroutines();
        }

        private void SetSprite(Sprite sprite)
        {
            if (_isUI)
            {
                _image.sprite = sprite;
            }
            else
            {
                _spriteRenderer.sprite = sprite;
            }
        }

        private IEnumerator Animate()
        {
            if (randomizeStart)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(0f, maxRandomDelay));
            }
            else if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }

            if (sprites != null && randomSpriteStart)
            {
                _randomSpriteIndex = UnityEngine.Random.Range(0, sprites.Length);
            }

            while (true)
            {
                if (randomSpriteStart)
                {
                    if (sprites != null)
                    {
                        for(int i = _randomSpriteIndex; i < sprites.Length; i++)
                        {
                            SetSprite(sprites[i]);
                            yield return new WaitForSeconds(duration / sprites.Length);
                        }
                        _randomSpriteIndex = 0;
                    }
                }
                else
                {
                    foreach (var sprite in sprites)
                    {
                        SetSprite(sprite);
                        yield return new WaitForSeconds(duration / sprites.Length);
                    }
                }
                
                OnAnimationEnd?.Invoke();
                if (OnCompleteAction != null)
                {
                    OnCompleteAction.Invoke();
                    OnCompleteAction = null;
                }

                if (!loop)
                {
                    break;
                }
                
                if (waitBetweenLoops > 0)
                {
                    yield return new WaitForSeconds(waitBetweenLoops);
                }
            }
        }
        
        public void PlaySpecial(Sprite[] specialSprites, float duration)
        {
            Stop();
            StartCoroutine(SpecialAnimation(specialSprites, duration, () => Play()));
        }

        private IEnumerator SpecialAnimation(Sprite[] specialSprites, float duration, Action callback = null)
        {
            foreach (var sprite in specialSprites)
            {
                SetSprite(sprite);
                yield return new WaitForSeconds(duration / specialSprites.Length);
            }
            callback?.Invoke();
        }

        public void OnEnable()
        {
            Play();
        }

        public void OnDisable()
        {
            Stop();
        }
    }
}