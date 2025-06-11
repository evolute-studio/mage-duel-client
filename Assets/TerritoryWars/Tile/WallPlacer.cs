using System.Collections.Generic;
using TerritoryWars.General;
using UnityEngine;

namespace TerritoryWars.Tile
{
    public class WallPlacer : MonoBehaviour
    {
        public GameObject WallSegmentPrefab => PrefabsManager.Instance.WallSegmentPrefab;
        public float wallYOffset = 0.035f;
        public float sidesOffsetX = 0f;
        
        public Transform[] pillars;
        public List<GameObject> wallSegments = new List<GameObject>();
        [Tooltip("Segment indexes to be skipped (index is the number of the initial segment column)")]
        public int[] skipSegments;
        
        [ContextMenu("PlaceStoneWall")]
        public void PlaceStoneWall()
        {
            PlaceWall(true);
        }
        
        [ContextMenu("PlaceWoodWall")]
        public void PlaceWoodWall()
        {
            PlaceWall(false);
        }
        
        
        
        public void PlaceWall(bool isContested)
        {
            return;
            if (!enabled) return;
            SetPillars();
            
            if (pillars == null || pillars.Length < 2) return;

            for (int i = 0; i < pillars.Length - 1; i++)
            {
                // Перевіряємо чи потрібно пропустити цей сегмент
                if (skipSegments != null && System.Array.IndexOf(skipSegments, i) != -1)
                {
                    continue;
                }
                
                Sprite wallSprite = PrefabsManager.Instance.TileAssetsObject.GetWall(isContested);
                
                Vector3 start = pillars[i].position;
                Vector3 end = pillars[i + 1].position;

                GameObject wallSegment = GetWallObject(i);
                SpriteSkew skew = wallSegment.GetComponent<SpriteSkew>();
               
                SpriteRenderer segmentRenderer = wallSegment.GetComponent<SpriteRenderer>();
                segmentRenderer.sprite = wallSprite;
                segmentRenderer.spriteSortPoint = SpriteSortPoint.Pivot;
                segmentRenderer.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
                
                skew.SetPositions(start, end, wallYOffset, sidesOffsetX);
                
                if (!wallSegments.Contains(wallSegment))
                {
                    wallSegments.Add(wallSegment);
                }
                
            }
            
            foreach (Transform pillar in pillars)
            {
                Sprite pillarSprite = PrefabsManager.Instance.TileAssetsObject.GetPillar(isContested);
                SpriteRenderer pillarRenderer = pillar.GetComponent<SpriteRenderer>();
                pillarRenderer.sprite = pillarSprite;
                pillarRenderer.spriteSortPoint = SpriteSortPoint.Pivot;
                // if (!pillar.TryGetComponent<SpriteMask>(out var mask))
                // {
                //     mask = pillar.gameObject.AddComponent<SpriteMask>();
                // }
                // mask.sprite = pillarSprite;
                // mask.isCustomRangeActive = true;
                // mask.frontSortingOrder = 15;
            }
        }

        private void SetPillars()
        {
            return;
            // get all children of this object with name containing "Pillar"
            Transform[] allPillars = GetComponentsInChildren<Transform>();
            List<Transform> filteredPillars = new List<Transform>();
            foreach (Transform pillar in allPillars)
            {
                if (pillar.name.Contains("Pillar"))
                {
                    filteredPillars.Add(pillar);
                }
            }
            // set pillars to filteredPillars
            pillars = filteredPillars.ToArray();
        }
        
        public Transform[] GetPillars()
        {
            return pillars;
        }
        
        public List<GameObject> GetWallSegments()
        {
            return wallSegments;
        }

        public GameObject GetWallObject(int index)
        {
            if (index < 0 || index >= wallSegments.Count)
            {
                GameObject defaultSegment = Instantiate(WallSegmentPrefab, transform);
                return defaultSegment;
            }
            GameObject wallSegment = wallSegments[index];
            if (wallSegment == null)
            {
                wallSegment = Instantiate(WallSegmentPrefab, transform);
                wallSegments[index] = wallSegment;
            }
            return wallSegment;
        }
        
        private void DestroyWallSegments()
        {
            foreach (GameObject segment in wallSegments)
            {
                Destroy(segment);
            }
            wallSegments.Clear();
        }
    }
}