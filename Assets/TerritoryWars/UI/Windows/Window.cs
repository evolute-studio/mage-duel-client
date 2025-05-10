using System.Collections.Generic;
using DG.Tweening;
using TerritoryWars.Tools;
using UnityEngine;

namespace TerritoryWars.UI.Windows
{
    public class Window : MonoBehaviour
    {
        [Header("Settings", order = 2)] 
        [SerializeField] protected bool withScrollBar = true;
        [SerializeField] protected List<GameObject> ObjectsToHide;
        
        
        [Header("General References", order = 1)]
        [SerializeField] protected GameObject windowGO;
        [SerializeField] protected GameObject backgroundPlaceholderGO;
        [SerializeField] protected Transform listItemParent;
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected GameObject backgroundCloseButtonGO;
        [SerializeField] protected GameObject scrollBar;
        
        [Header("Other", order = 3)]
        [SerializeField] protected GameObject listItemPrefab;
        
        protected List<object> listItems = new List<object>();

        public virtual void Initialize()
        {
            scrollBar.SetActive(withScrollBar);
        }
        
        protected virtual T CreateListItem<T>() where T : class
        {
            GameObject listItem = Instantiate(listItemPrefab, listItemParent);
            T matchListItem = listItem.GetComponent<T>();
            listItems.Add(matchListItem);
            return matchListItem;
        }
        
        protected virtual void ClearAllListItems()
        {
            foreach (var item in listItems)
            {
                if (item is MonoBehaviour monoBehaviour)
                {
                    if (monoBehaviour.gameObject != null)
                    {
                        Destroy(monoBehaviour.gameObject);
                    }
                }
            }
            listItems.Clear();
        }
        
        protected void SetBackgroundPlaceholder(bool isActive)
        {
            backgroundPlaceholderGO.SetActive(isActive);
        }

        public void SetActivePanel(bool isActive)
        {
            if (isActive)
                PanelActiveTrue();
            else
                PanelActiveFalse();
        }

        protected virtual void PanelActiveTrue()
        {
            ClearAllListItems();

            windowGO.SetActive(true);

            windowGO.transform.localScale = Vector3.one * 0.75f;
            windowGO.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);

            canvasGroup.alpha = 0;
            canvasGroup.DOFade(1, 0.25f).SetEase(Ease.OutBack);

            foreach (var objectToHide in ObjectsToHide)
            {
                if (!objectToHide.TryGetComponent(out CanvasGroup objectCanvasGroup))
                {
                    objectCanvasGroup = objectToHide.AddComponent<CanvasGroup>();
                }
                objectCanvasGroup.alpha = 1;
                objectCanvasGroup.DOFade(0, 0.25f).SetEase(Ease.OutBack);
            }
        }

        protected virtual void PanelActiveFalse()
        {
            windowGO.transform.localScale = Vector3.one;
            windowGO.transform.DOScale(Vector3.one * 0.75f, 0.25f).SetEase(Ease.OutBack);

            canvasGroup.alpha = 1;
            canvasGroup.DOFade(0, 0.25f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                windowGO.SetActive(false);
                CursorManager.Instance.SetCursor("default");
                ClearAllListItems();
            });
            
            foreach (var objectToHide in ObjectsToHide)
            {
                if (!objectToHide.TryGetComponent(out CanvasGroup objectCanvasGroup))
                {
                    objectCanvasGroup = objectToHide.AddComponent<CanvasGroup>();
                }
                objectCanvasGroup.alpha = 0;
                objectCanvasGroup.DOFade(1, 0.25f).SetEase(Ease.OutBack);
            }
        }
        
    }
}