using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    public static LoadingUI Instance;

    public GameObject root;
    public Slider progressBar;

    void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        if (progressBar == null)
            progressBar = GetComponentInChildren<Slider>();

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