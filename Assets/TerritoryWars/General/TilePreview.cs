using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TerritoryWars.Tile;
using UnityEngine;
using DG.Tweening;
using TerritoryWars.DataModels.ClientEvents;
using TerritoryWars.Dojo;
using TerritoryWars.Managers.SessionComponents;
using TerritoryWars.Tools;
using TerritoryWars.UI;
using TMPro;
using UnityEngine.UI;
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
        public float FullAnimationDuration => moveDuration + 0.5f;
        [SerializeField] private Ease moveEase = Ease.OutQuint;

        private TileData currentTile;
        private Vector3 _initialPosition;
        private Vector2Int _currentBoardPosition;
        private Tween currentTween;
        private Camera _mainCamera;

        public List<Sprite> HouseSprites = new List<Sprite>();

        public RectTransform TilePointsParent;
        public CanvasGroup TilePointsParentCanvasGroup;
        public TilePointsUI CityPointsUI;
        public TilePointsUI RoadPointsUI;
        private Vector2 _startTilePointsPosition;

        private void Awake()
        {
            _mainCamera = Camera.main;
            _startTilePointsPosition = TilePointsParent.anchoredPosition;
            TilePointsAnimation();
            SetInitialPosition();
            SetupSortingLayers();

            tileGeneratorForUI.gameObject.SetActive(false);
        }
        
        public void SetActivePreview(bool active)
        {
            gameObject.SetActive(active);
            tileGeneratorForUI.gameObject.SetActive(active);
            if (active)
            {
                SetInitialPosition();
            }
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
            EventBus.Subscribe<TileSelected>(TileSelected);
            EventBus.Subscribe<TilePlaced>(ResetPosition);
            EventBus.Subscribe<ClientInput>(OnClientInput);
        }

        private void TileSelected(TileSelected tileSelected)
        {
            SetPosition(tileSelected.Position);
        }

    private void OnClientInput(ClientInput input)
        {
            if (input.Type == ClientInput.InputType.UseJoker) GenerateFFFFTile();
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
            if(currentTile == null) SetActivePreview(false);
            tileGeneratorForUI.gameObject.SetActive(true);
            if (currentTile != null)
            {
                gameObject.SetActive(true);
                tileGenerator.Generate(currentTile);
                tileGeneratorForUI.Generate(currentTile);
                
                CityPointsUI.ShowScore(currentTile.GetPoints(true), currentTile.PlayerSide);
                RoadPointsUI.ShowScore(currentTile.GetPoints(false), currentTile.PlayerSide);
                
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
            //SessionManagerOld.Instance.TileSelector.SetCurrentTile(new TileData());
            tileGenerator.Generate(new TileData());
            tileGeneratorForUI.Generate(new TileData());
            tileGenerator.tileParts.HideForestAreas();
        }

        public void SetPosition(Vector2Int currentBoardPosition, Action callback = null)
        {
            currentTween?.Kill();
            _currentBoardPosition = currentBoardPosition;
            Vector3 targetPosition = BoardManager.GetTilePosition(currentBoardPosition.x, currentBoardPosition.y);
            targetPosition.y += tilePreviewSetHeight;

            currentTween = transform
                .DOMove(targetPosition, moveDuration)
                .SetEase(moveEase).OnComplete(() =>
                {
                    callback?.Invoke();
                });
            //previewTileView.transform.DOScale(1, 0.5f).SetEase(Ease.OutQuint);
        }
        
        public void PlaceTile(TileData tileData, Action callback = null)
        {
            currentTile = tileData;
            StartCoroutine(PlaceTileCoroutine(tileData.PlayerSide, callback));
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
            SessionManager.Instance.SessionContext.CurrentTurnPlayer.EndTurnAnimation();
            currentTween = transform
                .DOMove(targetPosition, moveDuration)
                .SetEase(moveEase);
            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(moveDuration);
            sequence.AppendCallback(() =>
            {
                callback?.Invoke();
                SessionManager.Instance.ManagerContext.BoardManager.FloatingTextAnimation(currentTile);
                SessionManager.Instance.ManagerContext.BoardManager.ScoreClientPrediction(playerIndex, currentTile);
                ResetPosition();
            });
            sequence.Play();
            
            HideTilePoints();
        }

        

        public void ResetPosition(TilePlaced data = default)
        {
            currentTween?.Kill();
            // currentTween = transform
            //     .DOMove(_initialPosition, moveDuration)
            //     .SetEase(moveEase);
            tileGenerator.Generate(new TileData());
            _tileJokerAnimator.SetOffAllAnimationObjects();
            _tileJokerAnimatorPreview.SetOffAllAnimationObjects();
            
            transform.position = _initialPosition;
        }
        
        public void TilePointsAnimation()
        {
            TilePointsParent.DOKill();
            TilePointsParent.anchoredPosition = _startTilePointsPosition;
            TilePointsParentCanvasGroup.alpha = 1f;
            
            
            // position Y
            Vector2 targetPosition = _startTilePointsPosition;
            targetPosition.y += 0.12f;
            TilePointsParent
                .DOAnchorPos(targetPosition, 2f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);

        }

        public void HideTilePoints()
        {
            TilePointsParent.DOKill();
            
            float targetOpacity = 0f;
            Vector2 targetPosition = new Vector2(_startTilePointsPosition.x, -0.78f);
            TilePointsParent
                .DOAnchorPos(targetPosition, moveDuration)
                .SetEase(Ease.InOutSine);
            
            TilePointsParentCanvasGroup.DOFade(targetOpacity, moveDuration)
                .OnComplete(() =>
                {
                    CityPointsUI.SetActive(false);
                    RoadPointsUI.SetActive(false);
                    TilePointsAnimation();
                });
            
            
        }

        private void OnDestroy()
        {
            
            currentTween?.Kill();

            EventBus.Unsubscribe<TileSelected>(TileSelected);
            EventBus.Unsubscribe<ClientInput>(OnClientInput);
            EventBus.Unsubscribe<TilePlaced>(ResetPosition);
        }

        [Serializable]
        public struct TilePointsUI
        {
            public GameObject PointsParent;
            public TextMeshProUGUI PointsText;
            public Image PointsImage;
            public Sprite[] SpritePerSide;

            public void SetActive(bool active)
            {
                PointsParent.SetActive(active);
            }

            public void ShowScore(int score, int playerSide)
            {
                SetActive(score > 0);
                if (playerSide < 0)
                {
                    SetActive(false); 
                    return;
                }
                PointsText.text = score.ToString();
                PointsImage.sprite = SpritePerSide[playerSide];
            }
        }
    }
    
    
}
