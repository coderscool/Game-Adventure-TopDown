using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    public static LoadingUI Instance { get; private set; }

    [FormerlySerializedAs("root")]
    [SerializeField] private GameObject _root;

    [FormerlySerializedAs("progressBar")]
    [SerializeField] private Slider _progressBar;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (_progressBar == null)
            _progressBar = GetComponentInChildren<Slider>();

        Hide();
    }

    public void Show()
    {
        _root.SetActive(true);
        UpdateProgress(0f);
    }

    public void Hide()
    {
        _root.SetActive(false);
    }

    public void UpdateProgress(float value)
    {
        _progressBar.value = value;
    }
}
