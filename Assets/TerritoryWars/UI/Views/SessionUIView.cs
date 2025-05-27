using System;
using TerritoryWars.General;
using TerritoryWars.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SessionUIView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button endTurnButton;
    [SerializeField] private Button skipTurnButton;
    [SerializeField] private Button rotateTileButton;
    [SerializeField] private Image currentTilePreview;
    [SerializeField] private Button jokerButton;
    [SerializeField] private Button deckButton;
    [SerializeField] private GameObject jokerModeIndicator;
    [SerializeField] private Sprite[] _toggleMods;
    [SerializeField] private Image _toggleSpriteRenderer;
    [SerializeField] private CanvasGroup _deckContainerCanvasGroup;
    [SerializeField] private ResultPopUpUI _resultPopUpUI;
    [SerializeField] private Button SaveSnapshotButton;
    [SerializeField] private TextMeshProUGUI SaveSnapshotText;
    [SerializeField] private ArrowAnimations arrowAnimations;
    
    public static event Action OnJokerButtonClickedEvent;
    
    [Header("Tile Preview")]
    [SerializeField] private TilePreview tilePreview;
    [SerializeField] private TileJokerAnimator tilePreviewUITileJokerAnimator;
    
    private SessionManagerOld _sessionManagerOld;
    private DeckManager deckManager;
    
}
