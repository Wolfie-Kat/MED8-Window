using UnityEngine;

public class TransitionCanvasLoader : MonoBehaviour
{
    [SerializeField] private GameObject transitionCanvasPrefab;

    void Awake()
    {
        if (SceneTransitionManager.Instance == null)
            Instantiate(transitionCanvasPrefab);
    }
}