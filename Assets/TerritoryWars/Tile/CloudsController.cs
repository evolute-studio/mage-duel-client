using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TerritoryWars.General;
using TerritoryWars.Tools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TerritoryWars.Tile
{
    public class CloudsController : MonoBehaviour
    {
        public float spawnTime = 1.2f;
        private List<Vector3> mountains = new List<Vector3>();
        private Coroutine coroutine;
        private bool _spawnClouds = false;
        private float timer = 0f;
        

        public void Update()
        {
            timer += Time.deltaTime;
            if (_spawnClouds && timer >= spawnTime)
            {
                SpawnClouds();
                timer = 0f;
            }
        }
        
        public void SpawnClouds()
        {
            int randomGameObject = Random.Range(0, mountains.Count);
            Vector3 position = mountains[randomGameObject];
            int randomClouds = Random.Range(0, PrefabsManager.Instance.TileAssetsObject.Clouds.Length);

            GameObject cloud = MonoBehaviour.Instantiate(PrefabsManager.Instance.CloudPrefab, position + new Vector3(0.539f,0.23f,0f)
                , Quaternion.identity);

            SpriteAnimator cloudSpriteAnimator = cloud.GetComponent<SpriteAnimator>();
            
            cloudSpriteAnimator.OnAnimationEnd += () =>
            {
                cloud.GetComponent<SpriteRenderer>().DOFade(0f, 0.5f).OnComplete(() =>
                {
                    Destroy(cloud);
                });
            };
            
            cloudSpriteAnimator.Play(
                PrefabsManager.Instance.TileAssetsObject.Clouds[randomClouds].Sprites);
        }

        public void StopSpawnClouds()
        {
            _spawnClouds = false;
        }
        
        public void SetMountains(List<GameObject> mountains)
        {
            for(int i = 0; i < mountains.Count; i++)
            {
                this.mountains.Add(mountains[i].transform.position);
            }
            _spawnClouds = true;
        }
    }
}