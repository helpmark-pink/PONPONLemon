using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using PONPONLemon.Core;

namespace PONPONLemon.UI
{
    public class HowToPlayManager : MonoBehaviour
    {
        [Header("Pages")]
        [SerializeField] private List<GameObject> pages;
        private int currentPage = 0;
        
        [Header("Navigation Buttons")]
        [SerializeField] private Button prevButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button backButton;
        
        [Header("Page Dots")]
        [SerializeField] private List<Image> pageDots;
        [SerializeField] private Sprite dotActiveSprite;
        [SerializeField] private Sprite dotInactiveSprite;
        
        [Header("Animation")]
        [SerializeField] private float slideSpeed = 0.3f;
        
        private bool isAnimating = false;
        private Vector2 touchStartPos;
        private float swipeThreshold = 100f;
        
        private void Start()
        {
            SetupButtons();
            ShowPage(0, false);
        }
        
        private void SetupButtons()
        {
            if (prevButton != null)
                prevButton.onClick.AddListener(OnPrevClicked);
            if (nextButton != null)
                nextButton.onClick.AddListener(OnNextClicked);
            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);
        }
        
        private void ShowPage(int pageIndex, bool animate = true)
        {
            if (pages == null || pageIndex < 0 || pageIndex >= pages.Count) return;
            if (isAnimating) return;
            
            int oldPage = currentPage;
            currentPage = pageIndex;
            
            if (animate && oldPage != currentPage)
            {
                StartCoroutine(AnimatePageTransition(oldPage, currentPage));
            }
            else
            {
                for (int i = 0; i < pages.Count; i++)
                {
                    if (pages[i] != null)
                        pages[i].SetActive(i == currentPage);
                }
            }
            
            UpdateNavigationButtons();
            UpdatePageDots();
        }
        
        private IEnumerator AnimatePageTransition(int fromPage, int toPage)
        {
            isAnimating = true;
            
            float direction = toPage > fromPage ? -1 : 1;
            
            GameObject oldPageObj = pages[fromPage];
            RectTransform oldRect = oldPageObj.GetComponent<RectTransform>();
            Vector2 oldStartPos = oldRect.anchoredPosition;
            Vector2 oldTargetPos = new Vector2(direction * 1200, 0);
            
            GameObject newPageObj = pages[toPage];
            newPageObj.SetActive(true);
            RectTransform newRect = newPageObj.GetComponent<RectTransform>();
            Vector2 newStartPos = new Vector2(-direction * 1200, 0);
            newRect.anchoredPosition = newStartPos;
            
            float elapsed = 0;
            while (elapsed < slideSpeed)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / slideSpeed;
                float easeT = 1 - Mathf.Pow(1 - t, 2);
                
                oldRect.anchoredPosition = Vector2.Lerp(oldStartPos, oldTargetPos, easeT);
                newRect.anchoredPosition = Vector2.Lerp(newStartPos, Vector2.zero, easeT);
                
                yield return null;
            }
            
            oldPageObj.SetActive(false);
            oldRect.anchoredPosition = Vector2.zero;
            newRect.anchoredPosition = Vector2.zero;
            
            isAnimating = false;
        }
        
        private void UpdateNavigationButtons()
        {
            if (prevButton != null)
                prevButton.gameObject.SetActive(currentPage > 0);
            if (nextButton != null)
                nextButton.gameObject.SetActive(pages != null && currentPage < pages.Count - 1);
        }
        
        private void UpdatePageDots()
        {
            if (pageDots == null) return;
            
            for (int i = 0; i < pageDots.Count; i++)
            {
                if (pageDots[i] != null)
                {
                    if (i == currentPage)
                    {
                        if (dotActiveSprite != null)
                            pageDots[i].sprite = dotActiveSprite;
                        pageDots[i].color = Color.white;
                    }
                    else
                    {
                        if (dotInactiveSprite != null)
                            pageDots[i].sprite = dotInactiveSprite;
                        pageDots[i].color = new Color(0.7f, 0.7f, 0.7f);
                    }
                }
            }
        }
        
        private void OnPrevClicked()
        {
            if (currentPage > 0)
                ShowPage(currentPage - 1);
        }
        
        private void OnNextClicked()
        {
            if (pages != null && currentPage < pages.Count - 1)
                ShowPage(currentPage + 1);
        }
        
        private void OnBackClicked()
        {
            StartCoroutine(ButtonPressAndLoad());
        }
        
        private IEnumerator ButtonPressAndLoad()
        {
            if (backButton != null)
            {
                Transform t = backButton.transform;
                float elapsed = 0;
                float duration = 0.1f;
                
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    t.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.9f, elapsed / duration);
                    yield return null;
                }
                
                elapsed = 0;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    t.localScale = Vector3.Lerp(Vector3.one * 0.9f, Vector3.one, elapsed / duration);
                    yield return null;
                }
            }
            
            SceneManager.LoadScene(GameConstants.SCENE_TITLE);
        }
        
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                touchStartPos = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                Vector2 touchEndPos = Input.mousePosition;
                float swipeDistance = touchEndPos.x - touchStartPos.x;
                
                if (Mathf.Abs(swipeDistance) > swipeThreshold)
                {
                    if (swipeDistance > 0)
                        OnPrevClicked();
                    else
                        OnNextClicked();
                }
            }
        }
    }
}
