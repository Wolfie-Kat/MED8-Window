using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneNavButtons : MonoBehaviour
{
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    void Awake()
    {
        leftButton.onClick.AddListener(OnLeftClicked);
        rightButton.onClick.AddListener(OnRightClicked);
    }
    void Start()
    {   
        DontDestroyOnLoad(gameObject);
        UpdateButtonVisibility();
    }

    private void OnLeftClicked()
    {
        Debug.Log("Left button clicked");
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.GoToPreviousScene();
    }

    private void OnRightClicked()
    {
        Debug.Log("Right button clicked");
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.GoToNextScene();
    }

    private void UpdateButtonVisibility()
    {
        int current = SceneManager.GetActiveScene().buildIndex;
        int total = SceneManager.sceneCountInBuildSettings;

        leftButton.gameObject.SetActive(current > 0);
        rightButton.gameObject.SetActive(current < total - 1);
    }
}