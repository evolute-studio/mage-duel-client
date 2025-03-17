using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace TerritoryWars.Tile
{
    public class TileRotator : MonoBehaviour
    {
        private TileRenderers _tileRenderers;
        
        public List<Transform> SimpleRotationObjects = new List<Transform>();
        public List<Transform> MirrorRotationObjects = new List<Transform>();
        public List<LineRenderer> LineRenderers = new List<LineRenderer>();
        public List<RoadSwapElement> SpriteSwapElements = new List<RoadSwapElement>();
        public List<SpriteLayerSwapElement> SpriteLayerSwapElements = new List<SpriteLayerSwapElement>();

        public UnityEvent OnRotation;

        [Header("Debug")]
        [SerializeField] private bool autoRotate = false;
        [SerializeField] private float rotateInterval = 1f;
        private float nextRotateTime;

        
        public void Awake()
        {
            _tileRenderers = GetComponent<TileRenderers>();
        }

        [ContextMenu("Rotate Clockwise")]
        public void RotateClockwise()
        {
            ApplyRotation();
        }

        public void RotateCounterClockwise()
        {
            ApplyRotation();
        }

        private void ApplyRotation()
        {
            foreach (var child in SimpleRotationObjects)
            {
                SimpleRotation(child);
            }

            foreach (var child in MirrorRotationObjects)
            {
                MirrorRotation(child);
            }

            foreach (var lineRenderer in LineRenderers)
            {
                LineRotation(lineRenderer);
            }
            
            foreach (var spriteSwapElement in SpriteSwapElements)
            {
                spriteSwapElement.Rotate();
                var pinsParent = spriteSwapElement.PinObjects[spriteSwapElement.CurrentIndex];
                Transform[] pins = new Transform[pinsParent.childCount];
                for (int i = 0; i < pinsParent.childCount; i++)
                {
                    pins[i] = pinsParent.GetChild(i);
                }
                _tileRenderers.PinsPositions = pins;
            }

            foreach (var spriteLayerSwapElement in SpriteLayerSwapElements)
            {
                spriteLayerSwapElement.Rotate();
            }
            OnRotation?.Invoke();
        }

        public void SimpleRotation(Transform obj, int times = 1)
        {
            for (int i = 0; i < times; i++)
            {
                Vector3 originalPos = obj.localPosition;
                Vector3 newPos = originalPos;

                newPos.x = originalPos.y * 2;
                newPos.y = originalPos.x / -2;
                obj.localPosition = newPos;
            }
        }

        public void MirrorRotation(Transform obj, int times = 1)
        {
            for (int i = 0; i < times; i++)
            {
                Vector3 originalPos = obj.localPosition;
                Vector3 newPos = originalPos;

                newPos.x = originalPos.y * 2;
                newPos.y = originalPos.x / -2;
                obj.localPosition = newPos;
                obj.localScale = new Vector3(-obj.localScale.x, obj.localScale.y, obj.localScale.z);
            }
        }

        public static void GetMirrorRotationStatic(Transform obj, int times = 1)
        {
            for (int i = 0; i < times; i++)
            {
                Vector3 originalPos = obj.localPosition;
                Vector3 newPos = originalPos;

                newPos.x = originalPos.y * 2;
                newPos.y = originalPos.x / -2;
                obj.localPosition = newPos;
                obj.localScale = new Vector3(-obj.localScale.x, obj.localScale.y, obj.localScale.z);
            }
        }

        public void LineRotation(LineRenderer lineRenderer, int times = 1)
        {
            Vector3[] positions = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(positions);
            for (int t = 0; t < times; t++)
            {
                for (int i = 0; i < positions.Length; i++)
                {
                    Vector3 originalPos = positions[i];
                    Vector3 newPos = originalPos;

                    newPos.x = originalPos.y * 2;
                    newPos.y = originalPos.x / -2;
                    positions[i] = newPos;
                }
                lineRenderer.SetPositions(positions);
            }
        }

        public void RotateTile(int times = 1)
        {
            if (SimpleRotationObjects != null)
            {
                foreach (var simpleRotationObject in SimpleRotationObjects)
                {
                    SimpleRotation(simpleRotationObject, times);
                }
            }
            
            if(MirrorRotationObjects != null)
            {
                foreach (var mirrorRotationObject in MirrorRotationObjects)
                {
                    MirrorRotation(mirrorRotationObject, times);
                }
            }
            
            if(LineRenderers != null)
            {
                foreach (var lineRenderer in LineRenderers)
                {
                    LineRotation(lineRenderer, times);
                }
            }

            if (SpriteSwapElements != null)
            {
                foreach (var spriteSwapElement in SpriteSwapElements)
                {
                    spriteSwapElement.Rotate(times);
                    var pinsParent = spriteSwapElement.PinObjects[spriteSwapElement.CurrentIndex];
                    Transform[] pins = new Transform[pinsParent.childCount];
                    for (int i = 0; i < pinsParent.childCount; i++)
                    {
                        pins[i] = pinsParent.GetChild(i);
                    }
                    _tileRenderers.PinsPositions = pins;
                }
            }

            if (SpriteLayerSwapElements != null)
            {
                foreach (var spriteLayerSwapElement in SpriteLayerSwapElements)
                {
                    spriteLayerSwapElement.Rotate(times);
                }
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                RotateClockwise();
            }

            if (autoRotate && Time.time >= nextRotateTime)
            {
                RotateClockwise();
                nextRotateTime = Time.time + rotateInterval;
            }
        }

        public void ClearLists()
        {
            SimpleRotationObjects = new List<Transform>();
            MirrorRotationObjects = new List<Transform>();
            LineRenderers = new List<LineRenderer>();
        }

        private void OnValidate()
        {
            nextRotateTime = Time.time;
        }

       
    }

    [Serializable]
    public class RoadSwapElement
    {
        public int CurrentIndex = 0;
        public SpriteRenderer SpriteRenderer;
        public SpriteSwapRule[] Rules;
        public Transform[] PinObjects;
        
        public int GetCurrentRuleIndex()
        {
            for (int i = 0; i < Rules.Length; i++)
            {
                if (SpriteRenderer.sprite == Rules[i].Sprite && SpriteRenderer.transform.localScale == Rules[i].Scale)
                {
                    return i;
                }
            }

            return -1;
        }
        
        public void Rotate(int times = 1)
        {
            int currentIndex = GetCurrentRuleIndex();
            if (currentIndex == -1) return;
            int newIndex = (currentIndex + times) % Rules.Length;
            CurrentIndex = newIndex;
            SpriteRenderer.sprite = Rules[newIndex].Sprite;
            SpriteRenderer.transform.localScale = Rules[newIndex].Scale;
            foreach (var pinObject in PinObjects)
            {
                pinObject.gameObject.SetActive(false);
            }
            PinObjects[newIndex].gameObject.SetActive(true);
        }
    }

    [Serializable]
    public class SpriteLayerSwapElement
    {
        public LineRenderer LineRenderer;
        public SpriteSwapLayerRule[] Rules;
        public int CurrentRotation = 0;

        public int GetCurrentRotation()
        {
            for(int i = 0; i < Rules.Length; i++)
            {
                if (CurrentRotation == Rules[i].RotationIndex)
                {
                    return i;
                }
            }
            return -1;
        }

        public void Rotate(int times = 1)
        {
            int currentIndex = GetCurrentRotation();
            if (currentIndex == -1) return;
            int newIndex = (currentIndex + times) % Rules.Length;
            CurrentRotation = newIndex;
            LineRenderer.sortingOrder = Rules[newIndex].LayerIndex;
        }
    }

    [Serializable]
    public class SpriteSwapRule
    {
        public Sprite Sprite;
        public Vector3 Scale;
    }

    [Serializable]
    public class SpriteSwapLayerRule
    {
        public int LayerIndex;
        public int RotationIndex;
    }
}