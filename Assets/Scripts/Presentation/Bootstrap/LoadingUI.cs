using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    public static LoadingUI Instance;

    public GameObject root;
    public Slider progressBar;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Hide();
    }

    public void Show()
    {
        root.SetActive(true);
        UpdateProgress(0f);
    }

    public void Hide()
    {
        root.SetActive(false);
    }

    public void UpdateProgress(float value)
    {
        progressBar.value = value;
    }
}