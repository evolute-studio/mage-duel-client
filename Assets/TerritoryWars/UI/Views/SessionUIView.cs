using System;
using DG.Tweening;
using TerritoryWars.General;
using TerritoryWars.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SessionUIView : MonoBehaviour
{
    [Header("Player Info")]
    
    [SerializeField] private TextMeshProUGUI[] cityScoreTextPlayers;
    [SerializeField] private TextMeshProUGUI[] tileScoreTextPlayers;
    [SerializeField] private TextMeshProUGUI[] playerScores;
    [SerializeField] private TextMeshProUGUI[] playerNames;
    [SerializeField] private Image[] playerAvatars;
    [SerializeField] private Image[] bluePlayerJokers;
    [SerializeField] private Image[] redPlayerJokers;
    
    [Header("References")]
    [SerializeField] public Button endTurnButton;
    [SerializeField] public Button skipTurnButton;
    [SerializeField] public Button rotateTileButton;
    [SerializeField] private Image currentTilePreview;
    [SerializeField] public Button jokerButton;
    [SerializeField] public Button deckButton;
    [SerializeField] private GameObject jokerModeIndicator;
    [SerializeField] private Sprite[] _toggleMods;
    [SerializeField] private Image _toggleSpriteRenderer;
    [SerializeField] private CanvasGroup _deckContainerCanvasGroup;
    [SerializeField] private ResultPopUpUI _resultPopUpUI;
    [SerializeField] public Button SaveSnapshotButton;
    [SerializeField] private TextMeshProUGUI SaveSnapshotText;
    [SerializeField] private ArrowAnimations arrowAnimations;
    [SerializeField] private TextMeshProUGUI JokerCountText;
    
    public TextMeshProUGUI DeckCountText; 
    public Button CancelGameButton;
    
    public static event Action OnJokerButtonClickedEvent;
    
    [Header("Tile Preview")]
    [SerializeField] private TilePreview tilePreview;
    [SerializeField] private TileJokerAnimator tilePreviewUITileJokerAnimator;
    
    public void UpdateDeckCountDisplay(int count)
    {
        DeckCountText.text = count.ToString();
    }

    public void UpdateJokersCountDisplay(int blueJokers, int redJokers)
    {
        for (int i = 0; i < bluePlayerJokers.Length; i++)
        {
            bluePlayerJokers[i].GetComponent<Button>().interactable = i < blueJokers;
            if(bluePlayerJokers[i].TryGetComponent<CursorOnHover>(out var hover))
            {
                hover.enabled = i < blueJokers;
            }
        }
        
        for (int i = 0; i < redPlayerJokers.Length; i++)
        {
            redPlayerJokers[i].GetComponent<Button>().interactable = i < redJokers;
            if (redPlayerJokers[i].TryGetComponent<CursorOnHover>(out var hover))
            {
                hover.enabled = i < redJokers;
            }
        }
    }
    
    public void UpdateJokerCountTextDisplay(int count)
    {
        JokerCountText.text = count.ToString();
    }
    
    public void UpdatePlayerNamesDisplay(string[] names)
    {
        for (int i = 0; i < playerNames.Length; i++)
        {
            playerNames[i].text = names[i];
        }
    }
    
    public void UpdateCityScoreDisplay(int[] scores)
    {
        for (int i = 0; i < cityScoreTextPlayers.Length; i++)
        {
            cityScoreTextPlayers[i].text = scores[i].ToString();
        }
    }
    
    public void UpdateTileScoreDisplay(int[] scores)
    {
        for (int i = 0; i < tileScoreTextPlayers.Length; i++)
        {
            tileScoreTextPlayers[i].text = scores[i].ToString();
        }
    }

    public void UpdateScoresDisplay(int[] scores)
    {
        for (int i = 0; i < playerScores.Length; i++)
        {
            playerScores[i].text = scores[i].ToString();
        }
    }
    
    public void UpdateAvatarsDisplay(Sprite[] avatars)
    {
        for (int i = 0; i < avatars.Length; i++)
        {
            playerAvatars[i].sprite = avatars[i];
        }
    }

    public void UpdateTilePreview()
    {
        if (tilePreview != null)
        {
            tilePreview.UpdatePreview(SessionManager.Instance.TileSelector.CurrentTile);
        }
        
        if(jokerButton != null)
        {
            jokerButton.interactable = SessionManager.Instance.JokerManager.CanUseJoker();
        }
    }
    
    public void UpdateJokerButtonInteractable(bool isInteractable)
    {
        jokerButton.interactable = isInteractable;
    }

    public void UpdateDeckContainerActive(bool active)
    {
        if (active)
        {
            _deckContainerCanvasGroup.alpha = 0.5f;
            _deckContainerCanvasGroup.DOFade(1, 0.5f);
            jokerButton.interactable = true;
            deckButton.interactable = true;
        }
        else
        {
            _deckContainerCanvasGroup.alpha = 1;
            _deckContainerCanvasGroup.DOFade(0.5f, 0.5f);
            jokerButton.interactable = false;
            deckButton.interactable = false;
        }
    }

    public void PlayArrowAnimation()
    {
        arrowAnimations.PlayRotationAnimation();
    }
    
    public void SetJokerModeAnimation(bool active)
    {
        if (active)
        {
            _toggleSpriteRenderer.sprite = _toggleMods[1];
            tilePreview._tileJokerAnimator.ShowIdleJokerAnimation();
            tilePreviewUITileJokerAnimator.ShowIdleJokerAnimation();
        }
        else
        {
            _toggleSpriteRenderer.sprite = _toggleMods[0];
            tilePreview._tileJokerAnimator.StopIdleJokerAnimation();
            tilePreviewUITileJokerAnimator.StopIdleJokerAnimation();
        }
    }
    
    public void SetRotateButtonActive(bool active)
    {
        if (rotateTileButton != null)
        {
            rotateTileButton.gameObject.SetActive(active);
            arrowAnimations.gameObject.SetActive(active);
            arrowAnimations.SetActiveArrow(active);
        }
    }
    
    public void SetSkipTurnButtonActive(bool active)
    {
        if (skipTurnButton != null)
        {
            skipTurnButton.gameObject.SetActive(active);
        }
    }
    
    public void SetEndTurnButtonActive(bool active)
    {
        if (endTurnButton != null)
        {
            endTurnButton.gameObject.SetActive(active);
        }
    }

    public void OnSnapshotButtonClicked()
    {
        SaveSnapshotText.gameObject.SetActive(true);
        SaveSnapshotText.DOFade(1, 0.2f);
        DOVirtual.DelayedCall(3, () =>
        {
               
            SaveSnapshotText.DOFade(0, 0.5f).OnComplete(() =>
            {
                SaveSnapshotText.gameObject.SetActive(false);
            });
        });
    }

    public void OnEndTurnClicked()
    {
        SetEndTurnButtonActive(false);
        UpdateDeckContainerActive(false);
    }
    
}
