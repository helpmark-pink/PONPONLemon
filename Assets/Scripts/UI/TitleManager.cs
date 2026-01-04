using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using PONPONLemon.Core;

namespace PONPONLemon.UI
{
    public class TitleManager : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button howToPlayButton;
        [SerializeField] private Button quitButton;
        
        [Header("Logo Animation")]
        [SerializeField] private RectTransform logoTransform;
        [SerializeField] private float bounceScale = 1.05f;
        [SerializeField] private float bounceSpeed = 2f;
        
        private void Start()
        {
            SetupButtons();
            StartCoroutine(LogoAnimation());
        }
        
        private void SetupButtons()
        {
            if (startButton != null)
                startButton.onClick.AddListener(OnStartClicked);
            if (howToPlayButton != null)
                howToPlayButton.onClick.AddListener(OnHowToPlayClicked);
            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);
        }
        
        private IEnumerator LogoAnimation()
        {
            if (logoTransform == null) yield break;
            
            while (true)
            {
                float elapsed = 0;
                float duration = 1f / bounceSpeed;
                
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;
                    float scale = Mathf.Lerp(1f, bounceScale, Mathf.Sin(t * Mathf.PI * 0.5f));
                    logoTransform.localScale = Vector3.one * scale;
                    yield return null;
                }
                
                elapsed = 0;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;
                    float scale = Mathf.Lerp(bounceScale, 1f, Mathf.Sin(t * Mathf.PI * 0.5f));
                    logoTransform.localScale = Vector3.one * scale;
                    yield return null;
                }
            }
        }
        
        private void OnStartClicked()
        {
            StartCoroutine(ButtonPressAnimation(startButton, () =>
                SceneManager.LoadScene(GameConstants.SCENE_GAME)));
        }
        
        private void OnHowToPlayClicked()
        {
            StartCoroutine(ButtonPressAnimation(howToPlayButton, () =>
                SceneManager.LoadScene(GameConstants.SCENE_HOWTOPLAY)));
        }
        
        private void OnQuitClicked()
        {
            StartCoroutine(ButtonPressAnimation(quitButton, () =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }));
        }
        
        private IEnumerator ButtonPressAnimation(Button button, System.Action onComplete)
        {
            if (button == null)
            {
                onComplete?.Invoke();
                yield break;
            }
            
            Transform t = button.transform;
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
            
            onComplete?.Invoke();
        }
    }
}
