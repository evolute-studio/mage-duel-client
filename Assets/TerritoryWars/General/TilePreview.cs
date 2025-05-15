using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TerritoryWars.Tile;
using UnityEngine;
using DG.Tweening;
using TerritoryWars.Dojo;
using TerritoryWars.Tools;
using TerritoryWars.UI;
using Random = UnityEngine.Random;

namespace TerritoryWars.General
{
    public class TilePreview : MonoBehaviour
    {
        [SerializeField] private TileGenerator tileGenerator;
        [SerializeField] private float tilePreviewSetHeight = 0.5f;
        public PolygonCollider2D PreviewPolygonCollider2D;
        public TileJokerAnimator _tileJokerAnimator;
        public TileJokerAnimator _tileJokerAnimatorPreview;
        
        [SerializeField] private TileGenerator tileGeneratorForUI;
        [SerializeField] private LayerMask previewLayerMask;

        [Header("Preview Position")] [SerializeField]
        private Vector2 screenOffset = new Vector2(100f, 100f);

        [Header("Animation Settings")] [SerializeField]
        private float moveDuration = 0.3f;

        [SerializeField] private Ease moveEase = Ease.OutQuint;

        private TileData currentTile;
        private Vector3 _initialPosition;
        private Vector2Int _currentBoardPosition;
        private Tween currentTween;
        private Camera _mainCamera;

        public List<Sprite> HouseSprites = new List<Sprite>();

        private void Awake()
        {
            _mainCamera = Camera.main;
            SetInitialPosition();
            SetupSortingLayers();
            
            tileGeneratorForUI.gameObject.SetActive(false);
        }

        private void SetInitialPosition()
        {
            
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);

            
            Vector2 screenPosition = new Vector2(
                screenSize.x - screenOffset.x,
                screenOffset.y
            );

            
            _initialPosition = _mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10));
            _initialPosition.z = 0;

            
            transform.position = _initialPosition;
        }

        public void Start()
        {
            SessionManager.Instance.TileSelector.OnTileSelected += SetPosition;
            SessionManager.Instance.TileSelector.OnTilePlaced.AddListener(ResetPosition);
            GameUI.OnJokerButtonClickedEvent += GenerateFFFFTile;
            
        }

        private void SetupSortingLayers()
        {
            SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer spriteRenderer in spriteRenderers)
            {
                spriteRenderer.sortingLayerName = "Preview";
            }

            LineRenderer[] lineRenderers = GetComponentsInChildren<LineRenderer>();
            foreach (LineRenderer lineRenderer in lineRenderers)
            {
                lineRenderer.sortingLayerName = "Preview";
            }
        }

        private void Update()
        {
            
            if (transform.position == _initialPosition)
            {
                SetInitialPosition();
            }
        }

       

        public void UpdatePreview(TileData currentTile)
        {
            tileGeneratorForUI.gameObject.SetActive(true);
            if (currentTile != null)
            {
                gameObject.SetActive(true);
                tileGenerator.Generate(currentTile);
                tileGeneratorForUI.Generate(currentTile);
                if (tileGenerator.CurrentTileGO != null)
                {
                    TileParts tileParts = tileGenerator.CurrentTileGO.GetComponent<TileParts>();
                    tileParts.HideForestAreas();
                    
                    if (tileParts.TileTerritoryFiller != null)
                    {
                        Transform territoryPlacer = tileParts
                            .TileTerritoryFiller.transform;
                        territoryPlacer.GetComponentInChildren<SpriteMask>().frontSortingLayerID
                            = SortingLayer.NameToID("Preview");
                    }
                    

                    HouseSprites.Clear();
                    List<SpriteRenderer> houseRenderers = new List<SpriteRenderer>();
                    
                    foreach (var house in tileParts.Houses)
                    {
                        houseRenderers.Add(house.HouseSpriteRenderer);
                    }
                    
                    for (int i = 0; i < houseRenderers.Count; i++)
                    {
                        houseRenderers[i].sortingLayerName = "Preview";
                        if (currentTile.HouseSprites.Count > i && currentTile.HouseSprites[i] != null)
                            houseRenderers[i].sprite = currentTile.HouseSprites[i];
                        HouseSprites.Add(houseRenderers[i].sprite);
                    }
                    foreach (SpriteRenderer houseRenderer in houseRenderers)
                    {
                        houseRenderer.sortingLayerName = "Preview";
                        HouseSprites.Add(houseRenderer.sprite);
                    }
                    
                }

                if (tileGeneratorForUI.CurrentTileGO != null)
                {
                    void SetLayerRecursively(Transform root)
                    {
                        root.gameObject.layer = LayerMask.NameToLayer("TilePreview");
                        foreach(Transform child in root)
                        {
                            SetLayerRecursively(child);
                        }
                    }
                    
                    SetLayerRecursively(tileGeneratorForUI.CurrentTileGO.transform);
                    
                }
            }
            else
            {
                gameObject.SetActive(false);
            }

            SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer spriteRenderer in spriteRenderers)
            {
                spriteRenderer.sortingLayerName = "Preview";
            }

            LineRenderer[] lineRenderers = GetComponentsInChildren<LineRenderer>();
            foreach (LineRenderer lineRenderer in lineRenderers)
            {
                lineRenderer.sortingLayerName = "Preview";
            }
        }

        private void GenerateFFFFTile()
        {
            SessionManager.Instance.TileSelector.SetCurrentTile(new TileData("FFFF"));
            tileGenerator.Generate(new TileData("FFFF"));
            tileGeneratorForUI.Generate(new TileData("FFFF"));
            tileGenerator.tileParts.HideForestAreas();
        }

        public void SetPosition(int x, int y)
        {
            currentTween?.Kill();
            _currentBoardPosition = new Vector2Int(x, y);

            Vector3 targetPosition = Board.GetTilePosition(x, y);
            targetPosition.y += tilePreviewSetHeight;

            currentTween = transform
                .DOMove(targetPosition, moveDuration)
                .SetEase(moveEase);
            //previewTileView.transform.DOScale(1, 0.5f).SetEase(Ease.OutQuint);
        }
        
        public void PlaceTile(int playerIndex, TileData tileData, Action callback = null)
        {
            currentTile = tileData;
            StartCoroutine(PlaceTileCoroutine(playerIndex, callback));
        }
        

        private IEnumerator PlaceTileCoroutine(int playerIndex, Action callback = null)
        {
            if (!gameObject.activeSelf) yield break;
            // shake animation Y
            transform.DOShakePosition(0.5f, 0.1f, 18, 45, false, true);

            yield return new WaitForSeconds(0.5f);
            
            SpriteRenderer[] grounds =
                transform.Find("Ground").GetComponentsInChildren<SpriteRenderer>();

            foreach (SpriteRenderer ground in grounds)
            {
                ground.sortingLayerName = "Default";
            }
            transform.Find("Grass").GetComponent<SpriteRenderer>().sortingLayerName = "Default";
            
            if (tileGenerator.CurrentTileGO != null)
            {
                TileParts tileParts = tileGenerator.CurrentTileGO.GetComponent<TileParts>();

                foreach (var spriteRenderer in tileParts.Enviroment.GetComponentsInChildren<SpriteRenderer>())
                {
                    spriteRenderer.sortingLayerName = "Default";
                }
                
                if (tileParts.WallPlacer != null)
                {
                    var pillars = tileParts.WallPlacer.GetPillars();
                    var segments = tileParts.WallPlacer.GetWallSegments();
                        
                    foreach (var pillar in pillars)
                    { 
                        pillar.GetComponent<SpriteRenderer>().sortingLayerName = "Default";
                    }

                    foreach (var segment in segments)
                    {
                        if (segment == null) continue;
                        segment.GetComponent<SpriteRenderer>().sortingLayerName = "Default";
                    }
                }
            
                if(tileParts.RoadRenderers != null)
                    foreach (var road in tileParts.RoadRenderers)
                    {
                        if(road != null) road.sortingLayerName = "Default";
                    }
                
                if (tileParts.TileTerritoryFiller != null)
                {
                    Transform territoryPlacer = tileParts
                        .TileTerritoryFiller.transform;
                    
                    territoryPlacer.GetComponentInChildren<SpriteMask>().frontSortingLayerID
                        = SortingLayer.NameToID("Default");
                    
                    territoryPlacer.GetComponentInChildren<SpriteRenderer>().sortingLayerName = "Default";
                }

                List<SpriteRenderer> groundRenderers = new List<SpriteRenderer>();
                groundRenderers.Add(tileGenerator.CurrentTileGO.transform.Find("Grass").GetComponent<SpriteRenderer>());
                groundRenderers.AddRange(tileGenerator.CurrentTileGO.transform.Find("Ground")
                    .GetComponentsInChildren<SpriteRenderer>());
                groundRenderers.Add(tileGenerator.tileParts.HangingGrass);

                foreach (var renderers in groundRenderers)
                {
                    renderers.sortingLayerName = "Default";
                }

                HouseSprites.Clear();
                List<SpriteRenderer> houseRenderers = new List<SpriteRenderer>();
                foreach (var house in tileParts.Houses)
                {
                    houseRenderers.Add(house.HouseSpriteRenderer);
                }
                foreach (SpriteRenderer houseRenderer in houseRenderers)
                {
                    houseRenderer.sortingLayerName = "Default";
                    HouseSprites.Add(houseRenderer.sprite);
                }
            }
            
            currentTween?.Kill();
            Vector3 currentPosition = transform.position;
            Vector3 targetPosition = currentPosition;
            targetPosition.y -= tilePreviewSetHeight;
            SessionManager.Instance.CurrentTurnPlayer.EndTurn();
            currentTween = transform
                .DOMove(targetPosition, moveDuration)
                .SetEase(moveEase)
                .OnComplete(() =>
                {
                    callback?.Invoke();
                    SessionManager.Instance.Board.FloatingTextAnimation(_currentBoardPosition);
                    SessionManager.Instance.Board.ScoreClientPrediction(playerIndex, currentTile);
                });
        }

        

        public void ResetPosition()
        {
            currentTween?.Kill();
            // currentTween = transform
            //     .DOMove(_initialPosition, moveDuration)
            //     .SetEase(moveEase);
            tileGenerator.Generate(new TileData("FFFF"));
            _tileJokerAnimator.SetOffAllAnimationObjects();
            _tileJokerAnimatorPreview.SetOffAllAnimationObjects();
            
            transform.position = _initialPosition;
        }

        private void OnDestroy()
        {
            
            currentTween?.Kill();
            
            SessionManager.Instance.TileSelector.OnTileSelected -= SetPosition;
            GameUI.OnJokerButtonClickedEvent -= GenerateFFFFTile;
        }
    }
}