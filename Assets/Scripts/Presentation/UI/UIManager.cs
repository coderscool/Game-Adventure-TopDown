using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject inventoryCanvas;
    public GameObject systemCanvas;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CloseAll();
        DontDestroyOnLoad(gameObject);
    }

    public void CloseAll()
    {
        inventoryCanvas.SetActive(false);
        systemCanvas.SetActive(false);
    }

    public void Open(GameObject canvas)
    {
        Toggle(canvas);
    }

    void Toggle(GameObject canvas)
    {
        bool isActive = canvas.activeSelf;
        CloseAll();
        canvas.SetActive(!isActive);
    }
}
