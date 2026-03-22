using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("References")]
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Settings")]
    [SerializeField] private float slideDuration = 0.4f;

    private bool _isTransitioning = false;
    private float _screenWidth;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);

        _screenWidth = Screen.width;
    }

    public void GoToPreviousScene()
    {
        Debug.Log("GoToNextScene called");
        if (_isTransitioning) return;
        int current = SceneManager.GetActiveScene().buildIndex;
        if (current > 0)
            StartCoroutine(SlideTransition(current - 1, goingLeft: true));
    }

    public void GoToNextScene()
    {
        Debug.Log("GoToNextScene called");
        if (_isTransitioning) return;
        int current = SceneManager.GetActiveScene().buildIndex;
        if (current < SceneManager.sceneCountInBuildSettings - 1)
            StartCoroutine(SlideTransition(current + 1, goingLeft: false));
    }

    private IEnumerator SlideTransition(int targetIndex, bool goingLeft)
    {
        _isTransitioning = true;

        // Step 1: Slide panel IN until screen is fully black
        float startX = goingLeft ? _screenWidth * 1.5f : -_screenWidth * 1.5f;
        yield return StartCoroutine(SlidePanel(startX, 0f));

        // Step 2: Screen is now fully black — start loading
        AsyncOperation load = SceneManager.LoadSceneAsync(targetIndex);
        load.allowSceneActivation = false;

        // Step 3: Wait until fully preloaded
        while (load.progress < 0.9f)
            yield return null;

        // Step 4: Activate scene
        load.allowSceneActivation = true;
        while (!load.isDone)
            yield return null;

        // Step 5: Slide panel OUT to reveal new scene
        float endX = goingLeft ? -_screenWidth * 1.5f : _screenWidth * 1.5f;
        yield return StartCoroutine(SlidePanel(0f, endX));

        _isTransitioning = false;
    }

    private IEnumerator SlidePanel(float fromX, float toX)
    {
        canvasGroup.alpha = 1f;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / slideDuration);
            panelRect.anchoredPosition = new Vector2(Mathf.Lerp(fromX, toX, t), 0f);
            yield return null;
        }

        panelRect.anchoredPosition = new Vector2(toX, 0f);

        if (Mathf.Abs(toX) >= _screenWidth)
            canvasGroup.alpha = 0f;
    }
}